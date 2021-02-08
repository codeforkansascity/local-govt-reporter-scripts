using LocalGovtReporter.Scripts.Kansas.City;
using LocalGovtReporter.Scripts.Kansas.County;
using LocalGovtReporter.Scripts.Missouri.County;
using System.Threading.Tasks;
using KansasCity = LocalGovtReporter.Scripts.Missouri.City.KansasCity;

namespace LocalGovtReporter
{
    class Program
    {
        static async Task Main()
        {
            await OverlandPark.RunScriptAsync();
            await KansasCity.RunScriptAsync();
            await Mission.RunScriptAsync();
            await Jackson.RunScriptAsync();
            await Johnson.RunScriptAsync();
            await Wyandotte.RunScriptAsync();
        }
    }
}