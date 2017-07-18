using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Runners;

namespace ScXunitRunner
{
    internal class TestFramework
    {
        // Use an event to know when test execution is done
        internal readonly ManualResetEvent finished = new ManualResetEvent(false);

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

            output += Environment.NewLine + $"Total execution time: {totalExecutionTime}s, Total: {count}, Passed: {passedCount}, Failed: {failedCount}, Skipped: {skippedCount}";
            output += Environment.NewLine + Environment.NewLine;

            return output;
        }

        internal void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            // Do nothing
        }

        internal void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            this.totalExecutionTime = Math.Round(info.ExecutionTime, 3).ToString();

            finished.Set();
        }

        internal void OnTestStarting(TestStartingInfo info)
        {
            // Do nothing
        }

        internal void OnTestFinished(TestFinishedInfo info)
        {
            // Do nothing
        }

        internal void OnTestFailed(TestFailedInfo info)
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

        internal void OnTestPassed(TestPassedInfo info)
        {
            TestCaseResult tcr = new TestCaseResult(
                testCaseName: info.TestDisplayName,
                testState: TestResultState.PASSED,
                executionTime: info.ExecutionTime
                );
            testCaseResults.Add(tcr);
        }

        internal void OnTestSkipped(TestSkippedInfo info)
        {
            TestCaseResult tcr = new TestCaseResult(
                testCaseName: info.TestDisplayName,
                testState: TestResultState.SKIPPED,
                skipReason: info.SkipReason
                );
            testCaseResults.Add(tcr);
        }
    }
}
