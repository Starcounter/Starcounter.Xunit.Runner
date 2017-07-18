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
            StarcounterXunitRunner runner = new StarcounterXunitRunner(triggerOnInstanceCreation: true, createUrlHandler: true);
        }
    }
}