using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit.Runners;

namespace ScXunitRunner
{
    public class StarcounterXunitRunner
    {
        private readonly string assemblyLocation;
        private readonly bool triggerOnInstanceCreation;

        // lock for not execute multiple runs at the same time
        private object testExecutionLock = new object();

        /// <summary>
        ///     Url for executing tests
        /// </summary>
        public string XunitRunnerUrl { get; private set; } = null;

        /// <summary>
        ///     Set to be able to filter the test cases to decide which ones to run. 
        ///     If this is not set, then all test cases will be run.
        /// </summary>
        public Func<Xunit.Abstractions.ITestCase, bool> TestCaseFilter { get; set; }

        /// <summary>
        ///     A Xunit runner for executing tests from the calling assembly in the same AppDomain as the hosted Starcounter database.
        ///     This runner should be created inside a Starcounter Application.
        /// </summary>
        /// <param name="triggerOnInstanceCreation">
        ///     If true (default: false): tests will be executed on this instance creation.
        /// </param>
        /// <param name="testCaseFilter">
        ///     Set to be able to filter the test cases to decide which ones to run. 
        ///     If this is not set, then all test cases will be run.
        /// </param>
        public StarcounterXunitRunner(bool triggerOnInstanceCreation = false, Func<Xunit.Abstractions.ITestCase, bool> testCaseFilter = null)
        {
            this.triggerOnInstanceCreation = triggerOnInstanceCreation;
            this.TestCaseFilter = testCaseFilter;

            Assembly assembly = Assembly.GetCallingAssembly();
            this.assemblyLocation = assembly.Location;
            
            if (this.triggerOnInstanceCreation)
            {
                Start();
            }
        }

        /// <summary>
        ///     Starts test execution.
        /// </summary>
        /// <param name="typeName">
        ///     If null (default: null): All tests will be run.
        ///     Otherwise only executing the tests within the typeName class i.e. typeName=<NameSpace>.<ClassName>.
        ///     <see cref="TestCaseFilter"/> will still be taken into account though.
        /// </param>
        public void Start(string typeName = null)
        {
            lock (testExecutionLock)
            {
                string output = ExecuteTests(typeName: typeName);
                Console.WriteLine(output);
            }
        }

        private string ExecuteTests(string typeName = null)
        {
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
                runner.TestCaseFilter = this.TestCaseFilter;

                int count = 0;
                while (runner.Status != AssemblyRunnerStatus.Idle)
                {
                    if (count > 20)
                    {
                        testFramework.finished.Dispose();
                        return "Timeout";
                    }

                    Thread.Sleep(1000);
                    count++;
                }

                runner.Start(typeName: typeName);

                testFramework.finished.WaitOne();
                testFramework.finished.Dispose();

                return testFramework.ToString();
            }
        }
    }
}
