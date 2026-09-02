# Search

After opening RepoM, you can narrow down the repository list in two ways:

- by typing in the search box at the top,
- by enabling one or more quick filters below the search box.

The search box is good for ad-hoc filtering. Quick filters are useful when you want to keep a search around and re-use it with a single click.

## Search box

By default, the search box uses the currently selected query parser.

The default parser is intentionally simple and is mainly suited for free-text matching on repository names and tags. If you need field-based queries such as `tag:work` or `is:favorite`, select a more capable parser such as the Lucene query parser.

The current search text is always combined with the active quick filters. In practice this means that the visible repository list must match both the search box and the enabled quick filters.

## Quick filters

Quick filters are persistent filter chips shown directly below the search box. A quick filter stores a parsed query and lets you enable, disable, invert, rename, and re-order that query without having to type it again.

### What quick filters can do

- Save a search query and re-use it later.
- Turn a repository tag into a reusable filter.
- Quickly switch common filters on and off.
- Invert a filter so matching repositories are excluded instead of included.
- Combine multiple quick filters to build a narrower result set.
- Keep custom labels and tooltips for filters you use often.

### How to create quick filters

There are three ways to get a quick filter:

1. Type a query in the search box and click the pin button.
2. Click a tag on a repository item to create or activate a `tag:<name>` filter.
3. Use the built-in quick filters that are always available.

When you save a search, RepoM parses the text with the currently selected query parser and stores the resulting query. Saving the same query again does not create duplicates; RepoM activates the existing quick filter instead.

### Built-in quick filters

RepoM currently includes two built-in quick filters:

- `Favorites`: matches repositories that are marked as favorite. Internally this is the same as `is:favorite`.
- `Active`: matches repositories with monitoring enabled. Internally this is the same as `is:active`.

Built-in quick filters are always present. They cannot be renamed, deleted, or re-ordered.

### Toggle behavior

Clicking a quick filter cycles through three states:

1. Off: the quick filter is ignored.
2. On: only repositories matching the filter are included.
3. Inverse: repositories matching the filter are excluded.

This makes quick filters useful for both inclusion and exclusion scenarios. For example, a quick filter based on `tag:archived` can be inverted to hide archived repositories.

### Combining behavior

All active quick filters are combined using `AND`.

Examples:

- `Favorites` + `tag:work` shows only favorite work repositories.
- `Active` + `branch:main` shows only monitored repositories on the `main` branch.
- Inverted `tag:archived` + `tag:work` shows work repositories except archived ones.

The search box is also combined with the quick filters using `AND`. So if the search box contains `github` and you enable a quick filter for `tag:work`, RepoM only shows repositories that match both.

### Managing custom quick filters

Custom quick filters support a small context popup that lets you:

- change the label shown on the quick filter chip,
- add or change a tooltip,
- delete the quick filter.

Custom quick filters can also be re-ordered by dragging them. This is useful when you have a small set of frequently used filters and want them in a predictable order.

Custom quick filters are stored in `%APPDATA%\RepoM\quickfilters.json`.

### Practical examples

- Save `tag:work` as a quick filter if you often switch to work repositories.
- Save `is:favorite` if you prefer using a textual query instead of the built-in `Favorites` filter.
- Save `tag:github AND branch:main` when you often work on GitHub repositories on the main branch.
- Click the `private` tag on a repository row to create or activate a quick filter for that tag.

## Query Parser

The query parser is responsible for parsing a query string into a query tree. The leaves are query terms, which can be combined with boolean operators.

The active query parser is used in two places:

- while typing in the search box,
- when saving the current search as a quick filter.

The default query parser takes the query string and creates a single `FreeText` query which is evaluated using the corresponding matcher.

A more sophisticated query parser can create multiple terms combined using operators such as `AND`, `OR`, and `NOT`.

Currently, the following query parsers are available:

- Default (legacy, all text will be matched using FreeTextTerm matcher)
- LuceneQueryParser (provided using `LuceneQueryParser` plugin, which enables a sophisticated query syntax based on Lucene)

### Query matchers

Terms are matched using query matchers. See [Query matchers](querymatchers.md) for the available matchers.

## Terms

There are multiple terms which express intent. It is up to the different handlers how these terms are handled.

### FreeText

Free text without a term. The free text matcher currently matches the free text against the repository name or its tags.

### StartsWithTerm

The start with term has a term and a value.

For example, the term `StartsWithTerm(term: 'branch', value: 'ma')` can be evaluated by a matcher that checks whether the branch starts with `ma`.

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

- Use `Ctrl+F` to focus the search box
- Use `Esc` to clear the box; when already cleared, this key will close RepoM
- Use the pin button to save the current search as a quick filter
- Click a quick filter to cycle through off, on, and inverse
- Click a repository tag to create or activate a tag-based quick filter
