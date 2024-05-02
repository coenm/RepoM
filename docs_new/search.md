# Search

After opening RepoM, you can search for respositories using the searchbox at the top.

By default, this box enabled you to search for repositories by simply matching repository names (i.e., the default query parser is very limited).

It is possible to use a different query parser enabling the use of complex queries.

## Query Parser

The query parser is responsible for parsing a query string into a tree representation of this query. The leaves are so called query terms, and which can be combined with boolean operators.

The default query parser takes the query string and creates a single 'FreeText' which will be evaluted using the corresponding matcher.

A suffisticated query parser can create for instance multiple terms combined using an 'OR' operator, so only one term has to match.

Currently, the following query parsers are available:

- Default (legacy, all text will be matched using FreeTextTerm matcher)
- LuceneQueryParser (provided using `LuceneQueryParser` plugin, which enables a suffisticated query syntax based on Lucene)

### Terms and Operators

sdf

### Query matchers

`IQueryMatcher`

sfd

## Terms

There are multiple terms which express intent. It is up to the different handlers how these terms are handled.

### FreeText

Free text without a term. The free text matcher currently matches the free text against the repository name or it's tags.

### SimpleTerm

sdf

### StartsWithTerm

The start with term has a term and a value.

For example, the term `StartsWithTerm(term: 'branch', value: 'ma')` will be evaluated by a specific IQueryMatcher which checks if the

<!-- ### RangeTerm (Rename TermRange)

Not used.

### WildCardTerm

Not used. -->

## Operators

Operators are also terms.

### And

The `And` operator combines one or more terms. The 'and query matcher' takes into account that all terms should be true for the evaluated repository to be true.

### Or

The `Or` operator combines one or more terms. The 'or query matcher' makes sure that only one term should be true for the evaluated repository to be true.

### Not

The `Not` operator accepts one term. The 'not query matcher' will inverse the matched result of the term for the evaluated repository.

## Cheatsheet

- Use <kbd>Ctrl</kbd>+<kbd>F</kbd> to focus the search box
- Use <kbd>Esc</kbd> to clear the box, when already cleared, this key will close RepoM
