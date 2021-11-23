global using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LocalGovtReporter.Interfaces;

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
			try
			{
				await Methods.HelperMethods.MessageSendSummaryMessage(Methods.HelperMethods.SummaryMessage);
			}
			catch (Exception ex)
            {
				Methods.HelperMethods.ErrorOnAgency("Error sending SQS", ex.Message);

			}
		}
	}
}