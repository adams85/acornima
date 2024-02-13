# Acorn + Esprima = Acornima 

This project is an interbreeding of the [acornjs](https://github.com/acornjs/) and the [Esprima.NET](https://github.com/sebastienros/esprima-dotnet) parsers, with the intention of creating an even more complete and performant ECMAScript (a.k.a JavaScript) parser library by combining the best bits of those.

(It's also worth mentioning that there is an earlier, unmaintained .NET port of acornjs, [AcornSharp](https://github.com/MatthewSmit/AcornSharp), which served as a good starting point. If it weren't for AcornSharp, this project probably have never started.)

### Here is how this Frankenstein's monster looks like:

* The tokenizer is mostly a direct translation of the acornjs tokenizer to C# (with many smaller and bigger performance improvements, partly inspired by Esprima.NET), apart from the regex validation/conversion logic, which has been borrowed from Esprima.NET.
* The parser is ~99% acornjs (also with a bunch of minor improvements) and ~1% Esprima.NET (strict mode detection, public API).
* Both projects follow the ESTree specification, so is Acornima. The actual implementation is based on that of Esprima.NET, with further minor improvements to the class hierarchy that bring it even closer to the spec and allow encoding a bit more information.
* The built-in AST visitors and additional utility functionality stems from Esprima.NET as well.

### And what good comes out of this mix?

* A parser which already matches the performance of Esprima.NET, while doing more: it also passes the **complete** [Test262](https://github.com/tc39/test262) test suite for ECMAScript 2023.
* It is also more economic with regard to stack usage, so it can parse ~1.5x deeper structures.
* More options for fine-tuning parsing.
* As the parser tracks variable scopes to detect variable redeclarations, it will be possible to expose this information to the consumer.

### Benchmarks:

| Method         | Runtime            | FileName            | Mean      | Allocated  |
|----------------|--------------------|---------------------|-----------|------------|
| Acornima-dev   | .NET 8.0           | angular-1.2.5       | 10.576 ms | 4062.79 KB |
| Acornima-dev   | .NET Framework 4.8 | angular-1.2.5       | 21.935 ms | 4083.74 KB |
|                |                    |                     |           |            |
| Esprima-v3.0.4 | .NET 8.0           | angular-1.2.5       | 11.214 ms | 3828.11 KB |
| Esprima-v3.0.4 | .NET Framework 4.8 | angular-1.2.5       | 20.684 ms | 3879.54 KB |
|                |                    |                     |           |            |
| Acornima-dev   | .NET 8.0           | backbone-1.1.0      | 1.408 ms  | 638.72 KB  |
| Acornima-dev   | .NET Framework 4.8 | backbone-1.1.0      | 3.226 ms  | 642.58 KB  |
|                |                    |                     |           |            |
| Esprima-v3.0.4 | .NET 8.0           | backbone-1.1.0      | 1.465 ms  | 613.88 KB  |
| Esprima-v3.0.4 | .NET Framework 4.8 | backbone-1.1.0      | 2.917 ms  | 620.3 KB   |
|                |                    |                     |           |            |
| Acornima-dev   | .NET 8.0           | jquery-1.9.1        | 8.221 ms  | 3322.58 KB |
| Acornima-dev   | .NET Framework 4.8 | jquery-1.9.1        | 18.009 ms | 3339.42 KB |
|                |                    |                     |           |            |
| Esprima-v3.0.4 | .NET 8.0           | jquery-1.9.1        | 8.469 ms  | 3305.23 KB |
| Esprima-v3.0.4 | .NET Framework 4.8 | jquery-1.9.1        | 16.542 ms | 3355.15 KB |
|                |                    |                     |           |            |
| Acornima-dev   | .NET 8.0           | jquery.mobile-1.4.2 | 14.038 ms | 5499.24 KB |
| Acornima-dev   | .NET Framework 4.8 | jquery.mobile-1.4.2 | 29.629 ms | 5530.42 KB |
|                |                    |                     |           |            |
| Esprima-v3.0.4 | .NET 8.0           | jquery.mobile-1.4.2 | 14.599 ms | 5428.48 KB |
| Esprima-v3.0.4 | .NET Framework 4.8 | jquery.mobile-1.4.2 | 27.261 ms | 5497.48 KB |
|                |                    |                     |           |            |
| Acornima-dev   | .NET 8.0           | mootools-1.4.5      | 6.695 ms  | 2812.26 KB |
| Acornima-dev   | .NET Framework 4.8 | mootools-1.4.5      | 14.633 ms | 2828 KB    |
|                |                    |                     |           |            |
| Esprima-v3.0.4 | .NET 8.0           | mootools-1.4.5      | 7.034 ms  | 2777.83 KB |
| Esprima-v3.0.4 | .NET Framework 4.8 | mootools-1.4.5      | 13.754 ms | 2816.33 KB |
|                |                    |                     |           |            |
| Acornima-dev   | .NET 8.0           | underscore-1.5.2    | 1.158 ms  | 541.81 KB  |
| Acornima-dev   | .NET Framework 4.8 | underscore-1.5.2    | 2.782 ms  | 544.51 KB  |
|                |                    |                     |           |            |
| Esprima-v3.0.4 | .NET 8.0           | underscore-1.5.2    | 1.229 ms  | 539.42 KB  |
| Esprima-v3.0.4 | .NET Framework 4.8 | underscore-1.5.2    | 2.516 ms  | 547.18 KB  |
|                |                    |                     |           |            |
| Acornima-dev   | .NET 8.0           | yui-3.12.0          | 5.867 ms  | 2638.28 KB |
| Acornima-dev   | .NET Framework 4.8 | yui-3.12.0          | 13.651 ms | 2655.09 KB |
|                |                    |                     |           |            |
| Esprima-v3.0.4 | .NET 8.0           | yui-3.12.0          | 6.488 ms  | 2585.78 KB |
| Esprima-v3.0.4 | .NET Framework 4.8 | yui-3.12.0          | 12.365 ms | 2624.92 KB |

### What's missing currently:
* License (considering MIT or BSD-3-Clause, but need to discuss this with the Esprima.NET guys)
* Implementation of some experimental features (decorators, import attributes)
* Moving messages into resources and replacing acorn messages with V8 messages
* Support for JSX
* AST to JSON, AST to source code conversion
* Porting additional tests from acornjs and Esprima.NET
* CI
