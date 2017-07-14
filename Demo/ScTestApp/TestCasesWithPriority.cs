using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScTestApp
{
    public class TestCasesWithPriority
    {
        private bool highestHasBeenRun = false;     // -10
        private bool middleHasBeenRun = false;      //  -1
        private bool noPriorityHasBeenRun = false;  //   0
        private bool lowestHasBeenRun = false;      //  10

        //[TestCase(-10)]
        //[Fact]
        public void TestCaseWithHighestPriority()
        {
            highestHasBeenRun = true;
            Assert.True(highestHasBeenRun);
            Assert.True(middleHasBeenRun == false);
            Assert.True(noPriorityHasBeenRun == false);
            Assert.True(lowestHasBeenRun == false);
        }

        //[TestCase(10)]
        //[Fact]
        public void TestCaseWithLowestPriority()
        {
            lowestHasBeenRun = true;
            Assert.True(highestHasBeenRun);
            Assert.True(middleHasBeenRun);
            Assert.True(noPriorityHasBeenRun);
            Assert.True(lowestHasBeenRun);
        }

        //[TestCase(-1)]
        //[Fact]
        public void TestCaseWithMiddlePriority()
        {
            middleHasBeenRun = true;
            Assert.True(highestHasBeenRun);
            Assert.True(middleHasBeenRun);
            Assert.True(noPriorityHasBeenRun == false);
            Assert.True(lowestHasBeenRun == false);
        }

        //[TestCase]
        //[Fact]
        public void TestCaseWithNoPriority()
        {
            noPriorityHasBeenRun = true;
            Assert.True(highestHasBeenRun);
            Assert.True(middleHasBeenRun);
            Assert.True(noPriorityHasBeenRun);
            Assert.True(lowestHasBeenRun == false);
        }
    }
}
