using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Runners;

// nuget xunit
// nuget xunit.runner.utility

namespace ConsoleAppXunitRunner
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

        static int Main(string[] args)
        {
            //string fullName = "ClassLibraryXunitTest.dll";
            string path = System.IO.Directory.GetCurrentDirectory();
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

                return result;
            }
        }

        static void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            lock (consoleLock)
                Console.WriteLine($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
        }

        static void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (consoleLock)
                Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");

            finished.Set();
        }

        static void OnTestFailed(TestFailedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
                if (info.ExceptionStackTrace != null)
                    Console.WriteLine(info.ExceptionStackTrace);

                Console.ResetColor();
            }

            result = 1;
        }

        static void OnTestSkipped(TestSkippedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[SKIP] {0}: {1}", info.TestDisplayName, info.SkipReason);
                Console.ResetColor();
            }
        }
    }
}
