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
		string UrlTop100 = "search/title/?groups=top_100&sort=user_rating,desc&view=advanced";

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
				await GetMovies();

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

		private async Task GetMovies()
		{
			int Step = 0;
			try
			{
				
				Step = 1; // Set url target
				string UrlTarget = $"{HalamanUtama}/{UrlTop100}";
				
				Console.WriteLine($"Get Movies started : {UrlTarget}");

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

				//await Task.Delay(10000);

				Paging: 
				Step = 3; // Pastikan div listings ada
				var listings = await page.WaitForXPathAsync("//div[@class='lister-list']");
				if (listings == null)
				{
					Console.WriteLine($"GetList step {Step} : Element 'listings' tidak ditemukan.");
					return;
				}

				Step = 4; // Ambil list company
				var list_item = await listings.XPathAsync(".//div[@class='lister-item-content']");
				if (list_item.Length == 0)
				{
					Console.WriteLine($"GetList step {Step} : list item tidak ditemukan.");
					return;
				}

				Step = 5; // Proses data company
				foreach (var item in list_item)
				{
					//await page.WaitForXPathAsync("//div[contains(@data-testid,'listing-grid')]");
					if (DataKe < TargetData)
					{
						var data = new IMDbData();

						Step = 6; //Url Detail Product
						var xUrlDetail = await item.XPathAsync("./h3/a");
						if (xUrlDetail.Length > 0)
						{
							await xUrlDetail[0].EvaluateFunctionAsync("e => e.scrollIntoView()");
							await Task.Delay(100);

							string Pathname = await xUrlDetail[0].EvaluateFunctionAsync<string>("e => e.pathname", xUrlDetail[0]);
							data.Url = $"{HalamanUtama}{Pathname}";
						}

						Step = 7;
						var xName = await item.XPathAsync("./h3/a");
						if (xName.Length > 0)
						{
							data.Name = await xName[0].EvaluateFunctionAsync<string>("e => e.innerText", xName[0]);
						}

						Step = 8;
						var xYear = await item.XPathAsync("./h3/span[contains(@class,'year')]");
						if (xYear.Length > 0)
						{
							string Year = await xYear[0].EvaluateFunctionAsync<string>("e => e.innerText", xYear[0]);

							if (Year.ReplaceByEmpty("(", ")").Contains(" "))
								data.Year = Year.GetAfter(" ").ReplaceByEmpty("(", ")").ToInt32();
							else
								data.Year = Year.ReplaceByEmpty("(", ")").ToInt32();

						}

						Step = 9;
						var xCertificate = await item.XPathAsync("./h3/following-sibling::p[1]/span[@class='certificate']");
						if (xCertificate.Length > 0)
						{
							data.Certificate = await xCertificate[0].EvaluateFunctionAsync<string>("e => e.innerText", xCertificate[0]);
						}

						Step = 10;
						var xDuration = await item.XPathAsync("./h3/following-sibling::p[1]/span[@class='runtime']");
						if (xDuration.Length > 0)
						{
							data.Duration = await xDuration[0].EvaluateFunctionAsync<string>("e => e.innerText", xDuration[0]);
							data.Duration = data.Duration.ReplaceByEmpty("min").Trim();
						}

						Step = 10;
						var xGenre = await item.XPathAsync("./h3/following-sibling::p[1]/span[@class='genre']");
						if (xGenre.Length > 0)
						{
							data.Genre = await xGenre[0].EvaluateFunctionAsync<string>("e => e.innerText", xGenre[0]);
						}

						Step = 11;
						var xRating = await item.XPathAsync("./div/div[contains(@class,'imdb-rating')]");
						if (xRating.Length > 0)
						{
							data.Rating = await xRating[0].EvaluateFunctionAsync<double>("e => e.getAttribute('data-value')", xRating[0]);
						}

						Step = 12;
						if (data.Url.IsNotNullOrEmpty())
							data.Image = $"{data.Url}mediaviewer/";

						Step = 13;
						var xPlot = await item.XPathAsync("./h3/following-sibling::p[2]");
						if (xPlot.Length > 0)
						{
							data.Plot = await xPlot[0].EvaluateFunctionAsync<string>("e => e.innerText", xPlot[0]);
						}

						Step = 14;
						var xDirectors_Stars = await item.XPathAsync("./h3/following-sibling::p[3]");
						if (xDirectors_Stars.Length > 0)
						{
							string Directors_Stars = await xDirectors_Stars[0].EvaluateFunctionAsync<string>("e => e.innerText", xDirectors_Stars[0]);

							data.Directors = Directors_Stars.Split('|').First().GetAfter(":").Trim();
							data.Stars = Directors_Stars.Split('|').Last().GetAfter(":").Trim();
						}

						Step = 15;
						var xVotes = await item.XPathAsync("./h3/following-sibling::p[4]/span[text()='Votes:']/following-sibling::span");
						if (xVotes.Length > 0)
						{
							data.Votes = await xVotes[0].EvaluateFunctionAsync<int>("e => e.getAttribute('data-value')", xVotes[0]);
						}

						Step = 16;
						var xGross = await item.XPathAsync("./h3/following-sibling::p[4]/span[text()='Gross:']/following-sibling::span");
						if (xGross.Length > 0)
						{
							data.Gross = await xGross[0].EvaluateFunctionAsync<string>("e => e.innerText", xGross[0]);
						}

						Step = 17; // hasil
						if (!string.IsNullOrEmpty(data.Name) &&
							Results.Where(x => x.Name == data.Name).Count() == 0)
						{
							DataKe += 1;
							data.DataKe = DataKe;
							Console.OutputEncoding = Encoding.UTF8;
							Console.WriteLine($"List Ke {data.DataKe} = {data.Name}");
							Console.WriteLine("");
							Console.WriteLine($"  - Name        : {data.Name}");
							Console.WriteLine($"  - Year        : {data.Year}");
							Console.WriteLine($"  - Certificate : {data.Certificate}");
							Console.WriteLine($"  - Duration    : {data.Duration} min");
							Console.WriteLine($"  - Genre       : {data.Genre}");
							Console.WriteLine($"  - Rating      : {data.Rating} / 10");
							Console.WriteLine($"  - Image       : {data.Image}");
							Console.WriteLine($"  - Plot        : {data.Plot}");
							Console.WriteLine($"  - Directors   : {data.Directors}");
							Console.WriteLine($"  - Stars       : {data.Stars}");
							Console.WriteLine($"  - Votes       : {data.Votes}");
							Console.WriteLine($"  - Gross       : {data.Gross}");
							Console.WriteLine("");

							SaveToDatabase(data);
							Results.Add(data);

							if (Program.Cancelled)
								break;

						}
					}
				}
				Console.WriteLine($"Result : {Results.Count}/{TargetData} data");

				if (Program.Cancelled || Results.Count == TargetData)
					return;

				Step = 8; // pindah page menggunakan tombol
				var nextPage = await page.XPathAsync("//a[contains(text(), 'Next')]");
				if (nextPage.Length > 0)
				{
					Console.WriteLine("Go to Next Page!");
					Console.WriteLine(new string('-', 50));
					await nextPage[0].ClickAsync();
					goto Paging;
				}
				else
				{
					Console.WriteLine("Data Sudah Habis");
					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"GetList step {Step} : {ex.Message}");
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
					"(Name, Year, Certificate, Duration, Genre, Rating, Image, Plot, Directors, Stars, Votes, Gross) values " +
					"(@Name, @Year, @Certificate, @Duration, @Genre, @Rating, @Image, @Plot, @Directors, @Stars, @Votes, @Gross)", connection);

				Step = 2;
				connection.Open();

				Step = 3;
				insertCommand.Parameters.AddWithValue("@Name", data.Name);
				insertCommand.Parameters.AddWithValue("@Year", data.Year);
				insertCommand.Parameters.AddWithValue("@Certificate", data.Certificate);
				insertCommand.Parameters.AddWithValue("@Duration", data.Duration);
				insertCommand.Parameters.AddWithValue("@Genre", data.Genre);
				insertCommand.Parameters.AddWithValue("@Rating", data.Rating);
				insertCommand.Parameters.AddWithValue("@Image", data.Image);
				insertCommand.Parameters.AddWithValue("@Plot", data.Plot);
				insertCommand.Parameters.AddWithValue("@Directors", data.Directors);
				insertCommand.Parameters.AddWithValue("@Stars", data.Stars);
				insertCommand.Parameters.AddWithValue("@Votes", data.Votes);
				insertCommand.Parameters.AddWithValue("@Gross", data.Gross);

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
