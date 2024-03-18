using System;
namespace backend.Models
{
	public class Paper
	{
        public string SearchTerm { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Source { get; set; }
        public string PublishedDate { get; set; }  // either: month+year or only year
        public string SampleBody { get; set; }

        public Paper(string SearchTerm, string Title, string Link, string Source, string PublishedDate, string SampleBody)
        {
            this.SearchTerm = SearchTerm;
            this.Title = Title;
            this.Link = Link;
            this.Source = Source;
            this.PublishedDate = PublishedDate;
            this.SampleBody = SampleBody;
        }
    }
}
