using System;
using System.Linq;
using System.Reflection;
using ScXunitRunner;

namespace ScTestApp
{
    public class Program
    {
        static void Main()
        {
            StarcounterXunitRunner runner = new StarcounterXunitRunner();

            Func<Xunit.Abstractions.ITestCase, bool> testCaseFiler = (testCase) =>
            {
                if (testCase.DisplayName.Contains(nameof(TestSetAlwaysFailing)))
                {
                    return false;
                }
            
                return true;
            };
            runner.TestCaseFiler = testCaseFiler;

            runner.Start();
        }
    }
}