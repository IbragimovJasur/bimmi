using System;
using backend.Models;
using OpenQA.Selenium;

namespace backend.Scrapers
{
	public interface IScraper
	{
		public void Scrap();
		public void openSearchResultsWebPage();
		public List<Paper> getMatchedResults();
		public void SaveResults2Db(List<Paper> papers);
	}
}
