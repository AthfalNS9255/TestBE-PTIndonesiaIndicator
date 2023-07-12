using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDbScrapper
{
	internal class Program
	{
		static void Main(string[] args)
		{
			new IMDbScrapper().Start().Wait();
			Console.WriteLine("Scraping Data Selesai...");
			Console.ReadLine();
		}
	}
}
