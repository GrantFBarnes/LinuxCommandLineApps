using System.Diagnostics;

public sealed class Command
{
    private readonly string _fileName;
    private readonly string _arguments;
    private string _workingDirectory;
    private bool _hideOutput;
    private bool _waitForExit;

    public Command(string text)
    {
        text = text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            throw new Exception("no command provided");
        }

        var textSplit = text.Split(" ");
        _fileName = textSplit.First();
        _arguments = string.Join(" ", textSplit.Skip(1));

        _workingDirectory = string.Empty;
        _hideOutput = false;
        _waitForExit = true;
    }

    public Command WorkingDirectory(string directory)
    {
        _workingDirectory = directory;
        return this;
    }

    public Command HideOutput(bool hide)
    {
        _hideOutput = hide;
        return this;
    }

    public Command WaitForExit(bool wait)
    {
        _waitForExit = wait;
        return this;
    }

    public void Run()
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = _fileName,
            Arguments = _arguments,
        };

        if (!string.IsNullOrEmpty(_workingDirectory))
        {
            startInfo.WorkingDirectory = _workingDirectory;
        }

        if (_hideOutput)
        {
            startInfo.RedirectStandardOutput = true;
        }

        var process = new Process()
        {
            StartInfo = startInfo
        };

        process.Start();

        if (_waitForExit)
        {
            process.WaitForExit();
        }
    }

    public string GetOutput()
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = _fileName,
            Arguments = _arguments,
            RedirectStandardOutput = true,
        };

        if (!string.IsNullOrEmpty(_workingDirectory))
        {
            startInfo.WorkingDirectory = _workingDirectory;
        }

        var process = new Process()
        {
            StartInfo = startInfo
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }

    public bool Exists()
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "command",
                Arguments = $"-v {_fileName}",
                RedirectStandardOutput = true,
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return !string.IsNullOrEmpty(output);
    }
}