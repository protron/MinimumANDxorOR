using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    private static ConcurrentDictionary<string, Progress> progressDict = new ConcurrentDictionary<string, Progress>();

    public static void Main()
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        var allCases = Directory.GetFiles("ExpectedResultsAndTestCases", "*.clean.txt");
        var query = from file in allCases.AsParallel()
                    select RunCase(file);
        var progressMonitor = Task.Factory.StartNew(async () => {
            while (!token.IsCancellationRequested)
            {
                foreach (var x in progressDict)
                {
                    var v = x.Value;
                    Write(ConsoleColor.White, $"{x.Key}: {v.ProcessedRecords} + {v.RecordsLeft} = {v.TotalRecords}. {v.ElapsedTime:mm':'ss}");
                }
                await Task.Delay(1000, token);
            }
        });
        foreach (var result in query)
        {
            if (result.IsSuccess)
            {
                Write(ConsoleColor.Green, $"{result.Id} Success! {result.ElapsedTime:mm':'ss}"); // \nOutput:\n{result.Actual}
            }
            else
            {
                Write(ConsoleColor.Red, $"{result.Id} ERROR! {result.ElapsedTime:mm':'ss} {result.Error}"); // \nActualOutput:\n{result.Actual}.\n\nExpected:\n{result.Expected}.
            }
        }
        cancellationTokenSource.Cancel();
        progressMonitor.Wait();
    }

    static void Write(ConsoleColor color, string s)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(s);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    private static Result RunCase(string pathTestCase)
    {
        var expectedResultsPath = pathTestCase.Remove(pathTestCase.Length - ".clean.txt".Length);
        var id = Path.GetFileNameWithoutExtension(expectedResultsPath);
        try
        {
            var pathToExe = (typeof(Solution).Module.FullyQualifiedName)[..^4] + ".exe";
            string processFileName = Path.GetFullPath(pathToExe);
            var workingDirectory = Path.GetDirectoryName(processFileName);
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = processFileName,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                };
                process.Start();
                var sw = Stopwatch.StartNew();
                Progress progress;
                var expectedResultsReader = new StreamReader(expectedResultsPath);
                bool isSuccess = true;
                string error = "";
                using (var actualResultsWriter = new StreamWriter(expectedResultsPath + ".actual.txt"))
                {
                    var outputBufferedLines = 0;
                    void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
                    {
                        if (string.IsNullOrEmpty(e.Data))
                            return;
                        var actualLine = e.Data;
                        if (actualResultsWriter is null)
                            return;
                        actualResultsWriter.WriteLine(actualLine);
                        if (++outputBufferedLines > 50)
                        {
                            actualResultsWriter.Flush();
                            outputBufferedLines = 0;
                        }
                        var expectedLine = expectedResultsReader.ReadLine();
                        if (actualLine != expectedLine)
                        {
                            error = $"actualLine != expectedLine: {actualLine} != {expectedLine}.";
                            isSuccess = false;
                        }
                    }
                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.BeginOutputReadLine();
                    using (FileStream reader = File.OpenRead(pathTestCase))
                    {
                        var bufferSize = Math.Min(81920, reader.Length);
                        var buffer = new byte[bufferSize];
                        int readSize;
                        while ((readSize = reader.Read(buffer)) != 0)
                        {
                            progress = new Progress(reader.Length, reader.Position, sw.Elapsed);
                            progressDict.AddOrUpdate(id, progress, (oid, ov) => progress);
                            process.StandardInput.BaseStream.Write(buffer, 0, readSize);
                        }
                    }
                    actualResultsWriter.Flush();
                    process.StandardInput.Close();
                    process.WaitForExit();
                }
                sw.Stop();
                progressDict.TryRemove(id, out _);
                return new Result(id, isSuccess, sw.Elapsed, error);
            }
        }
        catch (Exception ex)
        {
            progressDict.TryRemove(id, out _);
            return new Result(id, false, TimeSpan.Zero, ex.ToString());
        }
    }

    public record Result(string Id, bool IsSuccess, TimeSpan ElapsedTime, string Error)
    {
    }

    public record Progress(long TotalRecords, long ProcessedRecords, TimeSpan ElapsedTime)
    {
        public long RecordsLeft = TotalRecords - ProcessedRecords;
    }
}