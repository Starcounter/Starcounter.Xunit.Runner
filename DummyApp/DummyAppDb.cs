using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starcounter;

namespace DummyApp
{
    [Database]
    public class DummyAppDb
    {
        public string Name { get; set; }
        public int Integer { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
