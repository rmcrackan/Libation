# Lucene Query Syntax

Libation's search box takes a Lucene query. Simple searches need none of this - type a few words and press enter - but the full syntax is there when you want it.

The examples below use Libation's own fields. Click the [?] button beside the search box for the complete list, along with the synonyms for each: `author` and `authors` both work, as do `length`, `minutes` and `lengthinminutes`.

## Keyword matching

Search the title for the word "hound".

`title:hound`

Search the title for the phrase "sign of the four". Without the quotes these would be four separate words, and a book whose title contains any of them would match.

`title:"sign of the four"`

Search for books Neil Gaiman wrote and also narrated. Without `AND` you would get everything he wrote plus everything anyone narrated of their own.

`author:gaiman AND narrator:gaiman`

Group terms with parentheses to combine `AND` and `OR` in one query.

`(author:doyle AND narrator:fry) OR title:gods`

Search for books by Doyle that Fry did not narrate. `-` and `NOT` mean the same thing.

`author:doyle -narrator:fry`

Exclude a group of alternatives at once.

`-(narrator:fry OR narrator:jacobi)`

## Wildcard matching

Any word in the title starting with "scar", which finds "Scarlet".

`title:scar*`

`*` also works inside a word. This finds "Jacobi".

`narrator:j*bi`

Lucene does not allow `*` or `?` as the first character of a search, so `title:*hound` is an error rather than a search.

## Range searches

A range matches every value between a lower and an upper bound. Square brackets include the bounds and curly braces exclude them, so a book of exactly 290 minutes matches the first of these but not the second.

`length:[290 TO 397]`

`length:{290 TO 397}`

Dates are written as yyyymmdd.

`datepublished:[20200101 TO 20231231]`

Lucene sorts a range lexicographically rather than numerically, which would put 9 after 100. Libation stores its numbers zero-padded so that they sort correctly and pads whatever you type to match, so type the number you mean and ignore this.

---

The structure of the above guide was helpfully provided by https://supermind.org
