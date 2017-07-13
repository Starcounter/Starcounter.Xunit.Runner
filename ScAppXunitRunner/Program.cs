using System;
using System.Threading;
using Xunit.Runners;
using Starcounter;

namespace ScAppXunitRunner
{
    class Program
    {
        static void Main()
        {
            Handle.GET("/runtests", () =>
            {
                //string fullName = "ClassLibraryXunitTest.dll";
                //string path = System.IO.Directory.GetCurrentDirectory();
                string path = @"C:\GitHub\home\Starcounter2.3_xunit\ScAppXunitRunner\bin\Debug";
                string fullName = path + "\\ClassLibraryXunitTest.dll"; //full path is needed
                string typeName = null;

                using (AssemblyRunner runner = AssemblyRunner.WithAppDomain(fullName)) // TODO, test WithoutAppDomain();
                {
                    TestFramework testFramework = new TestFramework();
                    runner.OnDiscoveryComplete = testFramework.OnDiscoveryComplete;
                    runner.OnExecutionComplete = testFramework.OnExecutionComplete;
                    runner.OnTestStarting = testFramework.OnTestStarting;
                    runner.OnTestFailed = testFramework.OnTestFailed;
                    runner.OnTestSkipped = testFramework.OnTestSkipped;
                    runner.OnTestPassed = testFramework.OnTestPassed;
                    runner.OnTestFinished = testFramework.OnTestFinished;

                    int count = 0;
                    while (runner.Status != AssemblyRunnerStatus.Idle)
                    {
                        if (count > 30)
                        {
                            testFramework.finished.Dispose();
                            return "Timeout";
                        }

                        Thread.Sleep(1000);
                        count++;
                    }

                    runner.Start(typeName);

                    testFramework.finished.WaitOne();
                    testFramework.finished.Dispose();

                    return testFramework.ToString();
                }
            });
        }
    }
}