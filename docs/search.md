# Search

After opening RepoM, you can narrow down the repository list in two ways:

- by typing in the search box at the top,
- by enabling one or more quick filters below the search box.

The search box is good for ad-hoc filtering. Quick filters are useful when you want to keep a search around and re-use it with a single click.

## Search box

### Worktree UI preview

Run `dotnet run --project src\RepoM.App -- --worktree-preview` from the repository root
to open the isolated worktree UI prototype. It uses dummy data only: repository discovery,
Git actions, plugins and persisted settings are not started or changed.

The theme selector offers **Dark**, **Light** and **Follow Windows**. Dark is the initial
preview theme. Follow Windows uses the Windows **app** mode and updates when it changes,
without restarting. An unset Windows preference defaults to light; read errors are shown
in the preview and leave its current theme intact. Themes apply to rows, tags, controls
and every context-menu level. An open context menu closes on a theme change so its acrylic
frames are refreshed when reopened. Filtering, favorites, selection and display modes
are preserved. The choice lasts only for this preview session and does not affect the
regular RepoM interface or Windows settings.

The arrow beside a primary repository cycles through three views:

1. Minimal: only the name and favorite star, with branch, status, tags and worktrees hidden.
   Padding, margins and title size stay unchanged so the title does not shift when switching views.
2. Extended: details and tags visible, worktrees hidden.
3. Extended with worktrees: details and child worktrees visible.

**Animations** toggles a short (180 ms) eased height transition when using an item's arrow.
Newly revealed worktrees expand and fade in; text is not scaled. Animations are enabled
by default. Turning them off, filtering or resizing immediately finishes any transition.
Initial loading and scrolling do not animate, and recycled rows retain no animation state.
The toggle is session-only for now; `EnableAnimations` is the preview setting intended
to be connected to persisted configuration when worktree support is integrated.

Worktrees have their own arrow with two views (minimal and extended), starting extended.
Repositories without worktrees also have two views. The arrow's direction indicates the
current view and its tooltip describes the next view. Choices are kept for the preview session.
Selection and keyboard focus use neutral background highlights instead of outlines;
the blue left accent remains reserved for the worktree hierarchy.
Worktrees remain indented and have a connecting accent line in either view. Favorites can be
toggled for the current preview session. Search matches names, branches, paths and tags
(plain text, not the configured query parser). Try `login`: the matching worktree
and its primary repository stay visible, even if the family was collapsed.
Unmatched siblings are hidden. While filtering, matching entries and their parents show
details and view switching is disabled; clearing the search restores each entry's chosen view.

Tags use RepoM's rounded, pink monospace chips, with sample labels on both primary
repositories and worktrees. Clicking a tag filters the preview by that text.

Right-click a row (or use the keyboard context-menu key) to see an acrylic context menu
with editor, terminal, Explorer and Git actions, including a checkout submenu.
All submenu levels use the same background and thin, muted border as the root context menu,
without the theme's outer shadow gutter. This flat popup treatment is enabled only in the preview.
External actions only display a demo notification; they do not start programs or run Git.
The menu's favorite toggle and navigation to the primary repository or another worktree
work within the preview. Navigating clears the search and expands the target family.

### Normal repository search

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
