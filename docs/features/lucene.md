# Lucene Query Syntax

Lucene has a custom query syntax for querying its indexes. Here are some query examples demonstrating the query syntax.

## Keyword matching

Search for word "foo" in the title field.

`title:foo`

Search for phrase "foo bar" in the title field.

`title:"foo bar"`

Search for phrase "foo bar" in the title field AND the phrase "quick fox" in the body field.

`title:"foo bar" AND body:"quick fox"`

Search for either the phrase "foo bar" in the title field AND the phrase "quick fox" in the body field, or the word "fox" in the title field.

`(title:"foo bar" AND body:"quick fox") OR title:fox`

Search for word "foo" and not "bar" in the title field.

`title:foo -title:bar`

## Wildcard matching

Search for any word that starts with "foo" in the title field.

`title:foo*`

Search for any word that starts with "foo" and ends with bar in the title field.

`title:foo*bar`

Note that Lucene doesn't support using a * symbol as the first character of a search.

## Range searches

Range Queries allow one to match documents whose field(s) values are between the lower and upper bound specified by the Range Query. Range Queries can be inclusive or exclusive of the upper and lower bounds. Sorting is done lexicographically.


`mod_date:[20020101 TO 20030101]`

---

Copied from the now inactive [LuceneTutorial.com](http://www.lucenetutorial.com/lucene-query-syntax.html)
