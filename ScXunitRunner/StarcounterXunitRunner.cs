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
        private const string rootHandler = "/ScXunitRunner";
        private readonly string assemblyName;
        private readonly string assemblyLocation;

        /// <summary>
        /// Url for executing tests
        /// </summary>
        public string XunitRunnerUrl { get { return rootHandler + "/" + assemblyName; } }

        /// <summary>
        /// Creates a <see cref="Starcounter.Handle.GET"/> for executing Xunit tests which is located in the calling assembly.
        /// The test execution will take place in the same AppDomain as the Starcounter database.
        /// </summary>
        /// <param name="urlEnding">
        ///     Overrides the child part of the <see cref="XunitRunnerUrl"/>.
        ///     If null, then the calling assembly name will be set.
        /// </param>
        public StarcounterXunitRunner(string urlEnding = null)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            if (urlEnding == null)
            {
                assemblyName = assembly.GetName().Name;
            }
            else
            {
                assemblyName = urlEnding;
            }

            assemblyLocation = assembly.Location;
            AddHandler();
        }

        private void AddHandler()
        {
            Handle.GET(XunitRunnerUrl, () =>
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
