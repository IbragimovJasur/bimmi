using System;
using backend.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;


namespace backend.Services
{
	public class LuceneIndexService
	{
        private const LuceneVersion _version = LuceneVersion.LUCENE_48;
        private const float fiftyPercent = 0.5F;
        private readonly string _indexPath;
        private readonly object lockObject = new object();

        public LuceneIndexService(string indexPath)
        {
            _indexPath = indexPath;
        }

        public void AddPapers(List<Paper> papers)
        {
            lock (lockObject)
            {
                using (FSDirectory directory = FSDirectory.Open(new DirectoryInfo(_indexPath)))
                using (StandardAnalyzer analyzer = new StandardAnalyzer(_version))
                using (IndexWriter writer = new IndexWriter(directory, new IndexWriterConfig(_version, analyzer)))
                {
                    foreach (var paper in papers)
                    {
                        Document document = new Document();
                        document.Add(new TextField("SearchTerm", paper.SearchTerm, Field.Store.YES));
                        document.Add(new TextField("Title", paper.Title, Field.Store.YES));
                        document.Add(new TextField("Link", paper.Link, Field.Store.YES));
                        document.Add(new TextField("Source", paper.Source, Field.Store.YES));
                        document.Add(new TextField("PublishedDate", paper.PublishedDate, Field.Store.YES));
                        document.Add(new TextField("SampleBody", paper.SampleBody, Field.Store.YES));
                        writer.AddDocument(document);
                    }
                    writer.Commit();
                }
            }
        }

        public List<Paper> SearchPapersBySearchTerm(string queryParam)
        {
            List<Paper> papers = new List<Paper>();
            using (FSDirectory directory = FSDirectory.Open(new DirectoryInfo(_indexPath)))
            using (IndexReader reader = DirectoryReader.Open(directory))
            {
                IndexSearcher searcher = new IndexSearcher(reader);
                Analyzer analyzer = new StandardAnalyzer(_version);
                QueryParser parser = new QueryParser(_version, "SearchTerm", analyzer);
                Query query = parser.Parse(queryParam);

                TopDocs topDocs = searcher.Search(query, 8);

                foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                {
                    if (scoreDoc.Score > fiftyPercent)
                    {
                        Document doc = searcher.Doc(scoreDoc.Doc);
                        Paper paper = new Paper(
                            doc.Get("SearchTerm"),
                            doc.Get("Title"),
                            doc.Get("Link"),
                            doc.Get("Source"),
                            doc.Get("PublishedDate"),
                            doc.Get("SampleBody")
                        );
                        papers.Add(paper);
                    }
                }
            }
            return papers;
        }
    }
}
