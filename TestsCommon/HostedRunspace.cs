using System.Collections.Concurrent;
using System.Dynamic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;

namespace TestsCommon;

/// <summary>
///     Contains functionality for executing PowerShell scripts.
/// </summary>
public class HostedRunspace : IDisposable
{
    private static InitialSessionState _defaultSessionState;

    private HostedRunspace(RunspacePool rsPool)
    {
        RsPool = rsPool;
    }

    /// <summary>
    ///     The PowerShell runspace pool.
    /// </summary>
    private RunspacePool RsPool
    {
        get;
    }

    /// <summary>
    ///     Initialize the runspace pool.
    /// </summary>
    /// <param name="minRunspaces"></param>
    /// <param name="maxRunspaces"></param>
    public static HostedRunspace InitializeRunspaces(int minRunspaces, int maxRunspaces, params string[] modulesToLoad)
    {
        // create the default session state.
        // session state can be used to set things like execution policy, language constraints, etc.
        // optionally load any modules (by name) that were supplied.
        var raptorConfig = TestsSetup.Config;
        _defaultSessionState = InitialSessionState.CreateDefault2();
        _defaultSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;
        _defaultSessionState.LanguageMode = PSLanguageMode.FullLanguage;
        _defaultSessionState.ApartmentState = ApartmentState.MTA;
        _defaultSessionState.EnvironmentVariables.Add(new List<SessionStateVariableEntry>()
        {
            new("TenantID", raptorConfig.Value.TenantID, "Default Tenant Identifier", ScopedItemOptions.AllScope),
            new("ClientID", raptorConfig.Value.ClientID, "Default Client Identifier", ScopedItemOptions.AllScope),
            new("EducationTenantID", raptorConfig.Value.EducationTenantID, "Education Tenant Identifier", ScopedItemOptions.AllScope),
            new("EducationClientID", raptorConfig.Value.EducationClientID, "Education Client Identifier", ScopedItemOptions.AllScope),
        });
        _defaultSessionState.Variables.Add(new List<SessionStateVariableEntry>()
        {
            new("Certificate", raptorConfig.Value.Certificate.Value, "Authentication Certificate", ScopedItemOptions.AllScope)
        });
        _defaultSessionState.ImportPSModule(modulesToLoad);
        // use the runspace factory to create a pool of runspaces
        // with a minimum and maximum number of runspaces to maintain.
        var rsPool = RunspaceFactory.CreateRunspacePool(_defaultSessionState);
        rsPool.SetMinRunspaces(minRunspaces);
        rsPool.SetMaxRunspaces(maxRunspaces);
        // set the pool options for thread use.
        // we can throw away or re-use the threads depending on the usage scenario.

        rsPool.ThreadOptions = PSThreadOptions.UseNewThread;


        // open the pool. 
        // this will start by initializing the minimum number of runspaces.
        rsPool.Open();
        var hostedRunspace = new HostedRunspace(rsPool);
        return hostedRunspace;
    }

    /// <summary>
    ///     Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output.
    /// </summary>
    /// <param name="scriptContents">The script file contents.</param>
    /// <param name="scriptParameters">A dictionary of parameter names and parameter values.</param>
    /// <param name="output"></param>
    public async Task<(bool HadErrors, ConcurrentQueue<ErrorRecord> ErrorRecords, PSDataCollection<PSObject> Results)> RunScript(string scriptContents,
        Dictionary<string, object> scriptParameters,
        Func<string, Task> output,
        Scope currentScope = default)
    {
        if (scriptContents == null)
        {
            throw new ArgumentNullException(nameof(scriptContents));
        }

        if (scriptParameters == null)
        {
            throw new ArgumentNullException(nameof(scriptParameters));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (RsPool == null)
        {
            throw new ArgumentException("Runspace Pool must be initialized before calling RunScript().");
        }

        // create a new hosted PowerShell instance using a custom runspace.
        // wrap in a using statement to ensure resources are cleaned up.
        var currentState = _defaultSessionState.Clone();
        if (scriptParameters.Any())
        {
            currentState.Variables.Add(new List<SessionStateVariableEntry>()
            {
                new("AccessToken", scriptParameters["AccessToken"], "Current AccessToken",
                    ScopedItemOptions.AllScope)
            });
            currentState.EnvironmentVariables.Add(new List<SessionStateVariableEntry>()
            {
                new("AccessToken", scriptParameters["AccessToken"], "Current AccessToken",
                    ScopedItemOptions.AllScope)
            });
        }

        using var ps = PowerShell.Create(currentState);
        // use the runspace pool.
        ps.RunspacePool = RsPool;
        // specify the script code to run.
        ps.AddScript(scriptContents);

        // subscribe to events from some of the streams
        var errors = new ConcurrentQueue<ErrorRecord>();
        async void OnErrorOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (sender is PSDataCollection<ErrorRecord> streamObjectsReceived)
            {
                var streamObjectsList = streamObjectsReceived.ToList();
                var currentStreamRecord = streamObjectsList[e.Index];
                errors.Enqueue(currentStreamRecord);
                await output($@"ErrorStreamEvent: {currentStreamRecord.Exception.Message}  Current Scope: {currentScope?.value}").ConfigureAwait(false);
            }
        }

        async void OnWarningOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (sender is PSDataCollection<WarningRecord> streamObjectsReceived)
            {
                var streamObjectsList = streamObjectsReceived.ToList();
                var currentStreamRecord = streamObjectsList[e.Index];
                await output($"WarningStreamEvent: {currentStreamRecord.Message}").ConfigureAwait(false);
            }
        }

        async void OnInformationOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (sender is PSDataCollection<InformationRecord> streamObjectsReceived)
            {
                var streamObjectsList = streamObjectsReceived.ToList();
                var currentStreamRecord = streamObjectsList?[e.Index];
                await output($"InfoStreamEvent: {currentStreamRecord?.MessageData}").ConfigureAwait(false);
            }
        }

        ps.Streams.Error.DataAdded += OnErrorOnDataAdded;
        ps.Streams.Warning.DataAdded += OnWarningOnDataAdded;
        ps.Streams.Information.DataAdded += OnInformationOnDataAdded;

        // execute the script and await the result.
        var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);
        return (ps.HadErrors, errors, pipelineObjects);
    }

    private static string PrintJson(dynamic psData)
    {
        var hasJsonStringMethod = HasMethod(psData, "ToJsonString");
        return hasJsonStringMethod ? (string)psData.ToJsonString() : (string)psData.ToString();
    }
    private static bool HasMethod(dynamic obj, string name)
    {
        Type objType = obj.GetType();
        return objType.GetMethod(name) != null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            RsPool?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~HostedRunspace()
    {
        Dispose(false);
    }
}
