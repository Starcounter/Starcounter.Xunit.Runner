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
        private readonly bool triggerOnInstanceCreation;
        private readonly bool createUrlHandler;

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
        /// <param name="createUrlHandler">
        ///     If true (default: false): a Url handler, <see cref="Starcounter.Handle.GET"/>, will be created for executing the tests withing the Calling Assembly.
        /// </param>
        /// <param name="childUrlPath">
        ///     Overrides the child part of the <see cref="XunitRunnerUrl"/> string.
        ///     If null (default: null), then the calling assembly name will be set, i.e. "/ScXunitRunner/<CallingAssemblyName>".
        ///     Will always be null if <see cref="createUrlHandler"/> is false.
        /// </param>
        public StarcounterXunitRunner(bool triggerOnInstanceCreation = false, bool createUrlHandler = false, string childUrlPath = null, Func<Xunit.Abstractions.ITestCase, bool> testCaseFilter = null)
        {
            this.triggerOnInstanceCreation = triggerOnInstanceCreation;
            this.createUrlHandler = createUrlHandler;
            this.TestCaseFilter = testCaseFilter;

            Assembly assembly = Assembly.GetCallingAssembly();
            this.assemblyLocation = assembly.Location;
            
            if (this.triggerOnInstanceCreation)
            {
                Start();
            }

            if (this.createUrlHandler)
            {
                this.assemblyName = childUrlPath ?? assembly.GetName().Name;
                this.XunitRunnerUrl = rootHandler + "/" + assemblyName;
                AddHandler();
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

        private void AddHandler()
        {
            Handle.GET(this.XunitRunnerUrl, () =>
            {
                return ExecuteTests();
            });
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
                    if (count > 30)
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
