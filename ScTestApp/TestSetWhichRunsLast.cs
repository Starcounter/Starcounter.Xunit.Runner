using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScTestApp
{
    //[TestSet(10)]
    public class TestSetWhichRunsLast
    {
        public static bool TestSetHasBeenRun = false;

        //[Fact]
        public void TestSetWhichRunsLast_TestCase()
        {
            TestSetHasBeenRun = true;
            Assert.True(TestSetWhichRunsFirst.TestSetHasBeenRun);
            Assert.True(TestSetWhichRunsLast.TestSetHasBeenRun);
        }
    }
}
