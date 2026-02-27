# Copilot Instructions for JavaWho-Compiler

## Project Overview

JavaWho-Compiler is a compiler written in **C#** that translates **JavaWho**, a Java-like object-oriented language, into **JavaScript**. The goal is to explore how object-oriented programming languages work through implementation.

**Key language features supported:**
- Class-based inheritance and subtyping
- Objects and methods
- Checking if a variable is initialized before use
- Checking if `void` is used as a value
- Ensuring non-void functions always return
- Non-S-expression-based syntax

## Repository Structure

```
JavaWhoCompiler/       # Main compiler library (C#, .NET 10)
  Tokenizer.cs         # Lexer: converts source text into tokens
  Parser.cs            # Parser: converts tokens into an AST
CompilerTests/         # xUnit test project
  TokenizerTests.cs    # Tests for the tokenizer
  ParserTests.cs       # Tests for the parser
ConsolePlayground/     # Console app for manual testing
  Program.cs
CodeExamples/          # Example JavaWho programs (.txt files)
```

## Build and Test

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Run a specific test project
dotnet test CompilerTests/
```

Tests use the **xUnit** framework with `[Fact]` for single tests and `[Theory]` + `[InlineData(...)]` for parameterized tests.

## Code Conventions

- **Language:** C# with .NET 10
- **Nullable:** disabled (project setting)
- **Implicit usings:** enabled
- Tokens are defined as `sealed record` types implementing `IToken`
- AST nodes are defined as `sealed record` types extending `AST`
- Exceptions are defined as classes inheriting from `Exception` using primary constructors
- Regex patterns for tokenization use `\G` anchor (matches at current position) and word boundary `\b` for keywords

## JavaWho Language Syntax (Quick Reference)

```
type       ::= `Int` | `Boolean` | `Void` | classname
exp        ::= eq_exp
stmt       ::= exp `;` | vardec `;` | var `=` exp `;` | `while` `(` exp `)` stmt
             | `break` `;` | `return` [exp] `;` | `if` `(` exp `)` stmt [`else` stmt]
             | `{` stmt* `}`
methoddef  ::= `method` methodname `(` comma_vardec `)` type `{` stmt* `}`
constructor::= `init` `(` comma_vardec `)` `{` [`super` `(` comma_exp `)` `;`] stmt* `}`
classdef   ::= `class` classname [`extends` classname] `{` (vardec `;`)* constructor methoddef* `}`
program    ::= classdef* stmt+
```

## Compiler Pipeline

The compiler processes JavaWho source code in these stages:
1. **Tokenizer** (`Tokenizer.cs`) — lexes source text into a flat list of `IToken` instances, discarding whitespace
2. **Parser** (`Parser.cs`) — parses the token stream into an AST
3. *(Planned)* **Type Checker** — validates types, initialization, and return paths
4. *(Planned)* **Code Generator** — emits JavaScript output
