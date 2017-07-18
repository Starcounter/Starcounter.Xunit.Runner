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

        /// <summary>
        ///     Url for executing tests
        /// </summary>
        public string XunitRunnerUrl { get; private set; } = null;

        /// <summary>
        /// A Xunit runner for executing tests from the calling assembly in the same AppDomain as the hosted Starcounter database.
        /// This runner should be created inside a Starcounter Application.
        /// </summary>
        /// <param name="triggerOnInstanceCreation">
        ///     If true (default: true): tests will be executed on this instance creation.
        /// </param>
        /// <param name="createUrlHandler">
        ///     If true (default: false): a Url handler, <see cref="Starcounter.Handle.GET"/>, will be created for executing the tests withing the Calling Assembly.
        /// </param>
        /// <param name="childUrlPath">
        ///     Overrides the child part of the <see cref="XunitRunnerUrl"/> string.
        ///     If null (default: null), then the calling assembly name will be set, i.e. "/ScXunitRunner/<CallingAssemblyName>".
        ///     Will always be null if <see cref="createUrlHandler"/> is false.
        /// </param>
        public StarcounterXunitRunner(bool triggerOnInstanceCreation = true, bool createUrlHandler = false, string childUrlPath = null)
        {
            this.triggerOnInstanceCreation = triggerOnInstanceCreation;
            this.createUrlHandler = createUrlHandler;

            Assembly assembly = Assembly.GetCallingAssembly();
            this.assemblyLocation = assembly.Location;
            
            if (this.triggerOnInstanceCreation)
            {
                string output = ExecuteTests();
                Console.WriteLine(output);
            }

            if (this.createUrlHandler)
            {
                this.assemblyName = childUrlPath ?? assembly.GetName().Name;
                this.XunitRunnerUrl = rootHandler + "/" + assemblyName;
                AddHandler();
            }
        }

        private void AddHandler()
        {
            Handle.GET(this.XunitRunnerUrl, () =>
            {
                return ExecuteTests();
            });
        }

        private string ExecuteTests()
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
        }
    }
}
