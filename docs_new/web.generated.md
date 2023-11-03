# `web`

Module that provides Web functions (e.g `url_encode`, `json` ...)

This module contains the following methods, variables and/or constants:

- [`web.html_decode`](#web-html-decode)
- [`web.html_encode`](#web-html-encode)
- [`web.html_strip`](#web-html-strip)
- [`web.json`](#web-json)
- [`web.url_decode`](#web-url-decode)
- [`web.url_encode`](#web-url-encode)
- [`web.url_escape`](#web-url-escape)

## html_decode

`web.html_decode(text)`

Decodes a HTML input string (replacing `&amp;` by `&`)

Argument:

- `text`: The input string

### Returns

The input string removed with any HTML entities.

### Example
      
```kalk
    >>> "<p>This is a paragraph</p>" |> html_encode
    # "<p>This is a paragraph</p>" |> html_encode
    out = "&lt;p&gt;This is a paragraph&lt;/p&gt;"
    >>> out |> html_decode
    # out |> html_decode
    out = "<p>This is a paragraph</p>"
    ```

## html_encode

`web.html_encode(text)`

Encodes a HTML input string (replacing `&` by `&amp;`)

Argument:

- `text`: The input string

### Returns

The input string with HTML entities.

### Example
      
#### Usage


```
encoded = web.html_encode("&lt;p&gt;This is a paragraph&lt;/p&gt;");
```

#### Result


```
"&lt;p&gt;This is a paragraph&lt;/p&gt;"
```


## html_strip

`web.html_strip(text)`

Removes any HTML tags from the input string

Argument:

- `text`: The input string

### Returns

The input string removed with any HTML tags

### Example
      
```kalk
    >>> "<p>This is a paragraph</p>" |> html_strip
    # "<p>This is a paragraph</p>" |> html_strip
    out = "This is a paragraph"
    ```

## json

`web.json(value)`

Converts to or from a JSON object depending on the value argument.

Argument:

- `value`: A value argument:
    - If the value is a string, it is expecting this string to be a JSON string and will convert it to the appropriate object.
    - If the value is an array or object, it will convert it to a JSON string representation.

### Returns

A JSON string or an object/array depending on the argument.

### Example
      
```kalk
    >>> json {a: 1, b: 2, c: [4,5], d: "Hello World"}
    # json({a: 1, b: 2, c: [4,5], d: "Hello World"})
    out = "{\"a\": 1, \"b\": 2, \"c\": [4, 5], \"d\": \"Hello World\"}"
    >>> json out
    # json(out)
    out = {a: 1, b: 2, c: [4, 5], d: "Hello World"}
    ```

## url_decode

`web.url_decode(url)`

Converts a URL-encoded string into a decoded string.

Argument:

- `url`: The URL to decode.

### Returns

The decoded URL

### Example
      
#### Usage


```
encoded = web.url_decode("this%3Cis%3Ean%3Aurl+and+another+part");
```

#### Result


```
"this&lt;is&gt;an:url and another part"
```


## url_encode

`web.url_encode(url)`

Converts a specified URL text into a URL-encoded.

 URL encoding converts characters that are not allowed in a URL into character-entity equivalents.
 For example, when the characters < and > are embedded in a block of text to be transmitted in a URL, they are encoded as %3c and %3e.

Argument:

- `url`: The url text to encode as an URL.

### Returns

An encoded URL.

### Example
      
#### Usage


```
encoded = web.url_encode("this&lt;is&gt;an:url and another part");
```

#### Result


```
"this%3Cis%3Ean%3Aurl+and+another+part"
```


## url_escape

`web.url_escape(url)`

Identifies all characters in a string that are not allowed in URLS, and replaces the characters with their escaped variants.

Argument:

- `url`: The input string.

### Returns

The input string url escaped

### Example
      
#### Usage


```
encoded = web.url_escape("&lt;hello&gt; &amp; &lt;scriban&gt;");
```

#### Result


```
"%3Chello%3E%20&amp;%20%3Cscriban%3E"
```

