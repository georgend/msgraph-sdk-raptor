using System.Diagnostics;
public class ProcessSpawner
{
    // create a static method that spawns a process and waits for a timeout and returns stdout and stderr as a tuple
    public static (string stdout, string stderr) SpawnProcess(string command, string arguments, int timeout)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments);
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit(timeout);
        return (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
    }
}
