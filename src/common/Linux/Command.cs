using System.Diagnostics;

public sealed class Command
{
    private readonly string _fileName;
    private readonly string _arguments;
    private bool _showOutput;
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
        _showOutput = false;
        _waitForExit = true;
    }

    public Command ShowOutput(bool show)
    {
        _showOutput = show;
        return this;
    }

    public Command WaitForExit(bool wait)
    {
        _waitForExit = wait;
        return this;
    }

    public void Run()
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = _fileName,
                Arguments = _arguments,
                RedirectStandardOutput = !_showOutput,
            }
        };

        process.Start();

        if (_waitForExit)
        {
            process.WaitForExit();
        }
    }

    public string GetOutput()
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = _fileName,
                Arguments = _arguments,
                RedirectStandardOutput = true,
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }
}