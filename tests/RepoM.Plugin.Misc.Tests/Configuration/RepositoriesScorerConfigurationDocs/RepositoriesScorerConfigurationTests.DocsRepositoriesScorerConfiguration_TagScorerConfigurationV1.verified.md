Repository scorer based on the tags of a repository.

Properties:

- `weight`: The weight of this scorer. The higher the weight, the higher the impact.
- `tag`: The tag to match on. If the repository has this tag, the score will return the weight, otherwise, `0`. (optional)
