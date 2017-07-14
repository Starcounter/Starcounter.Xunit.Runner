using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScTestApp
{
    //[TestSet(-10)]
    public class TestSetWhichRunsFirst
    {
        public static bool TestSetHasBeenRun = false;

        //[Fact]
        public void TestSetWhichRunsFirst_TestCase()
        {
            Assert.True(TestSetWhichRunsFirst.TestSetHasBeenRun == false);
            TestSetHasBeenRun = true;
            Assert.True(TestSetWhichRunsFirst.TestSetHasBeenRun);
            Assert.True(TestSetWhichRunsLast.TestSetHasBeenRun == false);
        }
    }
}
