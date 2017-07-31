using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScTestApp
{
    [TestCaseOrderer("ScTestApp.PriorityOrderer", "ScTestApp")]
    public class TestSetWithSavedContextAndPriority : IClassFixture<TestSetWithSavedContextAndPriorityContext>
    {
        private TestSetWithSavedContextAndPriorityContext context;

        public TestSetWithSavedContextAndPriority(TestSetWithSavedContextAndPriorityContext context)
        {
            this.context = context;
        }

        [Fact, TestPriority(-10)]
        public void TestCaseWithHighestPriority()
        {
            context.highestHasBeenRun = true;
            Assert.True(context.highestHasBeenRun);
            Assert.True(context.middleHasBeenRun == false);
            Assert.True(context.noPriorityHasBeenRun == false);
            Assert.True(context.lowestHasBeenRun == false);
        }

        [Fact, TestPriority(10)]
        public void TestCaseWithLowestPriority()
        {
            context.lowestHasBeenRun = true;
            Assert.True(context.highestHasBeenRun);
            Assert.True(context.middleHasBeenRun);
            Assert.True(context.noPriorityHasBeenRun);
            Assert.True(context.lowestHasBeenRun);
        }

        [Fact, TestPriority(-1)]
        public void TestCaseWithMiddlePriority()
        {
            context.middleHasBeenRun = true;
            Assert.True(context.highestHasBeenRun);
            Assert.True(context.middleHasBeenRun);
            Assert.True(context.noPriorityHasBeenRun == false);
            Assert.True(context.lowestHasBeenRun == false);
        }

        [Fact]
        public void TestCaseWithNoPriority()
        {
            context.noPriorityHasBeenRun = true;
            Assert.True(context.highestHasBeenRun);
            Assert.True(context.middleHasBeenRun);
            Assert.True(context.noPriorityHasBeenRun);
            Assert.True(context.lowestHasBeenRun == false);
        }
    }

    public class TestSetWithSavedContextAndPriorityContext
    {
        public bool highestHasBeenRun = false;     // -10
        public bool middleHasBeenRun = false;      //  -1
        public bool noPriorityHasBeenRun = false;  //   0
        public bool lowestHasBeenRun = false;      //  10
    }
}
