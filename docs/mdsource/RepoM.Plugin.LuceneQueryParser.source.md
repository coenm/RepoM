# Lucene Query Parser

include: _plugin_enable

include: DocsModuleSettingsTests.DocsModuleSettings_LuceneQueryParserPackage#desc.verified.md

To search for repositories, you can use a repom-dialect of Lucene query syntax.

> ## Terms
>
> A query is broken up into terms and operators. There are two types of terms: Single Terms and Phrases.
>
> A Single Term is a single word such as "test" or "hello".
>
> A Phrase is a group of words surrounded by double quotes such as "hello dolly".
>
> Multiple terms can be combined together with Boolean operators to form a more complex query.
>
> ## Fields
>
> Lucene supports fielded data. When performing a search you can either specify a field, or use the default field. The field names and default field is implementation specific.
>
> You can search any field by typing the field name followed by a colon `:` and then the term you are looking for.
>
> As an example, let's assume a Lucene index contains two fields, title and text and text is the default field. If you want to find the document entitled "The Right Way" which contains the text "don't go this way", you can enter:
>
> `title:"The Right Way" AND text:go`
>
> or
>
> `title:"Do it right" AND right`
>
> Since text is the default field, the field indicator is not required.
>
> Note: The field is only valid for the term that it directly precedes, so the query
>
> `title:Do it right`
>
> Will only find "Do" in the title field. It will find "it" and "right" in the default field (in this case the text field).
>
> Note: The analyzer used to create the index will be used on the terms and phrases in the query string. So it is important to choose an analyzer that will not interfere with the terms used in the query string.
>
> ## Boolean Operators
>
> Boolean operators allow terms to be combined through logic operators. Lucene supports `AND`, `OR`, and `NOT` as Boolean operators (Note: Boolean operators must be ALL CAPS).
>
>The `AND` operator is the default conjunction operator. This means that if there is no Boolean operator between two terms, the `AND` operator is used.
>
> ### AND operator
>
> The `AND` operator links two terms and finds repositories when both terms are a match. I.e. `x AND y`. It is also possible to use `&&`.
>
> ### OR operator
>
> The `OR` operator finds repositories if either of the terms matches in a repository.
>
> To search for repositories that match either "x1 x2" or just "x3" use the query:
>
> `"x1 x2" OR x3`. You can also use `||` instead of `OR`.
>
> ### NOT operator
>
> The `NOT` operator excludes respositories that matches the term after NOT. You can also use `!` or `-`. For example, `NOT RepoM` finds all repositories except the ones that matches "RepoM". The same holds for `-RepoM` or `!RepoM`.
>
> ## Grouping
>
> Lucene supports using parentheses to group clauses to form sub queries. This can be very useful if you want to control the boolean logic for a query.
>
> To search for either "jakarta" or "apache" and "website" use the query: `(jakarta OR apache) AND website`.
>
> ## Escaping Special Characters
>
> Lucene supports escaping special characters that are part of the query syntax. The current list special characters are `+ - && || ! ( ) { } [ ] ^ " ~ * ? : \`.
>
>To escape these character use the `\` before the character. For example to search for "(1+1):2" use the query: `\(1\+1\)\:2`
>
