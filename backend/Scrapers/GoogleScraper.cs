using System;
using System.Reflection;
using backend.Models;
using backend.Scrapers.Parsers;
using backend.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace backend.Scrapers
{
    public class GoogleScraper : IScraper
    {
        private readonly IWebDriver _webDriver;
        private readonly string _queryParam;
        private readonly LuceneIndexService _luceneIndexService;
        private readonly int _maxNumOfPapers;
        private const string _url = "https://www.google.com/";

        // HTML Elements
        private const string searchInputXPath = "/html/body/div[1]/div[3]/form/div[1]/div[1]/div[1]/div/div[2]/textarea";
        private const string linkSourceTitleClassName = "yuRUbf";
        private const string sourceClassName = "VuuXrf";

        public GoogleScraper(string queryParam, int maxNumOfPapers, LuceneIndexService luceneIndexService)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless"); // Run Chrome in headless mode
            options.AddArgument("--disable-gpu"); // Disable GPU acceleration (recommended for headless mode
            _webDriver = new ChromeDriver(options);
            _queryParam = queryParam;
            _maxNumOfPapers = maxNumOfPapers;
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
            searchInput.SendKeys(_queryParam);
            searchInput.SendKeys(Keys.Enter);
            Thread.Sleep(500);
        }

        public List<Paper> getMatchedResults()
        {
            List<Paper> papers = new List<Paper>() { };
            List<IWebElement> linkTitleDivs = _webDriver.FindElements(By.ClassName(linkSourceTitleClassName)).ToList();
            List<IWebElement> sourceDivs = _webDriver.FindElements(By.ClassName(sourceClassName)).ToList();
            sourceDivs.RemoveAll(item => item.Text == "");  // remove divs where .Text is an empty string

            for (int i = 0; i < linkTitleDivs.Count(); i++)
            {
                if (papers.Count() == _maxNumOfPapers)
                {
                    break;
                }
                IWebElement linkTitleDivElement = linkTitleDivs[i];
                IWebElement sourceDivElement = sourceDivs[i];
                if (isCorrectDataDiv(linkTitleDivElement))
                {
                    Paper paper = new GoogleParser(_queryParam, linkTitleDivElement, sourceDivElement).Parse();
                    papers.Add(paper);
                }
            }
            return papers;
        }

        public bool isCorrectDataDiv(IWebElement linkTitleDivElement)
        {
            // Checking if result <div> has YearPublished and SampleBody sections
            try
            {
                IWebElement parentDiv = linkTitleDivElement.FindElement(By.XPath(".."));
                IWebElement publishedDateBodyDivs = parentDiv.FindElement(By.XPath("following-sibling::div"));
                IReadOnlyCollection<IWebElement> publishedDateBodySpans = publishedDateBodyDivs.FindElements(By.TagName("span"));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public void SaveResults2Db(List<Paper> papers)
        {
            _luceneIndexService.AddPapers(papers);
        }
    }
}
