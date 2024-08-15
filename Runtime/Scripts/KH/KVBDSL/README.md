# Basic Grammar (Simplified)
```
document
    : entry
    | comment
    ;

comment: '#' [^\n]*

entry: key \s* : \s* value \n
    ;

value
    : 'b' boolvalue
    : 'f' floatvalue
    : 'i' intvalue
    : stringtype
    : arraytype
    : dicttype
    ;

boolvalue
    : 'true'
    | 'false'; # Case insensitive

floatvalue: [+-]?([0-9]+ '.' [0-9]* | '.' [0-9]+)

intvalue: [+-]?[0-9]+

stringtype:
    : s string
    : s? "string"
    : s? """\s*\n multi-line string \s* """
    ;

arraytype: '[' \s*\n (value | comment)* \n\s* ']'

dicttype: '{' \s*\n (entry | comment)* \n\s* '}'
```

# Example File

```
keyInt: i 5
keyStr1: s foo!
keyStr2: "foo!\n"
keyStr3: """
        hello
        multi-line string!
        It prunes starting whitespace.
        """
keyFl: f 5.5
keyBl: b false
```

# Escape Characters

The following escape characters are supported: `\\, \", \b, \f, \n, \r, \t, \v`.

For multi-line strings, `\p` is also supported. It indicates that whitespace to its left should be preserved.

# Multi-Line Strings

The opening `"""` must be on the same line as the key and cannot have any non-whitespace text after it.

Multi-line strings will remove consistent leading whitespace.
For instance:

```
key: """
    foo
    bar
    baz
    """
```
Will end up with the string value "foo\nbar\nbaz".

Leading whitespace can be preserved by removing the whitespace before the closing `"""`, like so:
```
key: """
    foo
    bar
    baz
"""
```

This will end up with the string "    foo\n    bar    \nbaz".

Text can be placed on the same line as the closing `"""`. Any whitespace between the final character and the closing `"""` will be ignored.

The two keys have identical values:
```
key1: """ 
    foo
    bar
    baz """

key2: """
    foo
    bar
    baz
    """
```

In cases where the opening delimiter or closing delimiter are on a separate line from the content, *one* newline in each direction will be consumed. To add newlines before or after your content, add additional spaces.

For instance, if you wanted to represent the string `\nfoo\n`, you could do it like so:
```
key: """

    foo

    """
```

## Leading Whitespace Computation

If a line is all whitespace, it will not be considered when determining the amount of leading whitespace.

The leading whitespace will *either* count tabs or spaces, not both. It is determined by which character it sees first on the first line with non-whitespace content. Escaped whitespace (e.g. `\n` or `\t`) will be treated as non-whitespace characters for the purpose of whitespace computation.

## Terminal Whitespace

All whitespace to the right of the last non-whitespace character on a line will be stripped. To avoid this, add the `\p` character at the end of the whitespace you wish to preserve. The `\p` character will not show up in the output string, but any whitespace to the left of it will be. This would have been a simple backslash, but that would have introduced confusing behavior, as that is used in other languages to signal that line continuation should occur—the opposite effect.

To represent "foo  " using a MLS, you would do the following:
```
key: """
foo  \p
"""
```

Escaped whitespace (e.g. `\n` or `\t`) will be treated as non-whitespace characters for the purpose of terminal whitespace removal.

For instance, in the following:
```
key: """
foo \t
"""
```

The computed string would be "foo \t".

## Line Continuation

Line continuation is not supported. It would make computing leading whitespace non-intuitive.

## Escape Characters

Escaping characters is not required, but permitted. If the `\` character is before a non-escaped character, it will parse as if it was a single `\` character. This is error-prone, especially when copying large texts, but it's preferable to tossing out the whole string in this situation.

# Comment Locations

Comments are any amount of whitespace, followed by '#', followed by anything. Comments terminate with a newline.

## Permitted Locations

Between keys:
```
key1: s foo
# Here is fine.
key2: "bar"
```

Between array entries:
```
key: [
    "foo"
    # Here is also fine.
    i 5
]
```

Between dictionary entries:
```
key: {
    k1: "foo"
    # This is the last fine place.
    k2: i 5
}
```

## Prohibited Locations

After values:
```
key1: s foo # This will not work (it will get eaten up by the string)
```

After opening and closing array and dictionary entries:
```
k1: [ # Not here
    s foo
] # Not here
k2: { # Not here
    k1: foo
} # Not here
```

In MLS openers:
```
mls1: """ # Not here either.
"""
```