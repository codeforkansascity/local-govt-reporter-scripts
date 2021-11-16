using System.Threading.Tasks;

namespace LocalGovtReporter.Interfaces
{
    interface IScript
    {
        string AgencyName { get; }
        string SiteURL { get; }
        Task<int> RunScriptAsync();
    }
}