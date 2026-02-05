using Lucene.Net.Documents;
using System.Collections.Generic;

namespace LibationSearchEngine;

public class SearchResultSet
{
	public string SearchString { get; }
	public IEnumerable<ScoreDocExplicit> Docs { get; }
	public SearchResultSet(string searchString, IEnumerable<ScoreDocExplicit> docs)
	{
		SearchString = searchString;
		Docs = docs;
	}
}
public class ScoreDocExplicit
{
	public Document Doc { get; }
	public string ProductId { get; }
	public float Score { get; }

	public ScoreDocExplicit(Document doc, float score)
	{
		Doc = doc;
		ProductId = doc.GetField(SearchEngine._ID_).StringValue;
		Score = score;
	}
}
