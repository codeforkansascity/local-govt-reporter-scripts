using LocalGovtReporter.Scripts;
using System.Threading.Tasks;

namespace LocalGovtReporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Mission.RunScriptAsync();
        }
    }
}