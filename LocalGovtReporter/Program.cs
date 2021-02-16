using LocalGovtReporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LocalGovtReporter
{
    class Program
    {
        public static async Task Main()
        {
            IEnumerable<IScript> scripts =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => typeof(IScript).IsAssignableFrom(type))
                    .Where(type =>
                        !type.IsAbstract &&
                        !type.IsGenericType &&
                        type.GetConstructor(new Type[0]) != null)
                    .Select(type => (IScript)Activator.CreateInstance(type))
                    .ToList();

            foreach (IScript script in scripts)
                await script.RunScriptAsync();
        }
    }
}