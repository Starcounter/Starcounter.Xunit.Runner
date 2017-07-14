using System;
using System.Linq;
using System.Reflection;
using ScAppXunitRunner;

// nuget: xunit and xunit.runner.console
// copy local -> true (Starcounter, Starcounter.Internal, Starcounter.XSON)
// execute tests as xunit.runner.console WithoutAppDomain (not in separate AppDomain)
// run tests within a SchedulaTast and wait for completion. Scheduling.ScheduleTask(() => { }, waitForCompletion: true);

namespace ScTestApp
{
    public class Program
    {
        static void Main()
        {
            AddStarcounterTestRunner runner = new AddStarcounterTestRunner();
        }
    }
}