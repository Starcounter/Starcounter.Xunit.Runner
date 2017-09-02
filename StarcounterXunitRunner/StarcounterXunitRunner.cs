using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runners;
using XunitAbstractions = Xunit.Abstractions;
using XunitReporters = Xunit.Runner.Reporters;

namespace Starcounter.Xunit.Runner
{
    public class StarcounterXunitRunner
    {
        private readonly string assemblyLocation;
        private readonly string assebmlyName;

        // lock for not execute multiple runs at the same time
        private object testExecutionLock = new object();

        /// <summary>
        ///     Set to be able to filter the test cases to decide which ones to run. 
        ///     If this is not set, then all test cases will be run.
        /// </summary>
        public Func<XunitAbstractions.ITestCase, bool> TestCaseFilter
        {
            get
            {
                return testCaseFilter ?? ((testCasee) => true);
            }
            set
            {
                testCaseFilter = value;
            }
        }
        private Func<XunitAbstractions.ITestCase, bool> testCaseFilter;

        /// <summary>
        ///     Set to true (default: true) to run test collections in parallel; set to false to run them sequentially.
        /// </summary>
        public bool RunTestsInParallel { get; set; }

        /// <summary>
        ///     Set to true (default: false) to print executed test cases to console, including any test case stack trace.
        ///     Note that by doing this the tests will be run in sequence i.e. RunTestsInParallel will be overridden to false!
        /// </summary>
        public bool DeveloperMode { get; set; }

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
        /// <param name="runTestsInParallel">
        ///     Set to true (default: false) to run test collections in parallel; set to false to run them sequentially.
        /// </param>
        /// <param name="developerMode">
        ///     Set to true (default: false) to print executed test cases to console, including any test case stack trace.
        ///     Note that by doing this the tests will be run in sequence i.e. RunTestsInParallel will be overridden to false!
        /// </param>
        public StarcounterXunitRunner(bool triggerOnInstanceCreation = false, Func<XunitAbstractions.ITestCase, bool> testCaseFilter = null, bool runTestsInParallel = false, bool developerMode = false)
        {
            if (testCaseFilter == null)
            {
                testCaseFilter = (testCase) => true;
            }
            this.TestCaseFilter = testCaseFilter;
            this.RunTestsInParallel = runTestsInParallel;
            this.DeveloperMode = developerMode;

            Assembly assembly = Assembly.GetCallingAssembly();
            this.assemblyLocation = assembly.Location;
            this.assebmlyName = assembly.GetName().Name;

            if (triggerOnInstanceCreation)
            {
                Start();
            }
        }

        /// <summary>
        ///     Starts test collection execution.
        /// </summary>
        /// <param name="typeName">
        ///     If null (default: null): All tests will be run.
        ///     Otherwise only executing the tests within the typeName class i.e. typeName="NameSpace.ClassName".
        ///     <see cref="TestCaseFilter"/> will still be taken into account though.
        /// </param>
        /// <param name="testReportName">
        ///     If null (default: null): No test report will be generated. 
        ///     Set the desired name (FullPath or only FileName) to generate test report, file type is not needed. 
        /// </param>
        public void Start(string typeName = null, string testReportName = null)
        {
            lock (testExecutionLock)
            {
                ExecuteTests(typeName: typeName, testReportName: testReportName);
            }
        }

        /// <summary>
        ///     <see cref="StarcounterXunitRunner.Start(string, string)"/> for description.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="testReportName"></param>
        private void ExecuteTests(string typeName = null, string testReportName = null)
        {
            ExecutionSummary executionSummary = null;

            XunitProjectAssembly assembly = new XunitProjectAssembly
            {
                AssemblyFilename = assemblyLocation,
                ConfigFilename = null
            };

            XElement assembliesElement = new XElement("assemblies");
            XElement assemblyElement = new XElement("assembly");

            // Logger
            var verboserReporter = new XunitReporters.VerboseReporter();
            IRunnerLogger logger = new ConsoleRunnerLogger(useColors: true);
            IMessageSinkWithTypes reporterMessageHandler = MessageSinkWithTypesAdapter.Wrap(verboserReporter.CreateMessageHandler(logger));

            // Option setup
            ITestFrameworkDiscoveryOptions discoveryOptions = TestFrameworkOptions.ForDiscovery(null);
            ITestFrameworkExecutionOptions executionOptions = TestFrameworkOptions.ForExecution(null);
            executionOptions.SetSynchronousMessageReporting(true);
            executionOptions.SetDisableParallelization(DeveloperMode || !RunTestsInParallel);
            executionOptions.SetDiagnosticMessages(true);

            var assemblyDisplayName = Path.GetFileNameWithoutExtension(assembly.AssemblyFilename);
            var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
            var shadowCopy = assembly.Configuration.ShadowCopyOrDefault;

            var clockTime = Stopwatch.StartNew();
            bool cancel = false;
            using (var controller = new XunitFrontController(
                appDomainSupport: AppDomainSupport.Denied,
                assemblyFileName: assembly.AssemblyFilename,
                configFileName: null,
                shadowCopy: shadowCopy,
                shadowCopyFolder: null,
                sourceInformationProvider: null,
                diagnosticMessageSink: null))
            using (var discoverySink = new TestDiscoverySink(() => cancel))
            {
                // Discover & filter the tests
                reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryStarting(
                    assembly: assembly,
                    appDomain: controller.CanUseAppDomains && appDomainSupport != AppDomainSupport.Denied,
                    shadowCopy: shadowCopy,
                    discoveryOptions: discoveryOptions));

                if (typeName != null)
                {
                    controller.Find(typeName, false, discoverySink, discoveryOptions);
                }
                else
                {
                    controller.Find(false, discoverySink, discoveryOptions);
                }
                discoverySink.Finished.WaitOne();

                var testCasesDiscovered = discoverySink.TestCases.Count;
                var filteredTestCases = discoverySink.TestCases.Where(TestCaseFilter).ToList();
                var testCasesToRun = filteredTestCases.Count;

                reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryFinished(assembly, discoveryOptions, testCasesDiscovered, testCasesToRun));

                // Run the filtered tests
                if (testCasesToRun == 0)
                {
                    executionSummary = new ExecutionSummary();
                }
                else
                {
                    reporterMessageHandler.OnMessage(new TestAssemblyExecutionStarting(assembly, executionOptions));

                    IExecutionSink resultsSink = new DelegatingExecutionSummarySink(reporterMessageHandler, () => cancel, (path, summary) => { executionSummary = summary; });
                    if (assemblyElement != null)
                    {
                        resultsSink = new DelegatingXmlCreationSink(resultsSink, assemblyElement);
                    }

                    controller.RunTests(filteredTestCases, resultsSink, executionOptions);
                    resultsSink.Finished.WaitOne();

                    reporterMessageHandler.OnMessage(new TestAssemblyExecutionFinished(assembly, executionOptions, resultsSink.ExecutionSummary));
                    assembliesElement.Add(assemblyElement);
                }
            }

            clockTime.Stop();

            assembliesElement.Add(new XAttribute("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

            if (executionSummary != null)
            {
                Console.WriteLine();

                KeyValuePair<string, ExecutionSummary> kvpExecutionSummary = new KeyValuePair<string, ExecutionSummary>(this.assebmlyName, executionSummary);

                reporterMessageHandler.OnMessage(new TestExecutionSummary(clockTime.Elapsed, new List<KeyValuePair<string, ExecutionSummary>> { kvpExecutionSummary }));

                if (testReportName != null)
                {
                    // Create folder if it does not exist
                    FileInfo fi = new FileInfo(testReportName);
                    DirectoryInfo directory = fi.Directory;
                    if (!directory.Exists)
                    {
                        Directory.CreateDirectory(directory.FullName);
                    }

                    CreateXmlTestReport(assembliesElement, fi);
                    CreateHtmlTestReport(assembliesElement, fi);
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Creates directory and generates Xml TestReport
        /// </summary>
        /// <param name="assembliesElement"></param>
        /// <param name="testReportFileInfo"></param>
        private void CreateXmlTestReport(XElement assembliesElement, FileInfo testReportFileInfo)
        {
            string fullPath = testReportFileInfo.FullName;

            if (!fullPath.EndsWith(".xml"))
            {
                fullPath += ".xml";
            }

            using (var stream = File.OpenWrite(fullPath))
            {
                assembliesElement.Save(stream);
                Console.WriteLine($"   Test report generated: {stream.Name}");
            }
        }

        /// <summary>
        /// Creates directory and generates html TestReport
        /// </summary>
        /// <param name="assembliesElement"></param>
        /// <param name="testReportFileInfo"></param>
        private void CreateHtmlTestReport(XElement assembliesElement, FileInfo testReportFileInfo)
        {
            string fullPath = testReportFileInfo.FullName;

            if (!fullPath.EndsWith(".html"))
            {
                fullPath += ".html";
            }

            var xmlTransform = new System.Xml.Xsl.XslCompiledTransform();

            var currentAssembly = Assembly.GetAssembly(typeof(StarcounterXunitRunner));

            using (var writer = XmlWriter.Create(fullPath, new XmlWriterSettings { Indent = true }))
            using (var xsltStream = currentAssembly.GetManifestResourceStream($"Starcounter.Xunit.Runner.HTML.xslt"))
            using (var xsltReader = XmlReader.Create(xsltStream))
            using (var xmlReader = assembliesElement.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
                Console.WriteLine($"   Test report generated: {fullPath}");
            }
        }
    }
}
