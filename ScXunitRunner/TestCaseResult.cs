using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScXunitRunner
{
    internal class TestCaseResult
    {
        internal string TestCaseName { get; private set; }
        internal TestResultState TestState { get; private set; }
        internal string ExecutionTime { get; private set; }
        internal string ExceptionMessage { get; private set; }
        internal string ExceptionStackTrace { get; private set; }
        internal string SkipReason { get; private set; }

        internal TestCaseResult(
            string testCaseName, 
            TestResultState testState, 
            decimal executionTime = 0, 
            string exceptionMessage = null, 
            string exceptionStackTrace = null,  
            string skipReason = null)
        {
            TestCaseName = testCaseName;
            TestState = testState;
            ExecutionTime = Math.Round(executionTime, 3).ToString();
            ExceptionMessage = exceptionMessage ?? string.Empty;
            ExceptionStackTrace = exceptionStackTrace ?? string.Empty;
            SkipReason = skipReason ?? string.Empty;
        }

        public override string ToString()
        {
            string output = TestCaseName.PadRight(96, '.') + " " + TestState.ToString() + $". Execution time: {ExecutionTime}s";

            if (TestState == TestResultState.PASSED)
            {
                // Do nothing
            }
            else if (TestState == TestResultState.FAILED)
            {
                output += Environment.NewLine + ExceptionMessage + Environment.NewLine + "Stack Trace: " + Environment.NewLine + ExceptionStackTrace + Environment.NewLine;
            }
            else if (TestState == TestResultState.SKIPPED)
            {
                output += Environment.NewLine + SkipReason + Environment.NewLine;
            }
            else
            {
                // Should never be entered
            }

            return output;
        }
    }
}
