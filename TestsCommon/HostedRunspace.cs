using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

using Microsoft.PowerShell;

namespace TestsCommon;

/// <summary>
///     Contains functionality for executing PowerShell scripts.
/// </summary>
public static class HostedRunspace
{
    public readonly record struct PsCommand(string Command, Dictionary<string, object> Parameters);
    public readonly record struct PsExecutionResult(bool HadErrors, List<ErrorRecord> ErrorRecords, PSDataCollection<PSObject> Results);
    private static InitialSessionState CreateDefaultState()
    {
        var currentSessionState = InitialSessionState.CreateDefault2();
        currentSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;
        currentSessionState.LanguageMode = PSLanguageMode.FullLanguage;
        currentSessionState.ApartmentState = ApartmentState.STA;
        currentSessionState.ThreadOptions = PSThreadOptions.Default;
        currentSessionState.ImportPSModule("Microsoft.Graph.Authentication");
        return currentSessionState;
    }
    public static async Task<PsExecutionResult> FindMgGraphCommand(string command, string apiVersion, Func<string, Task> output)
    {
        var findMgGraphCommand = new PsCommand(Command: "Find-MgGraphCommand", Parameters: new Dictionary<string, object> { { "Command", command }, { "ApiVersion", apiVersion } });

        var findMgGraphCommandResult = await RunScript(new List<PsCommand> { findMgGraphCommand }, output, String.Empty)
            .ConfigureAwait(false);
        return findMgGraphCommandResult;
    }

    public static async Task<PsExecutionResult> RunScript(List<PsCommand> commands,
        Func<string, Task> output,
        string scriptContents,
        Scope currentScope = default)
    {
        if (commands == null)
        {
            throw new ArgumentNullException(nameof(commands));
        }
        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }
        var currentState = CreateDefaultState();
        using var ps = PowerShell.Create(currentState);
        foreach (var (command, dictionary) in commands)
        {
            ps.AddStatement()
                .AddCommand(command, true)
                .AddParameters(dictionary);
        }

        if (!string.IsNullOrWhiteSpace(scriptContents))
        {
            ps.AddStatement()
                .AddScript(scriptContents, true);
        }

        async void OnErrorOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (sender is PSDataCollection<ErrorRecord> streamObjectsReceived)
            {
                var streamObjectsList = streamObjectsReceived.ToList();
                var currentStreamRecord = streamObjectsList[e.Index];
                await output($@"ErrorStreamEvent: {currentStreamRecord.Exception.Message}  Current Scope: {currentScope?.value}");
            }
        }

        async void OnWarningOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (sender is PSDataCollection<WarningRecord> streamObjectsReceived)
            {
                var streamObjectsList = streamObjectsReceived.ToList();
                var currentStreamRecord = streamObjectsList[e.Index];
                await output($"WarningStreamEvent: {currentStreamRecord.Message}");
            }
        }

        async void OnInformationOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (sender is PSDataCollection<InformationRecord> streamObjectsReceived)
            {
                var streamObjectsList = streamObjectsReceived.ToList();
                var currentStreamRecord = streamObjectsList?[e.Index];
                await output($"InfoStreamEvent: {currentStreamRecord?.MessageData}");
            }
        }
        ps.Streams.Error.DataAdded += OnErrorOnDataAdded;
        ps.Streams.Warning.DataAdded += OnWarningOnDataAdded;
        ps.Streams.Information.DataAdded += OnInformationOnDataAdded;

        // execute the script and await the result.
        var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);
        var executionErrors = ps.Streams.Error.ToList();

        return new PsExecutionResult(ps.HadErrors, executionErrors, pipelineObjects);
    }
}
