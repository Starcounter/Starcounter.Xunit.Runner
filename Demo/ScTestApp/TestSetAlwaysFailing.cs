using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ScTestApp
{
    public class TestSetAlwaysFailing
    {
        [Fact]
        public void TestCase_AlwaysFailing_1()
        {
            Assert.True(false, "This assertion will always fail");
        }

        [Fact]
        public void TestCase_AlwaysFailing_2()
        {
            Assert.True(false, "This assertion will always fail");
        }

        [Fact]
        public void TestCase_AlwaysFailing_3()
        {
            Assert.True(false, "This assertion will always fail");
            Assert.True(false, "This assertion will also always fail");
        }
    }
}
