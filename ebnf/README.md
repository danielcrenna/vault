ebnf
====
This is an EBNF parser in C#.

- EBNF is a cornerstone of language-oriented development, as you can describe any deterministic programming language as a subset of EBNF.
- The unit tests include usage as well as the beginnings of a T4 template to generate an Earley parser for a valid EBNF grammar.
- An Earley parser, while bound to the size of the AST, can parse any grammar, where ambiguation kills traditional recursive descent parsers
- The combination of an EBNF syntax tree and a T4 template to generate and compile any language grammar into a purpose-built parser, seems rad.