# Tags

Tags can be set to repositories and are used for searching, filtering, and ordering purposes.

## Define tags

Tags can be defined in the `TagsV2.yaml` file located in `%APPDATA%\RepoM\` folder.
The yaml consists of two parts. First an optional context section, and second the tags section.

The context is the same context you use defining the action menu's. It allows you to add or define methods and variables. More information over the context can be found [here](Context.md)

The 'tags' section of the yaml is an array of tag specifications. The property `tag` is a string and represents the name of the tag. The optional property `when` is a predicate condition when to apply the tag. The predicate is evaluated using Scriban.

### Example

The following `TagsV2.yaml` file defines three different tags (work, private, github) which are applied to a given repository when the `when` predicate matches the repository.

```yaml
context:
- type: evaluate-script@1
  content: |-
    # at this moment, you must leave this function intact
    # todo, make sure this is not required anymore.
    func translate(input)
      ret input
    end

    func remotes_contain_inner(remotes, url_part)
      urls = remotes | array.map "url"
      filtered = array.filter(urls, do 
        ret string.contains($0, url_part) 
      end)
      ret array.size(filtered) > 0;
    end

    func remotes_contain(url_part)
      ret remotes_contain_inner(repository.remotes, url_part)
    end

    func get_remote_origin()
      remotes = repository.remotes;
      filtered = array.filter(remotes, do 
        remote = $0;
        ret remote.key == "origin"
      end)
      ret array.first(filtered);
    end

    func repository_path_contains(path)
      ret repository.linux_path | string.contains path
    end

    is_work_repository = remotes_contain("My-Work");
    
tags:

# Add tag work when it is a 'work repository'. The 'is_work_repository' variable is defined in the context section.
- tag: work
  when: is_work_repository

# Add tag 'private' when it is not a work repository, and the path contains 'Projects/Private'.
# The custom method 'repository_path_contains' is defined in the context section.
- tag: private
  when: '!is_work_repository && repository_path_contains("Projects/Private")'

# add tag 'github' when the remote contains 'github.com'.
# remote_contains is a method defined in the context section.
- tag: github
  when: 'remotes_contain("github.com")'

```

## Use tags

First, after applying tags, you noticat that they become visible in the UI. Although this is nice, it doesn't do anything for you at the moment.

The power of defining tags is currently the possiblility to search by a tag. For this to work, you need to have selected the 'Lucene query parser'. 

### Search

Search for repositories with a given tag can be done using the the search key `tag` like this:

- `tag:private` will match and show only repostitories containting this tag,
- `!tag:github` will match and show only repositories that do NOT have the `github` tag,
- `tag:work AND tag:github` will show only the repositories that both have the tag `work` as well as `github`.

For now, all regular text is also matched against tags.

- `private` will also match repositories with the tag `private` but will also match a repository called `My private stuff`.

### Filter

TODO
See filtering.

### Ordering

TODO
See Ordering.
