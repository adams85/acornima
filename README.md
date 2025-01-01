[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/adams85/acornima/build.yml)](https://github.com/adams85/acornima/actions/workflows/build.yml)
[![NuGet Release](https://img.shields.io/nuget/v/Acornima)](https://www.nuget.org/packages/Acornima)
[![Feedz Version](https://img.shields.io/feedz/v/acornima/acornima/Acornima)](https://feedz.io/org/acornima/repository/acornima/packages/Acornima)
[![Donate](https://img.shields.io/badge/-buy_me_a%C2%A0coffee-gray?logo=buy-me-a-coffee)](https://www.buymeacoffee.com/adams85)

# Acorn + Esprima = Acornima

This project is an interbreeding of the [acornjs](https://github.com/acornjs/) and the [Esprima.NET](https://github.com/sebastienros/esprima-dotnet) parsers, with the intention of creating an even more complete and performant ECMAScript (a.k.a JavaScript) parser library for .NET by combining the best bits of those.

It should also be mentioned that there is an earlier .NET port of acornjs, [AcornSharp](https://github.com/MatthewSmit/AcornSharp), which though is unmaintained for a long time, served as a good starting point. Had it not been for AcornSharp, this project would probably have never started.

### Here is how this Frankenstein's monster looks like:

* The tokenizer is mostly a direct translation of the acornjs tokenizer to C# (with many bigger and smaller performance improvements, partly inspired by Esprima.NET) - apart from the regex validation/conversion logic, which has been borrowed from Esprima.NET.
* The parser is ~99% acornjs (also with a bunch of minor improvements) and ~1% Esprima.NET (strict mode detection, public API). It is also worth mentioning that the error reporting has been changed to use the error messages of V8.
* It includes protection against the non-catchable `StackOverflowException` using [the same approach](https://github.com/adams85/acornima/blob/v1.0.0/src/Acornima/Helpers/StackGuard.cs) as Roslyn.
* Both parent projects follow the ESTree specification, so does Acornima. The actual AST implementation is based on that of Esprima.NET, with further minor improvements to the class hierarchy that bring it even closer to the spec and allow encoding a bit more information.
* The built-in AST visitors and additional utility features stems from Esprima.NET as well.

### And what good comes out of this mix?

* A parser which already matches the performance of Esprima.NET, while doing more: it also passes the **complete** [Test262 test suite](https://github.com/tc39/test262) for ECMAScript 2023.
* It is also more economic with regard to stack usage, so it can parse ~2x deeper structures.
* More options for fine-tuning parsing.
* A standalone tokenizer which can deal with most of the ambiguities of the JavaScript grammar (thanks to the clever context tracking solution implemented by acornjs).
* The parser tracks variable scopes to detect variable redeclarations. As of v1.1.0, it's able to expose the collected scope information to the consumer (see also [this PR](https://github.com/adams85/acornima/pull/13) or this [other example of usage](https://github.com/adams85/bundling/blob/3.9.0/source/Bundling.EcmaScript/Internal/Helpers/VariableScopeBuilder.cs)).

### Getting started

#### 1. Install the [package](https://www.nuget.org/packages/Acornima) from [NuGet](https://learn.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-using-the-dotnet-cli)

```bash
dotnet add package Acornima
```

Or, if you want to use additional features like JSX parsing, JavaScript generation from AST or AST to JSON conversion:

```bash
dotnet add package Acornima.Extras
```

#### 2. Import the *Acornima* namespace in your application

```csharp
using Acornima;
```

#### 3. Create a parser instance

```csharp
var parser = new Parser();
```

Or, if you want to tweak the available settings:

```csharp
var parser = new Parser(new ParserOptions { /* ... */ });
```

#### 4. Use the parser instance to parse your JavaScript code

```csharp
var ast = parser.ParseScript("console.log('Hello world!')");
```

### AST

```
Node [x]
 ├─AssignmentPattern : IDestructuringPatternElement [v,s]
 ├─CatchClause [v,s]
 ├─ClassBody [v,s]
 ├─ClassProperty : IClassElement, IProperty
 │  ├─AccessorProperty : IClassElement, IProperty [v,s]
 │  ├─MethodDefinition : IClassElement, IProperty [v,s]
 │  └─PropertyDefinition : IClassElement, IProperty [v,s]
 ├─Decorator [v,s]
 ├─DestructuringPattern : IDestructuringPatternElement
 │  ├─ArrayPattern : IDestructuringPatternElement [v,s]
 │  └─ObjectPattern : IDestructuringPatternElement [v,s]
 ├─ImportAttribute [v,s]
 ├─ModuleSpecifier
 │  ├─ExportSpecifier [v,s]
 │  └─ImportDeclarationSpecifier
 │     ├─ImportDefaultSpecifier [v,s]
 │     ├─ImportNamespaceSpecifier [v,s]
 │     └─ImportSpecifier [v,s]
 ├─Program : IHoistingScope [v]
 │  ├─Module : IHoistingScope [s,t=Program]
 │  └─Script : IHoistingScope [s,t=Program]
 ├─Property : IProperty
 │  ├─AssignmentProperty : IProperty [v,s,t=Property]
 │  └─ObjectProperty : IProperty [v,s,t=Property]
 ├─RestElement : IDestructuringPatternElement [v,s]
 ├─StatementOrExpression
 │  ├─Expression [x]
 │  │  ├─ArrayExpression [v,s]
 │  │  ├─ArrowFunctionExpression : IFunction [v,s]
 │  │  ├─AssignmentExpression [v,s]
 │  │  ├─AwaitExpression [v,s]
 │  │  ├─BinaryExpression [v]
 │  │  │  ├─LogicalExpression [s]
 │  │  │  └─NonLogicalBinaryExpression [s,t=BinaryExpression]
 │  │  ├─CallExpression : IChainElement [v,s]
 │  │  ├─ChainExpression [v,s]
 │  │  ├─ClassExpression : IClass [v,s]
 │  │  ├─ConditionalExpression [v,s]
 │  │  ├─FunctionExpression : IFunction [v,s]
 │  │  ├─Identifier : IDestructuringPatternElement [v,s]
 │  │  ├─ImportExpression [v,s]
 │  │  ├─Literal [v]
 │  │  │  ├─BigIntLiteral [s,t=Literal]
 │  │  │  ├─BooleanLiteral [s,t=Literal]
 │  │  │  ├─NullLiteral [s,t=Literal]
 │  │  │  ├─NumericLiteral [s,t=Literal]
 │  │  │  ├─RegExpLiteral [s,t=Literal]
 │  │  │  └─StringLiteral [s,t=Literal]
 │  │  ├─MemberExpression : IChainElement, IDestructuringPatternElement [v,s]
 │  │  ├─MetaProperty [v,s]
 │  │  ├─NewExpression [v,s]
 │  │  ├─ObjectExpression [v,s]
 │  │  ├─ParenthesizedExpression [v,s]
 │  │  ├─PrivateIdentifier [v,s]
 │  │  ├─SequenceExpression [v,s]
 │  │  ├─SpreadElement [v,s]
 │  │  ├─Super [v,s]
 │  │  ├─TaggedTemplateExpression [v,s]
 │  │  ├─TemplateLiteral [v,s]
 │  │  ├─ThisExpression [v,s]
 │  │  ├─UnaryExpression [v]
 │  │  │  ├─NonUpdateUnaryExpression [s,t=UnaryExpression]
 │  │  │  └─UpdateExpression [s]
 │  │  └─YieldExpression [v,s]
 │  └─Statement [x]
 │     ├─BlockStatement [v]
 │     │  ├─FunctionBody : IHoistingScope [v,s,t=BlockStatement]
 │     │  ├─NestedBlockStatement [s,t=BlockStatement]
 │     │  └─StaticBlock : IClassElement, IHoistingScope [v,s]
 │     ├─BreakStatement [v,s]
 │     ├─ContinueStatement [v,s]
 │     ├─DebuggerStatement [v,s]
 │     ├─Declaration [x]
 │     │  ├─ClassDeclaration : IClass [v,s]
 │     │  ├─FunctionDeclaration : IFunction [v,s]
 │     │  ├─ImportOrExportDeclaration
 │     │  │  ├─ExportDeclaration
 │     │  │  │  ├─ExportAllDeclaration [v,s]
 │     │  │  │  ├─ExportDefaultDeclaration [v,s]
 │     │  │  │  └─ExportNamedDeclaration [v,s]
 │     │  │  └─ImportDeclaration [v,s]
 │     │  └─VariableDeclaration [v,s]
 │     ├─DoWhileStatement [v,s]
 │     ├─EmptyStatement [v,s]
 │     ├─ExpressionStatement [v]
 │     │  ├─Directive [s,t=ExpressionStatement]
 │     │  └─NonSpecialExpressionStatement [s,t=ExpressionStatement]
 │     ├─ForInStatement [v,s]
 │     ├─ForOfStatement [v,s]
 │     ├─ForStatement [v,s]
 │     ├─IfStatement [v,s]
 │     ├─LabeledStatement [v,s]
 │     ├─ReturnStatement [v,s]
 │     ├─SwitchStatement [v,s]
 │     ├─ThrowStatement [v,s]
 │     ├─TryStatement [v,s]
 │     ├─WhileStatement [v,s]
 │     └─WithStatement [v,s]
 ├─SwitchCase [v,s]
 ├─TemplateElement [v,s]
 └─VariableDeclarator [v,s]
```

Legend:
* `v` - A visitation method is generated in the visitors for the node type.
* `s` - The node class is sealed. (It's [beneficial to check for sealed types](https://www.meziantou.net/performance-benefits-of-sealed-class.htm#casting-objects-is-a) when possible.)
* `t` - The node type (the value of the `Node.Type` property) as specified by ESTree (shown only if it differs from the name of the node class).
* `x` - The node class can be subclassed. (The AST provides some limited extensibility for special use cases.)

### JSX

The library also supports the syntax extension [JSX](https://facebook.github.io/jsx/).
However, mostly for performance reasons, the related functionality is separated from the core parser: it is available in the `Acornima.Extras` package, in the `Acornima.Jsx` namespace.

#### Installation & usage

After installing the `Acornima.Extras` package as described in the [Getting started](#getting-started) section, you can parse JSX code like this:

```csharp
using Acornima.Jsx;

var parser = new JsxParser(new JsxParserOptions { /* ... */ });

var ast = parser.ParseScript("<>Hello world!</>");
```

#### AST

```
Node [x]
 └─StatementOrExpression
    └─Expression [x]
       └─JsxNode [x]
          ├─JsxAttributeLike
          │  ├─JsxAttribute [v,s]
          │  └─JsxSpreadAttribute [v,s]
          ├─JsxClosingTag
          │  ├─JsxClosingElement [v,s]
          │  └─JsxClosingFragment [v,s]
          ├─JsxElementOrFragment
          │  ├─JsxElement [v,s]
          │  └─JsxFragment [v,s]
          ├─JsxEmptyExpression [v,s]
          ├─JsxExpressionContainer [v,s]
          ├─JsxName
          │  ├─JsxIdentifier [v,s]
          │  ├─JsxMemberExpression [v,s]
          │  └─JsxNamespacedName [v,s]
          ├─JsxOpeningTag
          │  ├─JsxOpeningElement [v,s]
          │  └─JsxOpeningFragment [v,s]
          └─JsxText [v,s]
```

### Migration from Esprima.NET

Projects using Esprima.NET can be converted to Acornima relatively easily as the public API of the two libraries are very similar.
(A pretty good proof of this statement is [this PR](https://github.com/sebastienros/jint/pull/1820), which migrates Jint to Acornima.)

The most notable changes to keep in mind with regard to migration are the following:

* The default value of the `ParserOptions.RegExpParseMode` property has been changed to `RegExpParseMode.Validate`.
* The default value of the `ParserOptions.RegexTimeout` property has been changed to 5 seconds.
* The default value of the `ParserOptions.Tolerant` property has been changed to `false`.
* The `Location` struct has been renamed to `SourceLocation`.
* The `TokenType` and `CommentType` enums have been renamed named to `TokenKind` and `CommentKind`, respectively. Also, some of the member names have been changed.
* The `Token` and `Comment` structs have been completely reworked. The `SyntaxToken` and `SyntaxComment` classes have been removed.
* The `SyntaxElement` class has been removed, that is, the `Node` class has become the root of the AST node type hierarchy. (This also means that tokens and comments are not attached to the root nodes of the AST. You can obtain those via the `ParserOptions.OnToken` and `ParserOptions.OnComment` callbacks).
* The `Nodes` enum has been renamed named to `NodeType`.
* The `Node.AssociatedData` property has been renamed to `UserData`.
* The `AssignmentOperator`, `BinaryOperator` and `UnaryOperator` enums have been merged into a single enum named `Operator`. Also, some of the member names have been changed.
* The `Literal` node class has been changed to only provide an `object? Value { get; }` property for accessing literal value. There are sealed subclasses for the different kinds of literals. Use those to access literal values in a type-safe (and more efficient) manner.
* The `Property` node class has been made abstract and two sealed subclasses have been introduced: `AssignmentProperty` and `ObjectProperty` (for representing properties of object destructuring patterns and object literals, respectively). Also, the `VisitProperty` method has been replaced with `VisitAssignmentProperty` and `VisitObjectProperty` in visitors.
* Similar changes have been made to the `BlockStatement` node class. Two new sealed subclasses have been introduced: `FunctionBody` and `NestedBlockStatement` (for representing bodies of function expressions/declarations and actual block statements that occurs within function bodies, respectively). Also, to conform to the ESTree spec, `StaticBlock` has been changed to be a subclass of `BlockStatement`. The `VisitBlockStatement` method has been kept in visitors, but only `NestedBlockStatement` is dispatched to it. The other two subclasses has dedicated visitation methods (`VisitFunctionBody` and `VisitStaticBlock`).
* The `ClassElement` node base class has been replaced with the `IClassElement` interface.
* The `Strict` property of function expression/declaration node classes has been moved to `FunctionBody`.
* The `JsxExpression` node class has been renamed to `JsxNode`.
* The `JsxElement` node class has been renamed to `JsxElementOrFragment` and two sealed subclasses have been introduced: `JsxElement` and `JsxFragment`.
* The `ParserException` class has been renamed to `ParseErrorException` and been made abstract. Two concrete subclasses (`SyntaxErrorException` and `RegExpConversionError`) have been introduced to indicate different kinds of errors that can occur during parsing.
* The message format of the `ParseErrorException` class has been changed. The reported messages are translatable text resources, so it is not recommended to rely on them to determine the reason of the error. For such purposes, you can use the `ParseErrorException.Error.Code` property.
* The `ParseErrorException.Column` property has been changed to store zero-based indices. (The exception message still includes one-based column indices though.)

### Benchmarks

| Method          | Runtime            | FileName            |      Mean |  Allocated |
|-----------------|--------------------|---------------------|----------:|-----------:|
| Acornima v1.0.0 | .NET 8.0           | angular-1.2.5       | 10.679 ms | 3978.22 KB |
| Acornima v1.0.0 | .NET Framework 4.8 | angular-1.2.5       | 22.905 ms | 3999.01 KB |
|                 |                    |                     |           |            |
| Esprima v3.0.5  | .NET 8.0           | angular-1.2.5       | 11.443 ms | 3828.11 KB |
| Esprima v3.0.5  | .NET Framework 4.8 | angular-1.2.5       | 20.483 ms | 3879.53 KB |
|                 |                    |                     |           |            |
| Acornima v1.0.0 | .NET 8.0           | backbone-1.1.0      |  1.428 ms |  629.26 KB |
| Acornima v1.0.0 | .NET Framework 4.8 | backbone-1.1.0      |  3.218 ms |  633.09 KB |
|                 |                    |                     |           |            |
| Esprima v3.0.5  | .NET 8.0           | backbone-1.1.0      |  1.440 ms |  613.88 KB |
| Esprima v3.0.5  | .NET Framework 4.8 | backbone-1.1.0      |  2.903 ms |   620.3 KB |
|                 |                    |                     |           |            |
| Acornima v1.0.0 | .NET 8.0           | jquery-1.9.1        |  8.066 ms | 3271.63 KB |
| Acornima v1.0.0 | .NET Framework 4.8 | jquery-1.9.1        | 18.210 ms | 3288.41 KB |
|                 |                    |                     |           |            |
| Esprima v3.0.5  | .NET 8.0           | jquery-1.9.1        |  8.391 ms | 3305.23 KB |
| Esprima v3.0.5  | .NET Framework 4.8 | jquery-1.9.1        | 16.456 ms | 3355.15 KB |
|                 |                    |                     |           |            |
| Acornima v1.0.0 | .NET 8.0           | jquery.mobile-1.4.2 | 14.253 ms | 5449.24 KB |
| Acornima v1.0.0 | .NET Framework 4.8 | jquery.mobile-1.4.2 | 29.750 ms | 5480.16 KB |
|                 |                    |                     |           |            |
| Esprima v3.0.5  | .NET 8.0           | jquery.mobile-1.4.2 | 14.566 ms | 5428.48 KB |
| Esprima v3.0.5  | .NET Framework 4.8 | jquery.mobile-1.4.2 | 27.084 ms | 5497.48 KB |
|                 |                    |                     |           |            |
| Acornima v1.0.0 | .NET 8.0           | mootools-1.4.5      |  6.735 ms |  2755.9 KB |
| Acornima v1.0.0 | .NET Framework 4.8 | mootools-1.4.5      | 14.818 ms | 2771.45 KB |
|                 |                    |                     |           |            |
| Esprima v3.0.5  | .NET 8.0           | mootools-1.4.5      |  6.877 ms | 2777.83 KB |
| Esprima v3.0.5  | .NET Framework 4.8 | mootools-1.4.5      | 13.740 ms | 2816.33 KB |
|                 |                    |                     |           |            |
| Acornima v1.0.0 | .NET 8.0           | underscore-1.5.2    |  1.214 ms |  529.61 KB |
| Acornima v1.0.0 | .NET Framework 4.8 | underscore-1.5.2    |  2.775 ms |  532.29 KB |
|                 |                    |                     |           |            |
| Esprima v3.0.5  | .NET 8.0           | underscore-1.5.2    |  1.235 ms |  539.42 KB |
| Esprima v3.0.5  | .NET Framework 4.8 | underscore-1.5.2    |  2.501 ms |  547.18 KB |
|                 |                    |                     |           |            |
| Acornima v1.0.0 | .NET 8.0           | yui-3.12.0          |  6.408 ms | 2611.82 KB |
| Acornima v1.0.0 | .NET Framework 4.8 | yui-3.12.0          | 13.831 ms | 2628.61 KB |
|                 |                    |                     |           |            |
| Esprima v3.0.5  | .NET 8.0           | yui-3.12.0          |  6.667 ms | 2585.78 KB |
| Esprima v3.0.5  | .NET Framework 4.8 | yui-3.12.0          | 12.636 ms | 2624.92 KB |

### Known issues and limitations

#### Regular expressions

The parser can be configured to convert JS regular expression literals to .NET `Regex` instances (see `ParserOptions.RegExpParseMode`).
However, because of the fundamental differences between the JS and .NET regex engines, in many cases this conversion can't be done perfectly (or, in some cases, at all): 

* Case-insensitive matching [won't always yield the same results](https://github.com/adams85/acornima/blob/488e55472113af21e31cbc24a79c18b01d23dcc7/src/Acornima/Tokenizer.RegExpParser.cs#L99). Implementing a workaround for this issue would be extremely hard, if not impossible.
* The JS regex engine assigns numbers to capturing groups sequentially (regardless of the group being named or not named) but [.NET uses a different, weird approach](https://learn.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#grouping-constructs-and-regular-expression-objects): "Captures that use parentheses are numbered automatically from left to right based on the order of the opening parentheses in the regular expression, starting from 1. However, named capture groups are always ordered last, after non-named capture groups." Without some adjustments, this would totally mess up numbered backreferences and replace pattern references. So, as a workaround, the converter wraps all named capturing groups in a non-named capturing group to force .NET to include all the original capturing groups in the resulting match in the expected order. (Of course, this won't prevent named groups from being listed after the numbered ones.) If needed, the original number of groups can be obtained from the returned `RegExpParseResult` object's `ActualRegexGroupCount` property.
* The characters allowed in group names differs in the two regex engines. For example a the group name `$group` is valid in JS but invalid in .NET. So, as a workaround, the converter [encodes the problematic group names](https://github.com/adams85/acornima/blob/488e55472113af21e31cbc24a79c18b01d23dcc7/src/Acornima/Tokenizer.RegExpParser.cs#L1041) to names that are valid in .NET and probably won't collide with other group names present in the pattern. For example, `$group` is encoded like `__utf8_2467726F7570`. The original group names can be obtained using the returned `RegExpParseResult` object's `GetRegexGroupName` method.
* Self-referencing capturing groups like `/((a+)(\1) ?)+/` may not produce the exact same captures. [`RegexOptions.ECMAScript` is supposed to cover this](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#ecmascript-matching-behavior), however even the MSDN example doesn't produce the same matches. (As a side note, `RegexOptions.ECMAScript` is kinda a false promise, it can't even get some basic cases right by itself.)
* Similarily, repeated nested groups like `/((a+)?(b+)?(c))*/` may produce different captures for the groups. ([JS has an overwrite behavior](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Capturing_group#description) while .NET doesn't).
* .NET treats forward references like `\1(\w)` differently than JS and it's not possible to convert this kind of patterns reliably. (The converter could make some patterns work by rewriting them to something like `(?:)(\w)` but there are cases where even this wouldn't work.)
* Unicode mode issues:
  * There could be false positive empty string matches in the middle of surrogate pairs. Patterns as simple as `/a?/u` will cause this issue when the input string contains astral Unicode chars. There is no out-of-the-box workaround for this issue but it can be mitigated somewhat using a bit of "post-processing", i.e., by filtering out the false positive matches after evaluation like it's done [here](https://github.com/adams85/acornima/blob/488e55472113af21e31cbc24a79c18b01d23dcc7/test/Acornima.Tests/RegExpTests.Fixtures.cs#L112). Probably there is no way to improve this situation until .NET adds the option to treat the input string as Unicode code points.
  * Support for Unicode property escapes is pretty limited (see [explanation](https://github.com/adams85/acornima/blob/488e55472113af21e31cbc24a79c18b01d23dcc7/src/Acornima/Tokenizer.RegExpParser.Unicode.cs#L871)). Currently, only General Category expressions are converted. But even this is not perfect as the result will depend the Unicode version included in the specific .NET runtime which is executing the parser's code.

To sum it up, legacy pattern conversion is pretty solid apart from the minor issues listed above. However, support for unicode mode (flag u) patterns is partial and quirky, while conversion of the upcoming unicode sets mode (flag v) will be next to impossible - until the .NET regex engine gets some improved Unicode support.

### *Any feedback appreciated, contributions are welcome!*