using backend.Models;
using backend.Scrapers;
using backend.Services;
using backend.Validators;
using Lucene.Net.Index;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.Chrome;

namespace backend.Controllers
{
    [Route("api/v1/search")]
    public class SearchController : Controller
    {
        private const string ResearchGate = "researchgate net";
        private const int ResearchGateNumOfPapers = 2;
        private const string JSTOR = "jstor org";
        private const int JSTORNumOfPapers = 2;
        private const string IEEE_XPLORE = "ieee xplore com";
        private const int IEEE_XPLORENumOfPapers = 1;

        [HttpGet]
        public IActionResult Get([FromQuery(Name = "q")] string queryParam)
        {
            if (QueryParamValidator.isValid(queryParam))
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string luceneIndexPath = Path.Combine(currentDirectory, "LuceneIndex");
                LuceneIndexService luceneIndexService = new LuceneIndexService(luceneIndexPath);
                try
                {
                    List<Paper> papers = luceneIndexService.SearchPapersBySearchTerm(queryParam);
                    if (papers.Count() == 0)
                    {
                        ScrapWithThreads(queryParam, luceneIndexService);
                        papers = luceneIndexService.SearchPapersBySearchTerm(queryParam);
                    }
                    return Ok(papers);
                }
                catch (IndexNotFoundException)
                {
                    ScrapWithThreads(queryParam, luceneIndexService);
                    List<Paper> papers = luceneIndexService.SearchPapersBySearchTerm(queryParam);
                    foreach (Paper paper in papers)
                    {
                        Console.WriteLine("PublishedDate: " + paper.PublishedDate);
                        Console.WriteLine();
                    }
                    return Ok(papers);
                }
            }
            return BadRequest("The term should be at least 5 characters!");
        }

        private void ScrapWithThreads(string queryParam, LuceneIndexService luceneIndexService)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");

            // Scraper classes
            GoogleScholarScraper googleScholarScraper = new GoogleScholarScraper(queryParam, luceneIndexService);
            GoogleScraper researchGateGoogleScraper = new GoogleScraper(
                $"{queryParam} {ResearchGate}",
                ResearchGateNumOfPapers,
                luceneIndexService
            );
            GoogleScraper jstorGoogleScraper = new GoogleScraper(
                $"{queryParam} {JSTOR}",
                JSTORNumOfPapers,
                luceneIndexService
            );
            GoogleScraper ieeeGoogleScraper = new GoogleScraper(
                $"{queryParam} {IEEE_XPLORE}",
                IEEE_XPLORENumOfPapers,
                luceneIndexService
            );

            // Threads
            Thread threadScholar = new Thread(googleScholarScraper.Scrap);
            Thread threadResearchGate = new Thread(researchGateGoogleScraper.Scrap);
            Thread threadJSTOR = new Thread(jstorGoogleScraper.Scrap);
            Thread threadIEEE = new Thread(ieeeGoogleScraper.Scrap);

            threadScholar.Name = "Google Scholar Thread";
            threadResearchGate.Name = "ResearchGate Thread";
            threadJSTOR.Name = "JSTO Thread";
            threadIEEE.Name = "IEEE Thread";

            threadScholar.Start();
            threadResearchGate.Start();
            threadJSTOR.Start();
            threadIEEE.Start();

            threadScholar.Join();
            threadResearchGate.Join();
            threadJSTOR.Join();
            threadIEEE.Join();
        }
    }
}
