using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ScTestApp
{
    //[TestSet(saveContext: false, priority: -1)]
    public class TestSetDoNotSaveContext
    {
        private bool test1HasBeenRun = false;
        private bool test2HasBeenRun = false;
        private bool test3HasBeenRun = false;

        [Fact]
        public void TestSetDoNotSaveContext_Test()
        {
            Assert.True(test1HasBeenRun == false);
            test1HasBeenRun = true;
            Assert.True(test1HasBeenRun);
            Assert.True(test2HasBeenRun == false);
            Assert.True(test3HasBeenRun == false);
        }

        [Fact]
        public void TestSetDoNotSaveContext_Test2()
        {
            Assert.True(test2HasBeenRun == false);
            test2HasBeenRun = true;
            Assert.True(test1HasBeenRun == false);
            Assert.True(test2HasBeenRun);
            Assert.True(test3HasBeenRun == false);
        }

        [Fact]
        public void TestSetDoNotSaveContext_Test3()
        {
            Assert.True(test3HasBeenRun == false);
            test3HasBeenRun = true;
            Assert.True(test1HasBeenRun == false);
            Assert.True(test2HasBeenRun == false);
            Assert.True(test3HasBeenRun);
        }
    }
}
