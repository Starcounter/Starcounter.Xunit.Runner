using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Runners;
using Starcounter.Logging;

namespace ScAppXunitRunner
{
    internal class TestFramework
    {
        // Starcounter logging
        internal readonly LogSource log = new LogSource("ScAppXunitRunner");

        // We use consoleLock because messages can arrive in parallel, so we want to make sure we get
        // consistent console output.
        internal object consoleLock = new object();

        // Use an event to know when we're done
        internal readonly ManualResetEvent finished = new ManualResetEvent(false);

        // Start out assuming success; we'll set this to 1 if we get a failed test
        internal int result = 0;

        private readonly List<TestCaseResult> testCaseResults;
        private string totalExecutionTime;

        private int passedCount { get { return testCaseResults.Count(x => x.TestState == TestResultState.PASSED); } }
        private int failedCount { get { return testCaseResults.Count(x => x.TestState == TestResultState.FAILED); } }
        private int skippedCount { get { return testCaseResults.Count(x => x.TestState == TestResultState.SKIPPED); } }
        private int totalCount { get { return testCaseResults.Count(); } }

        internal TestFramework()
        {
            testCaseResults = new List<TestCaseResult>();
        }

        public override string ToString()
        {
            string output = "";
            int count = totalCount;

            for (int i = 0; i < count; i++)
            {
                output += $"[{i + 1}/{count}] " + testCaseResults[i].ToString() + Environment.NewLine;
            }

            output += Environment.NewLine + $"Total execution time: {totalExecutionTime}s, Passed: {passedCount}, Failed: {failedCount}, Skipped: {skippedCount}";

            return output;
        }

        internal void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            lock (consoleLock)
            {
                
            }
        }

        internal void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (consoleLock)
            {
                this.totalExecutionTime = Math.Round(info.ExecutionTime, 3).ToString();
            }

            finished.Set();
        }

        internal void OnTestStarting(TestStartingInfo info)
        {
            lock (consoleLock)
            {
                
            }
        }

        internal void OnTestFinished(TestFinishedInfo info)
        {
            lock (consoleLock)
            {
                
            }
        }

        internal void OnTestFailed(TestFailedInfo info)
        {
            lock (consoleLock)
            {
                TestCaseResult tcr = new TestCaseResult(
                    testCaseName: info.TestDisplayName,
                    testState: TestResultState.FAILED,
                    executionTime: info.ExecutionTime,
                    exceptionMessage: info.ExceptionMessage,
                    exceptionStackTrace: info.ExceptionStackTrace
                    );
                testCaseResults.Add(tcr);
            }

            result = 1;
        }

        internal void OnTestPassed(TestPassedInfo info)
        {
            lock (consoleLock)
            {
                TestCaseResult tcr = new TestCaseResult(
                    testCaseName: info.TestDisplayName, 
                    testState: TestResultState.PASSED, 
                    executionTime: info.ExecutionTime
                    );
                testCaseResults.Add(tcr);
            }
        }

        internal void OnTestSkipped(TestSkippedInfo info)
        {
            lock (consoleLock)
            {
                TestCaseResult tcr = new TestCaseResult(
                    testCaseName: info.TestDisplayName,
                    testState: TestResultState.SKIPPED,
                    skipReason: info.SkipReason
                    );
                testCaseResults.Add(tcr);
            }
        }

        internal void OnTestOutput(TestOutputInfo info)
        {

        }


        internal void OnDiagnosticMessage(DiagnosticMessageInfo info)
        {

        }

        internal void OnErrorMessage(ErrorMessageInfo info)
        {

        }
    }
}
