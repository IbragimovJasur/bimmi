using System;
using backend.Models;
using OpenQA.Selenium;

namespace backend.Scrapers.Parsers
{
	public class GoogleParser
	{
        public const int numOfSpanElements = 3;
        public const int numOfSpanElement = 0;

        private readonly string _searchTerm;
        private readonly IWebElement _linkTitleDivElement;
        private readonly IWebElement _sourceDivElement;

        public GoogleParser(string SearchTerm, IWebElement linkTitleDivElement, IWebElement sourceDivElement)
		{
            _searchTerm = SearchTerm;
            _linkTitleDivElement = linkTitleDivElement;
            _sourceDivElement = sourceDivElement;
        }

        public Paper Parse()
        {
            string Link = _linkTitleDivElement.FindElement(By.TagName("a")).GetAttribute("href");
            string Title = _linkTitleDivElement.FindElement(By.TagName("h3")).Text;
            string Source = _sourceDivElement.Text;

            IWebElement parentDiv = _linkTitleDivElement.FindElement(By.XPath(".."));
            IWebElement publishedDateBodyDivs = parentDiv.FindElement(By.XPath("following-sibling::div"));
            IReadOnlyCollection<IWebElement> publishedDateBodySpans = publishedDateBodyDivs.FindElements(By.TagName("span"));

            string PublishedDate = getPublishedDateFromSpans(publishedDateBodySpans);
            string SampleBody = getSampleBodyFromSpans(publishedDateBodySpans);

            return new Paper(
                SearchTerm: _searchTerm,
                Title: Title,
                Link: Link,
                Source: Source,
                PublishedDate: PublishedDate,
                SampleBody: SampleBody
            );
        }

        private string getPublishedDateFromSpans(IReadOnlyCollection<IWebElement> publishedDateBodySpans)
        {
            // Spans elements are in the following form:
            // <span>
            //    <span>9-may, 2023<span/>
            //    --
            // <span/>
            // <span>{SampleBody}<span/>

            if (publishedDateBodySpans.Count() == numOfSpanElements)
            {
                // skips 1st, take only 2nd
                var dateBodySpans = publishedDateBodySpans.Skip(1).Take(1).ToList();
                return dateBodySpans[0].Text;
            }
            return "";
        }

        private string getSampleBodyFromSpans(IReadOnlyCollection<IWebElement> publishedDateBodySpans)
        {
            // Spans elements are in the following form:
            // <span>
            // 
            //    <span>9-may, 2023<span/>
            //    --
            // <span/>
            // <span>{SampleBody}<span/>

            if (publishedDateBodySpans.Count() == numOfSpanElements)
            {
                // skips 1st & 2nd, take 3rd
                var dateBodySpans = publishedDateBodySpans.Skip(2).Take(1).ToList();
                return dateBodySpans[0].Text;
            }
            if (publishedDateBodySpans.Count() > numOfSpanElement)
            {
                return publishedDateBodySpans.LastOrDefault().Text;
            }
            return "";
        }
    }
}
