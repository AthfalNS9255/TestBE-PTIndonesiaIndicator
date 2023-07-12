using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Security.Cryptography.X509Certificates;

namespace IMDbScrapper
{
	internal class IMDbScrapper
	{
		IPage page;
		IBrowserFetcher browserFetcher;
		IBrowser browser;

		int TargetData = 100;
		int DataKe = 0;
		string HalamanUtama = "https://www.imdb.com";
		string UrlTop100 = "chart/top/";

		List<IMDbData> Results;

		public async Task Start()
		{
			int Step = 0;
			try
			{
				Step = 1; // init data
				Results = new List<IMDbData>();

				Step = 2; // start browser
				await StartBrowser();

				Step = 3; // get list top 100 movies
				await GetList();

				Step = 4; // ambil detail
				foreach (var data in Results)
				{
					var tunggu = new RandomGenerator().RandomNumber(2, 5);
					await Task.Delay(tunggu * 1000);
					await GetDetail(data);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Start step {Step} : {ex.Message}");
			}
			finally
			{
				StopBrowser();
			}
		}

		private async Task GetList()
		{
			int Step = 0;
			try
			{
				Step = 1; // Set url target
				string UrlTarget = $"{HalamanUtama}/{UrlTop100}";
				Console.WriteLine($"Get list {UrlTarget}");

				var navigation = new NavigationOptions
				{
					Timeout = 0,
					WaitUntil = new[]
					{
						WaitUntilNavigation.Networkidle2
					}
				};

				Step = 2; // Load web
				await page.GoToAsync(UrlTarget, navigation);

				Step = 3; // Pastikan div listings ada
				var listings = await page.WaitForXPathAsync("//tbody[@class='lister-list']");
				if (listings == null)
				{
					Console.WriteLine($"GetList step {Step} : Element 'listings' tidak ditemukan.");
					return;
				}

				Step = 4; // Ambil list company
				var listings_company = await listings.XPathAsync("./tr");
				if (listings_company.Length == 0)
				{
					Console.WriteLine($"GetList step {Step} : listings_company tidak ditemukan.");
					return;
				}

				Step = 5; // Proses data company
				foreach (var item in listings_company)
				{
					//await page.WaitForXPathAsync("//div[contains(@data-testid,'listing-grid')]");
					if (DataKe < TargetData)
					{
						var data = new IMDbData();

						Step = 6; //Url Detail Product
						var xUrlDetail = await item.XPathAsync("./td[contains(@class, 'title')]/a");
						if (xUrlDetail.Length > 0)
						{
							string Pathname = await xUrlDetail[0].EvaluateFunctionAsync<string>("e => e.pathname", xUrlDetail[0]);
							data.Url = $"{HalamanUtama}{Pathname}";
						}

						await xUrlDetail[0].EvaluateFunctionAsync("e => e.scrollIntoView()");
						var tunggu = new RandomGenerator().RandomNumber(2, 5);
						await Task.Delay(tunggu * 100);

						Step = 7; // hasil
						if (!string.IsNullOrEmpty(data.Url) && 
							Results.Where(x => x.Url == data.Url).Count() == 0)
						{
							DataKe += 1;
							data.DataKe = DataKe;
							Console.WriteLine($"List Ke {data.DataKe} = {data.Url}\n");
							Results.Add(data);

						}
					}
				}
				Console.WriteLine($"Result : {Results.Count} data");
				Console.WriteLine(new string('-', 100));

			}
			catch (Exception ex)
			{
				Console.WriteLine($"GetList step {Step} : {ex.Message}");
			}
		}

		private async Task GetDetail(IMDbData data)
		{
			int Step = 0;
			try
			{
				Console.WriteLine($"Get detail : {data.DataKe}");

				var navigation = new NavigationOptions
				{
					Timeout = 0,
					WaitUntil = new[]
					{
						WaitUntilNavigation.Networkidle2
					}
				};

				//var ClearCache = await page.Target.CreateCDPSessionAsync();
				//await ClearCache.SendAsync("Network.clearBrowserCookies");
				//await ClearCache.SendAsync("Network.clearBrowserCache");

				Step = 1; // Load web
				await page.GoToAsync(data.Url, navigation);

				Step = 2; // Pastikan element main ada
				var xCard = await page.WaitForXPathAsync("//main");
				if (xCard == null)
				{
					Console.WriteLine($"- GetDetail step {Step} : Element tidak ditemukan.");
					return;
				}

				Step = 3; // untuk ambil Nama film
				var xName = await xCard.XPathAsync(".//h1");
				if (xName.Length > 0)
				{
					data.Name = await xName[0].EvaluateFunctionAsync<string>("e => e.innerText", xName[0]);
				}

				Step = 4; // untuk ambil Nama Pemasang
				var xYear = await xCard.XPathAsync(".//a[contains(@href, 'release')]");
				if (xYear.Length > 0)
				{
					data.Year = await xYear[0].EvaluateFunctionAsync<int>("e => e.innerText", xYear[0]);
				}

				Step = 5; // ambil rating film
				var xRating = await xCard.XPathAsync(".//div[contains(@data-testid, 'rating')]/span");
				if (xRating.Length > 0)
				{
					data.Rating = await xRating[0].EvaluateFunctionAsync<double>("e => e.innerText", xRating[0]);
				}

				Step = 6; // ambil duration film
				var xDuration = await xCard.XPathAsync(".//h1[contains(@data-testid, 'pageTitle')]/following-sibling::ul/li[3]");
				if (xDuration.Length > 0)
				{
					data.Duration = await xDuration[0].EvaluateFunctionAsync<string>("e => e.innerText", xDuration[0]);
				}

				Step = 7; // Images film
				data.Image = $"{data.Url}mediaviewer/";

				Step = 8; // ambil director film
				var xLDirectors = await xCard.XPathAsync("//*[contains(text(), 'Director')]/following-sibling::div");
				if (xLDirectors.Length > 0)
				{
					List<string> Directors = new List<string>();
					var xDirectors = await xLDirectors[0].XPathAsync(".//a");

					foreach (var item in xDirectors)
					{
						string Director = await item.EvaluateFunctionAsync<string>("e => e.innerText", item);
						Directors.Add(Director);
					}

					data.Directors = string.Join(", ", Directors);
				}

				Step = 9; // ambil writers film
				var xLWriters = await xCard.XPathAsync(".//*[contains(text(), 'Writer')]/following-sibling::div");
				if (xLWriters.Length > 0)
				{
					List<string> Writers = new List<string>();
					var xWriters = await xLWriters[0].XPathAsync(".//a");
					
					foreach (var item in xWriters)
					{
						string Writer = await item.EvaluateFunctionAsync<string>("e => e.innerText", item);
						Writers.Add(Writer);
					}

					data.Writers = string.Join(", ", Writers);
				}

				Step = 10; // ambil stars film
				var xLStars = await xCard.XPathAsync(".//*[contains(text(), 'Star')]/following-sibling::div");
				if (xLStars.Length > 0)
				{
					List<string> Stars = new List<string>();
					var xStars = await xLStars[0].XPathAsync(".//a");

					foreach (var item in xStars)
					{
						string Star = await item.EvaluateFunctionAsync<string>("e => e.innerText", item);
						Stars.Add(Star);
					}

					data.Stars = string.Join(", ", Stars);
				}

				Step = 11; // ambil plot film
				var xPlot = await xCard.XPathAsync(".//p[@data-testid='plot']");
				if (xPlot.Length > 0)
				{
					data.Plot = await xPlot[0].EvaluateFunctionAsync<string>("e => e.innerText", xPlot[0]);
				}

				Step = 12; // ambil Genre film
				var xGenres = await xCard.XPathAsync(".//div[@data-testid='genres']//a");
				if (xGenres.Length > 0)
				{
					List<string> Genres = new List<string>();
					
					foreach (var item in xGenres)
					{
						string Genre = await item.EvaluateFunctionAsync<string>("e => e.innerText", item);
						Genres.Add(Genre);
					}

					data.Genre = string.Join(", ", Genres);
				}

				Step = 16;
				if (!string.IsNullOrEmpty(data.Name))
				{
					Console.OutputEncoding = Encoding.UTF8;
					Console.WriteLine($"  - URL        : {data.Url}");
					Console.WriteLine($"  - Name       : {data.Name}");
					Console.WriteLine($"  - Year       : {data.Year}");
					Console.WriteLine($"  - Genre      : {data.Genre}");
					Console.WriteLine($"  - Plot       : {data.Plot}");
					Console.WriteLine($"  - Rating     : {data.Rating} / 10");
					Console.WriteLine($"  - Duration   : {data.Duration}");
					Console.WriteLine($"  - Image      : {data.Image}");
					Console.WriteLine($"  - Directors  : {data.Directors}");
					Console.WriteLine($"  - Writers    : {data.Writers}");
					Console.WriteLine($"  - Stars      : {data.Stars}");

					SaveToDatabase(data);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error GetDetailPemasang Step {Step} : {ex.Message}");
			}
		}

		private void SaveToDatabase(IMDbData data)
		{
			int Step = 0;
			try
			{
				Step = 1;
				MySqlConnection connection = new MySqlConnection("Server=localhost; Database=imdbscrapper; Uid=root; Pwd=root;");
				MySqlCommand insertCommand = new MySqlCommand("Insert into movie " +
					"(Url, Name, Year, Rating, Duration, Image, Directors, Writers, Stars, Plot, Genre) values " +
					"(@Url, @Name, @Year, @Rating, @Duration, @Image, @Directors, @Writers, @Stars, @Plot, @Genre)", connection);

				Step = 2;
				connection.Open();

				Step = 3;
				insertCommand.Parameters.AddWithValue("@Url", data.Url);
				insertCommand.Parameters.AddWithValue("@Name", data.Name);
				insertCommand.Parameters.AddWithValue("@Year", data.Year);
				insertCommand.Parameters.AddWithValue("@Rating", data.Rating);
				insertCommand.Parameters.AddWithValue("@Duration", data.Duration);
				insertCommand.Parameters.AddWithValue("@Image", data.Image);
				insertCommand.Parameters.AddWithValue("@Directors", data.Directors);
				insertCommand.Parameters.AddWithValue("@Writers", data.Writers);
				insertCommand.Parameters.AddWithValue("@Stars", data.Stars);
				insertCommand.Parameters.AddWithValue("@Plot", data.Plot);
				insertCommand.Parameters.AddWithValue("@Genre", data.Genre);

				Step = 3;
				if (insertCommand.ExecuteNonQuery() > 0)
				{
					Console.WriteLine("Row Added");
					Console.WriteLine(new string('-', 50));
				}

				Step = 4;
				connection.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error SaveToDatabase Step {Step} : {ex.Message}");
			}
		}

		private async Task StartBrowser()
		{
			int Step = 0;
			try
			{
				Step = 1; // Browser fetch
				browserFetcher = new BrowserFetcher();
				await browserFetcher.DownloadAsync();

				Step = 2; // Init browser
				var browser = await Puppeteer.LaunchAsync(new LaunchOptions
				{
					Headless = true,
					DefaultViewport = null,
					//Args = new[] { $"--proxy-server=5.35.93.34:8091" }
					//Devtools = true,
					//ExecutablePath = ""
				});

				Step = 3; // Init page
				var pages = await browser.PagesAsync();
				page = pages[0];

				await page.SetRequestInterceptionAsync(true);

				// disable images to download
				page.Request += (sender, e) =>
				{
					if (e.Request.ResourceType == ResourceType.Image)
						e.Request.AbortAsync();
					else
						e.Request.ContinueAsync();
				};

				Step = 5; // Set user agent
				await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

			}
			catch (Exception ex)
			{
				Console.WriteLine($"StartBrowser step {Step} : {ex.Message}");
			}
		}

		private void StopBrowser()
		{
			int Step = 0;
			try
			{
				Step = 1; // Tutup browser
				page?.Dispose();
				browser?.Dispose();
				browserFetcher?.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"StopBrowser step {Step} : {ex.Message}");
			}
		}
	}
}
