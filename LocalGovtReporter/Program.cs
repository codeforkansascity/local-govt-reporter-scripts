using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Scripts.Kansas.City;
using LocalGovtReporter.Scripts.Kansas.County;
using LocalGovtReporter.Scripts.Missouri.City;
using LocalGovtReporter.Scripts.Missouri.County;
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

			foreach (IScript script in scripts)//.Where(s => s.ToString().Contains("Overl")))
			{
				Methods.HelperMethods.MessageBuildingMeetingList(script.AgencyName);
				Console.WriteLine("Class name: " + script.ToString());
				try
				{
					Task<int> result;
					result = script.RunScriptAsync();
					await result;
					Methods.HelperMethods.AddToSummaryMessage(script.AgencyName, result.Result);
				}
				catch (Exception ex)
				{
					Methods.HelperMethods.ErrorOnAgency(script.AgencyName, ex.Message);
				}
			}
			Console.WriteLine(Methods.HelperMethods.SummaryMessage);
		}
	}
}