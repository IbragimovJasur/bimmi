using System;
using backend.Models;
using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace backend.Scrapers.Parsers
{
	public class GoogleScholarParser
	{
        private const string Source = "GoogleScholar";
        private const string sourceYearDivClassName = "gs_a";
        private const string sampleBodyDivClassName = "gs_rs";

        private readonly string SearchTerm;
        private readonly IWebElement DivElement;

        public GoogleScholarParser(string SearchTerm, IWebElement DivElement)
		{
            this.SearchTerm = SearchTerm;
            this.DivElement = DivElement;
		}

        public Paper Parse()
        {
            var linkTitleATag = DivElement.FindElement(By.TagName("h3")).FindElement(By.TagName("a"));
            var sourceYearDiv = DivElement.FindElement(By.ClassName(sourceYearDivClassName));
            var sampleBodyDiv = DivElement.FindElement(By.ClassName(sampleBodyDivClassName));
            return new Paper(
                SearchTerm: SearchTerm,
                Title: linkTitleATag.Text,
                Link: linkTitleATag.GetAttribute("href"),
                Source: extractSourceFromString(sourceYearDiv.Text),
                PublishedDate: extractYearFromString(sourceYearDiv.Text),
                SampleBody: sampleBodyDiv.Text
            );
        }

        private string extractSourceFromString(string sourceYearDivText)
        {
            // Split the string into words
            string[] words = sourceYearDivText.Split(" ");

            // Check if there are any words in the string
            if (words.Length > 0)
            {
                // Get the last word
                return words[words.Length - 1];
            }

            // Default value if no words are found
            return "";
        }

        private string extractYearFromString(string sourceYearDivText)
        {
            // Use regular expression to find the year
            Regex regex = new Regex(@"(\b\d{4}\b)");
            Match match = regex.Match(sourceYearDivText);

            if (match.Success)
            {
                return match.Value;
            }
            return ""; // Default value if year is not found
        }
    }
}
