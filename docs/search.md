# Search

After opening RepoM, you can narrow down the repository list in two ways:

- by typing in the search box at the top,
- by enabling one or more quick filters below the search box.

The search box is good for ad-hoc filtering. Quick filters are useful when you want to keep a search around and re-use it with a single click.

## Search box

### Repository interface

The regular RepoM window now uses the card layout from the worktree preview with real
repositories, branches, status and tags. Focusing a control within a repository card
also selects that repository, removing selection from the previous card.
The arrow switches between compact and
extended views. Compact rows show `name   branch` as one continuous text line,
with the branch in the smaller branch font, followed by the favorite and Git indicators:
an amber dot for local changes (hover for staged/unstaged/untracked counts),
an up arrow for commits ahead of upstream and a down arrow for commits behind it.
Both arrows mean the branches have diverged; pushing may require synchronization first.
These counts reflect the last fetched remote state, not a live server check.
Zero counts are hidden, `?` indicates incomplete or unavailable status, and `∅` indicates
no upstream. Stashes alone do not mark a repository dirty. Name and branch are truncated
when space is limited, leaving room for the indicators.
Extended rows show the same Git indicators next to the favorite in the header,
with branch and tags underneath. The old status-text row and monitoring-eye button
are no longer shown. A small triangle in the top-right corner toggles active monitoring
in both compact and extended views. When active, a blue-green two-pixel border
and a matching triangle indicate monitoring. The 14-pixel triangle sits inside the
two-pixel border and follows its inner corner rounding; when inactive, the border is
transparent and the triangle is invisible at rest but remains clickable. Hover
reveals it through a fill-color change. The corner button cannot receive focus and
is skipped during keyboard tab navigation.
Hover and selection retain their normal background colors.
The Active quick filter uses the same blue-green border and top-right triangle
instead of an eye icon. Selected quick filters use a muted green background and a
matching thin border; inverse filters use muted rose tones. Hover slightly lightens each
state. The favorite star stays gold, and the Active filter keeps its blue-green
border and triangle. Both themes use matching palettes.
Repository lists use a narrow rounded scrollbar thumb without arrow buttons; its
muted blue-gray color brightens on hover and adapts to the selected theme.
Hover/menu preparation automatically enables monitoring for repositories without an
explicit choice. Manually disabling monitoring prevents that automatic activation
until it is explicitly enabled again. This choice lasts for the current application session.
Favorites, tag
quick filters, configured context menus and default actions keep their existing behavior.
Display modes last for the current session and survive repository refreshes and filtering.
Worktree discovery and family grouping are not integrated yet; real repositories remain
a flat list until that follow-up.

The existing hamburger menu remains available. Under **Appearance** choose
**Theme > Dark / Light / Follow Windows** or toggle **Animations**. Both settings are
saved in the existing application configuration as `Theme` (`Dark`, `Light`, `System`)
and `EnableAnimations` (boolean). Existing configurations default to Dark with animations
enabled. Follow Windows tracks the Windows app theme live. Disabling animations also
finishes active row transitions and disables the main-window fade-in.

Run `dotnet run --project src\RepoM.App` for the regular application.

#### Context-menu loading and timings

Hover now starts preparing the root menu immediately by default. Set
`MenuPrefetchHoverDelayMilliseconds` to `0` for immediate prefetch, a positive value
for a delay, or a negative value to disable prefetch. Previously `0` disabled it;
use `-1` to preserve that behavior. Existing explicit positive delays remain respected.

`ContextMenuLoadingIndicatorDelayMilliseconds` controls how long the root context menu waits
before showing `Loading...`. It defaults to `200`; set it to `0` to show the indicator
immediately, or a negative value to keep the menu closed until its entries are ready without
showing a loading indicator.

The root becomes available without waiting for deferred submenus. Deferred submenus
load only when opened, using their existing captured Scriban scopes, and show
`Loading...` while pending. Concurrent opens share one task (including failures).
Non-deferred menus retain their original evaluation order during root generation.
Git reads and menu evaluation run off the UI thread.

Search the configured Serilog file for `Context menu`. Information-level entries include:

- `prefetch hit/miss` and `ready`: whether hover work was reused and already complete.
- `Git refresh`: elapsed time spent obtaining a repository snapshot, including RepoM tag
  calculation and any wait for a shared refresh (not just native Git calls).
- `evaluation` and `build`: root-menu evaluation and total build time, with a `MenuBuildId`.
- `deferred submenu`: submenu name, elapsed time and outcome, linked to that build ID.
- `presentation`: the user's wait for data and UI item-population time, separately.

At Debug level, `yielded` records the interval until each root item is produced.
The first interval includes configuration loading and root-context scripts; subsequent
intervals can include skipped actions and whole non-deferred folders. They are not
individual script timings. UI population does not include the subsequent rendered frame.
Root, submenu and presentation times can overlap and should not be summed.
Logs include repository paths and menu labels, but not script bodies or variable values.
With the checked-in logging configuration, files are written under `%APPDATA%\RepoM\Logs`.

Repository refreshes requested by hover/context menus, window activation (`RefreshAllAsync`)
and the periodic monitored refresh share an in-flight read per normalized repository path.
Different repositories can still refresh concurrently. Cancelling one caller only stops its
wait; the read is cancelled when no callers remain. A subsequent request waits for an
abandoned native read to exit before starting another. Discovery scans and filesystem-change
reads remain separate; a new change event must not simply reuse a read begun before that change.
The existing two-second snapshot cache is unchanged, and general refreshes still force a read
unless they join one already running.

Search for `Repository read` for Information-level reader timings. Each invocation has a
`ReadId`, input path, `Stage`, `Attempt`, `ElapsedMs` and `Outcome`:

- `discover`, `open`: locate and open the Git repository.
- `status`: retrieve working-tree/index status, current branch, upstream presence and
  ahead/behind counts through one Git CLI call (`backend=git-cli`), including process
  startup, output reading and parsing.
- `head`: resolve a tag display name for detached HEAD, or read HEAD for a bare repository,
  using LibGit2Sharp. Normal working-tree branches no longer need this separate stage.
- `branches`: enumerate all branch names using LibGit2Sharp (`backend=libgit2sharp`).
- `metadata`: assemble repository data, including stashes and remotes from LibGit2Sharp.
- `repom-tags`: evaluate the configured RepoM tags, including materializing the results.
- `retry-delay`: wait after a locked-file failure.
- `total`: the complete reader invocation, including all attempts and tags.

These read IDs are separate from menu build IDs; correlate them by path and timestamp.
At Debug level, `Repository refresh` logs a refresh ID for starts, joins and cancellations,
and reports snapshot cache hits. A cache hit produces no reader timings. Total and individual
stage times overlap; do not add totals to their constituent stages.

Status uses the installed `git` from PATH with
`status --porcelain=v2 --branch --ahead-behind -z --untracked-files=all --ignored=no --no-renames --ignore-submodules=none`.
Ignored files remain excluded, untracked files remain included, and staged additions,
modifications and deletions retain separate counters. NUL delimiters make filenames with
spaces or newlines safe to parse. `GIT_OPTIONAL_LOCKS=0` prevents status from rewriting the
index just to refresh its stat cache. There is no fallback to LibGit2Sharp status on failure:
missing Git, nonzero exits, timeouts and malformed output are reported as read failures.

Branch headers also handle repositories with no commits and detached HEAD. Missing
ahead/behind information remains `null`, not zero; a configured but missing upstream still
counts as an upstream when Git reports its name. A detached HEAD retains the existing tag
name (when found) or full commit-ID display. Bare repositories have no working-tree status
and retain their existing LibGit2Sharp metadata path.

Cancellation is checked between read stages, during branch/remote enumeration and retry waits.
The CLI status process is terminated on cancellation or after the existing ten-second Git
command timeout. Remaining native LibGit2Sharp calls and the tag factory do not support
interruption mid-call; cancellation is honored after those calls return, and cancelled
results are not published. Compare `status` and `total` timings with earlier logs to assess
the CLI's performance on the same repositories.

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
The preview toggle remains session-only; the regular application's hamburger menu
uses the persisted `EnableAnimations` setting.

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
without the theme's outer shadow gutter. The regular interface uses the same popup treatment.
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
