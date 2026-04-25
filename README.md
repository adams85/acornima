[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/adams85/acornima/build.yml)](https://github.com/adams85/acornima/actions/workflows/build.yml)
[![NuGet Release](https://img.shields.io/nuget/v/Acornima)](https://www.nuget.org/packages/Acornima)
[![Feedz Version](https://img.shields.io/feedz/v/acornima/acornima/Acornima)](https://feedz.io/org/acornima/repository/acornima/packages/Acornima)
[![Donate](https://img.shields.io/badge/-buy_me_a%C2%A0coffee-gray?logo=buy-me-a-coffee)](https://www.buymeacoffee.com/adams85)

[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://stand-with-ukraine.pp.ua)

# Acorn + Esprima = Acornima

This project is a crossbreeding of the [acornjs](https://github.com/acornjs/) and the [Esprima.NET](https://github.com/sebastienros/esprima-dotnet) parsers, with the intention of creating an even more complete and performant ECMAScript (a.k.a JavaScript) parser library for .NET by combining the best bits of those.

It should also be mentioned that there is an earlier .NET port of acornjs, [AcornSharp](https://github.com/MatthewSmit/AcornSharp), which though is unmaintained for a long time, served as a good starting point. Had it not been for AcornSharp, this project would probably have never started.

### Here is how this Frankenstein's monster looks like:

* The tokenizer is mostly a direct translation of the acornjs tokenizer to C# (with many bigger and smaller performance improvements, partly inspired by Esprima.NET) - apart from some parts of the regex validation logic, which have been borrowed from Esprima.NET.
* The parser is ~99% acornjs (also with a bunch of minor improvements) and ~1% Esprima.NET (strict mode detection, public API). It is also worth mentioning that the error reporting has been changed to use the error messages of V8.
* It includes protection against the non-catchable `StackOverflowException` using [the same approach](https://github.com/adams85/acornima/blob/v1.0.0/src/Acornima/Helpers/StackGuard.cs) as Roslyn.
* Both parent projects follow the ESTree specification, so does Acornima. The actual AST implementation is based on that of Esprima.NET, with further minor improvements to the class hierarchy that bring it even closer to the spec and allow encoding a bit more information.
* The built-in AST visitors and additional utility features stems from Esprima.NET as well.

### And what good comes out of this mix?

* A parser that matches the performance of Esprima.NET while doing more: it also passes the **complete** [Test262 test suite](https://github.com/tc39/test262) for ECMAScript 2026.
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

* The `ParserOptions.RegExpParseMode` and `ParserOptions.RegexTimeout` properties (along with the ability to convert JS regular expressions to .NET `Regex` instances) have been removed. Regular expressions are validated by default, but you can override this behavior using `ParserOptions.OnRegExp`. (To replicate `RegExpParseMode.Skip`, return `default(RegExpParseResult)` from the callback. To replicate `RegExpParseMode.AdaptTo...`, you can use the [regexp2regex](https://github.com/adams85/regexp2regex) library.)
* The default value of the `ParserOptions.Tolerant` property has been changed to `false`.
* The `Location` struct has been renamed to `SourceLocation`.
* The `TokenType` and `CommentType` enums have been renamed named to `TokenKind` and `CommentKind`, respectively. Also, some of the member names have been changed.
* The `Token` and `Comment` structs have been completely reworked. The `SyntaxToken` and `SyntaxComment` classes have been removed.
* The `SyntaxElement` class has been removed, that is, the `Node` class has become the root of the AST node type hierarchy. (This also means that tokens and comments are not attached to the root nodes of the AST. You can obtain those via the `ParserOptions.OnToken` and `ParserOptions.OnComment` callbacks).
* The `Nodes` enum has been renamed to `NodeType`.
* The `Node.AssociatedData` property has been renamed to `UserData`.
* The `AssignmentOperator`, `BinaryOperator` and `UnaryOperator` enums have been merged into a single enum named `Operator`. Also, some of the member names have been changed.
* The `Literal` node class has been changed to only provide an `object? Value { get; }` property for accessing literal value. There are sealed subclasses for the different kinds of literals. Use those to access literal values in a type-safe (and more efficient) manner.
* The `Property` node class has been made abstract and two sealed subclasses have been introduced: `AssignmentProperty` and `ObjectProperty` (for representing properties of object destructuring patterns and object literals, respectively). Also, the `VisitProperty` method has been replaced with `VisitAssignmentProperty` and `VisitObjectProperty` in visitors.
* Similar changes have been made to the `BlockStatement` node class. Two new sealed subclasses have been introduced: `FunctionBody` and `NestedBlockStatement` (for representing bodies of function expressions/declarations and actual block statements that occurs within function bodies, respectively). Also, to conform to the ESTree spec, `StaticBlock` has been changed to be a subclass of `BlockStatement`. The `VisitBlockStatement` method has been kept in visitors, but only `NestedBlockStatement` is dispatched to it. The other two subclasses has dedicated visitation methods (`VisitFunctionBody` and `VisitStaticBlock`).
* The `ClassElement` node base class has been replaced with the `IClassElement` interface.
* The `Strict` property of function expression/declaration node classes has been moved to `FunctionBody`.
* The `JsxExpression` node class has been renamed to `JsxNode`.
* The `JsxElement` node class has been renamed to `JsxElementOrFragment` and two sealed subclasses have been introduced: `JsxElement` and `JsxFragment`.
* The `ParserException` class has been renamed to `ParseErrorException` and been made abstract. A concrete subclass named `SyntaxErrorException` has been introduced to indicate syntax errors.
* The message format of the `ParseErrorException` class has been changed. The reported messages are translatable text resources, so it is not recommended to rely on them to determine the reason of the error. For such purposes, you can use the `ParseErrorException.Error.Code` property.
* The `ParseErrorException.Column` property has been changed to store zero-based indices. (The exception message still includes one-based column indices though.)

### Benchmarks

```
BenchmarkDotNet v0.15.6, Windows 10
AMD Ryzen 7 7735HS with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.201
  [Host]    : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3

Job=.NET 10.0  Runtime=.NET 10.0  Toolchain=net10.0  
IterationCount=15  LaunchCount=2  WarmupCount=10  
```

| Method          | FileName            | Mean        | Allocated  |
|-----------------|---------------------|-------------|------------|
| Acornima v1.4.0 | angular-1.2.5       | 6,536.1 μs  | 3970.92 KB |
| Esprima v3.0.5  | angular-1.2.5       | 6,535.1 μs  | 3828.1 KB  |
|                 |                     |             |            |
| Acornima v1.4.0 | angular-1.7.9       | 14,729.4 μs | 6719.11 KB |
| Esprima v3.0.5  | angular-1.7.9       | 14,329.3 μs | 6575.47 KB |
|                 |                     |             |            |
| Acornima v1.4.0 | backbone-1.1.0      | 806.1 μs    | 628.3 KB   |
| Esprima v3.0.5  | backbone-1.1.0      | 824.9 μs    | 613.88 KB  |
|                 |                     |             |            |
| Acornima v1.4.0 | jquery-1.9.1        | 4,784.7 μs  | 3263.86 KB |
| Esprima v3.0.5  | jquery-1.9.1        | 5,167.4 μs  | 3305.23 KB |
|                 |                     |             |            |
| Acornima v1.4.0 | jquery.mobile-1.4.2 | 8,588.6 μs  | 5443.43 KB |
| Esprima v3.0.5  | jquery.mobile-1.4.2 | 8,190.9 μs  | 5428.44 KB |
|                 |                     |             |            |
| Acornima v1.4.0 | mootools-1.4.5      | 3,923.1 μs  | 2750.82 KB |
| Esprima v3.0.5  | mootools-1.4.5      | 4,089.0 μs  | 2777.83 KB |
|                 |                     |             |            |
| Acornima v1.4.0 | underscore-1.5.2    | 683.6 μs    | 528.92 KB  |
| Esprima v3.0.5  | underscore-1.5.2    | 702.5 μs    | 539.41 KB  |
|                 |                     |             |            |
| Acornima v1.4.0 | yui-3.12.0          | 3,618.3 μs  | 2607.91 KB |
| Esprima v3.0.5  | yui-3.12.0          | 3,674.8 μs  | 2585.77 KB |

### *Any feedback appreciated, contributions are welcome!*
