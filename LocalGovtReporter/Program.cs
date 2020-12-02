using LocalGovtReporter.Scripts;
using LocalGovtReporter.Scripts.Kansas.City;
using LocalGovtReporter.Scripts.Missouri.City;
using LocalGovtReporter.Scripts.Missouri.County;
using System.Threading.Tasks;

namespace LocalGovtReporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Jackson.RunScriptAsync();
        }
    }
}