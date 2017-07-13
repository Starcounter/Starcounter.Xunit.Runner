using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ClassLibraryXunitTest
{
    public class Class1
    {
        [Fact]
        public void AlwaysTrue_Test()
        {
            Assert.True(true, "Always true");
            Assert.True(true, "Always true");
        }

        [Fact]
        public void AlwaysFalse_Test()
        {
            Assert.True(true, "Always true");
            Assert.True(false, "Always false");
        }
    }
}
