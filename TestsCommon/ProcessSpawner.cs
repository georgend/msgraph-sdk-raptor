using System.Diagnostics;

namespace TestsCommon;

public static class ProcessSpawner
{
    // create a static method that spawns a process and waits for a timeout and returns stdout and stderr as a tuple
    public static async Task<(string stdout, string stderr)> SpawnProcess(string command, string arguments, string workingDirectory, int timeout)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments);
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;
        startInfo.WorkingDirectory = workingDirectory;
        using Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit(timeout);
        var stdout = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var stderr = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
        return (stdout, stderr);
    }
}
