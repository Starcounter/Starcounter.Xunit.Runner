using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit.Runners;
using Starcounter;
using System.Reflection;

namespace ScXunitRunner
{
    public class StarcounterXunitRunner
    {
        private readonly string assemblyName;
        private readonly string assemblyLocation;

        public StarcounterXunitRunner()
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            assemblyName = assembly.GetName().Name;
            assemblyLocation = assembly.Location;
            AddHandler();
        }

        private void AddHandler()
        {
            Handle.GET("/ScXunitRunner/" + assemblyName, () =>
            {
                string typeName = null;

                using (AssemblyRunner runner = AssemblyRunner.WithoutAppDomain(assemblyLocation))
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
