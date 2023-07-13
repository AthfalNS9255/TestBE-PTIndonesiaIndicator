using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDbScrapper
{
	internal class Program
	{
		public static bool Cancelled = false;

		static void Main(string[] args)
		{
			Console.CancelKeyPress += Console_CancelKeyPress;
			Console.Title = "IMDbScrapper";

			new IMDbScrapper().Start().Wait();
			Console.WriteLine("Scraping Data Selesai...");
			Console.ReadLine();
		}

		private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true;
			Cancelled = true;
		}
	}
}
