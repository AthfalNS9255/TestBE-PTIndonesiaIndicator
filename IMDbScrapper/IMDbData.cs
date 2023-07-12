using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDbScrapper
{
	class IMDbData
	{
		public int DataKe { get; set; }

		public string Url { get; set; }
		public string Name { get; set; }
		public int Year { get; set; }
		public double Rating { get; set; }
		public string Duration { get; set; }
		public string Image { get; set; }
		public string Directors { get; set; }
		public string Writers { get; set; }
		public string Stars { get; set; }
		public string Plot { get; set; }
		public string Genre { get; set; }

	}
}
