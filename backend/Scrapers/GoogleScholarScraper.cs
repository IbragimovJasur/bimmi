using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using backend.Models;
using backend.Scrapers.Parsers;
using backend.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace backend.Scrapers
{
	public class GoogleScholarScraper : IScraper
    {
        private readonly string _queryParam;
        private readonly IWebDriver _webDriver;
        private readonly LuceneIndexService _luceneIndexService;

        public const int maxNumOfPapers = 3;
        private const string _url = "https://scholar.google.com/";

        // HTML Elements
        private const string searchInputXPath = "/html/body/div/div[7]/div[1]/div[2]/form/div/input";
        private const string searchBtnXPath = "/html/body/div/div[7]/div[1]/div[2]/form/button";
        private const string matchingDivsClassName = "gs_ri";

        public GoogleScholarScraper(string queryParam, LuceneIndexService luceneIndexService)
		{
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless"); // Run Chrome in headless mode
            options.AddArgument("--disable-gpu"); // Disable GPU acceleration (recommended for headless mode
            _webDriver = new ChromeDriver(options);
            _queryParam = queryParam;
            _luceneIndexService = luceneIndexService;
		}

        public void Scrap()
        {
            try
            {
                openSearchResultsWebPage();
                List<Paper> papers = getMatchedResults();
                SaveResults2Db(papers);
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                Console.WriteLine($"An exception occurred: {ex.Message}");
            }
            finally
            {
                // Ensure WebDriver is closed even if an exception occurs
                if (_webDriver != null)
                {
                    _webDriver.Quit();
                }
            }
        }

        public void openSearchResultsWebPage()
        {
            _webDriver.Navigate().GoToUrl(_url);
            Thread.Sleep(500);
            var searchInput = _webDriver.FindElement(By.XPath(searchInputXPath));
            var searchBtn = _webDriver.FindElement(By.XPath(searchBtnXPath));
            searchInput.SendKeys(_queryParam);
            searchBtn.Click();
            Thread.Sleep(500);
        }

        public List<Paper> getMatchedResults()
        {
            List<Paper> papers = new List<Paper>() { };

            ICollection<IWebElement> matchingDivs = _webDriver.FindElements(By.ClassName(matchingDivsClassName));
            foreach (IWebElement matchingDiv in matchingDivs)
            {
                if (papers.Count() == maxNumOfPapers)
                {
                    break;
                }
                Paper paper = new GoogleScholarParser(_queryParam, matchingDiv).Parse();
                papers.Add(paper);
            }
            return papers;
        }

        public void SaveResults2Db(List<Paper> papers)
        {
            _luceneIndexService.AddPapers(papers);
        }
    }
}
