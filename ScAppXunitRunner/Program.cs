using System;
using System.Threading;
using Xunit.Runners;
using Starcounter;

namespace ScAppXunitRunner
{
    class Program
    {
        // We use consoleLock because messages can arrive in parallel, so we want to make sure we get
        // consistent console output.
        static object consoleLock = new object();

        // Use an event to know when we're done
        static ManualResetEvent finished = new ManualResetEvent(false);

        // Start out assuming success; we'll set this to 1 if we get a failed test
        static int result = 0;

        static Starcounter.Logging.LogSource log = new Starcounter.Logging.LogSource("ScAppXunitRunner");

        static void Main()
        {
            Handle.GET("/runtests", () =>
            {
                //string fullName = "ClassLibraryXunitTest.dll";
                //string path = System.IO.Directory.GetCurrentDirectory();
                string path = @"C:\GitHub\home\Starcounter2.3_xunit\ConsoleAppXunitRunner\bin\Debug";
                string fullName = path + "\\ClassLibraryXunitTest.dll"; //full path is needed
                string typeName = null;

                using (var runner = AssemblyRunner.WithAppDomain(fullName))
                {
                    runner.OnDiscoveryComplete = OnDiscoveryComplete;
                    runner.OnExecutionComplete = OnExecutionComplete;
                    runner.OnTestFailed = OnTestFailed;
                    runner.OnTestSkipped = OnTestSkipped;

                    Console.WriteLine("Discovering...");
                    runner.Start(typeName);

                    finished.WaitOne();
                    finished.Dispose();

                    string message = "Tests has been run with exit code: " + result;

                    log.Debug(message);

                    return message;
                }
            });
        }

        static void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            lock (consoleLock)
            {
                log.Debug($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
            }
        }

        static void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (consoleLock)
            { 
                log.Debug($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
            }

            finished.Set();
        }

        static void OnTestFailed(TestFailedInfo info)
        {
            lock (consoleLock)
            {
                //Console.ForegroundColor = ConsoleColor.Red;

                log.Debug("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
                if (info.ExceptionStackTrace != null)
                    log.Debug(info.ExceptionStackTrace);

                //Console.ResetColor();
            }

            result = 1;
        }

        static void OnTestSkipped(TestSkippedInfo info)
        {
            lock (consoleLock)
            {
                //Console.ForegroundColor = ConsoleColor.Yellow;
                log.Debug("[SKIP] {0}: {1}", info.TestDisplayName, info.SkipReason);
                //Console.ResetColor();
            }
        }
    }
}