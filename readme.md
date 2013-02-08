# DCPUB

DCPUB is a language similar to [B](http://en.wikipedia.org/wiki/B_%28programming_language%29). This compiler targets the
1.7 [DCPU-16 specification]([DCPU-16 specification](http://pastebin.com/raw.php?i=Q4JvQvnM).

## Features

* A straight forward language with familiar C-like syntax.
* Predictable and effecient assembly generation.
* Top-level code. Execution of a DCPUB program begins at the top of the file, not in an arbitrary main function. This allows
  library code to initialize static variables without resorting to hacks to insert code at program start.
* Separate preprocessor. The preprocessor is a separate executable. You can choose to invoke it or not, and you can modify
  its output before you compile it.
* A minimalist standard library including display and keyboard abstractions, a memory allocator, and a basic implementation
  of printf.
* Peephole optimizations. Supply the '-peepholes definition-file' switch to the compiler to enable peephole optimizations
  with a definition file you supply. A basic definition file is included in the binary release.

## Installation

[Download Latest Version](https://github.com/Blecki/DCPUB/blob/master/DCPUB%20release.zip?raw=true)

Included in this archive:

* DCPUB Compiler
* DCPUB Preprocessor
* Sample code

On Linux and Mac, install Mono and prepend all commands with `mono`.

## Building from Source

DCPUB is written in C#, and may be built with Microsoft.NET on Windows, or Mono on Linux/Mac.

**MS.NET**: Add Microsoft.NET to your %PATH% and run `msbuild` from the root of the repository.

**Mono**: Run `xbuild` from the root of the repository.

## Usage

### b - Compiler

Compiles DCPUB source to assembly or machine language.

    b [flags]

**Flags**:

* -in "filename": Specify the file to be compiled. Not optional.
* -out "filename": Specify the file to write the compiled program to. Not optional.
* -binary : Emit a binary file. If not supplied, the compiler will emit unassembled DCPU assembly.
* -be : Emit in big endian. Only valid if paired with -binary. If not supplied, binary output will be in little endian.

### pre - Preprocessor

Expands macros and file inclusions

    pre input.b output.b

pre has no switches or options.

Preprocessor directives will cause errors in the compiler. If they are used in the program, it must be preprocessed first.

## Getting help

If you find bugs, need assistance, or otherwise encounter trouble, feel free to [make an issue](https://github.com/Blecki/DCPUB/issues) on GitHub.
