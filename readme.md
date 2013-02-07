DCPUC, A C-like language for the DCPU-16, the 16-bit 'hardware' in Mojang's game 0x10c. 
This compiler targets DCPU assembly. http://pastebin.com/raw.php?i=Q4JvQvnM

DCPUC quick start -
Download 'DCPUC release.zip' from the repository. Within you will find pre-built versions of the preprocessor (bin/pre.exe) and the compiler (bin/b.exe).
Preprocess the helloworld sample with "pre helloworld.dc helloworld.pdc".
Compile with "b -in "helloworld.pdc" -out "helloworld.dasm"".
The compiler can produce binary output with the -binary flag. It defaults to little-endian, but will produce big-endian output with the -be flag.


# Features

* Top-level code. Execution of a DCPUC program begins at the top of the file. 
* Separate preprocessor. The preprocessor is a separate executable. You can choose to invoke it or not, and you can modify it's output before you compile it.
* Peephole optimizations. Supply the '-peepholes definition-file' switch to the compiler to enable peephole optimizations with a definition file you supply. A basic definition file is in Binaries.

# Thinking the DCPUC way

DCPUC is not a safe language. It is a very, very dangerous language. It makes no attempt to protect the programmer from his mistakes. The language is based on B, the predecessor to C. Some features have been added and some have been removed to put the language more in-line with the target processor. In the general case, if there were two ways to do something in DCPUC, one of those ways was removed. If error checking kept a DCPUC program from being written elegantly, the error checking was removed. A good example of this is the implementation of function pointers: that is, there isn't one. Function pointers were 'implemented' by allowing operator() to be applied to any expression. Possibly the single biggest design goal for this language is to avoid supporting anything at the language level that the DCPU can't do natively. The DCPU only has instructions for one size of value, so DCPUC only works with one size of value. The DCPU doesn't have virtual memory, so DCPUC doesn't manage any memory. The list goes on. However, all of these things can be done with libraries - possibly written in DCPUC itself. In fact, you'll find a simple allocator in Binaries/memory.dc. This allocator reflects another design principle of DCPUC. It doesn't create a heap in all available memory, it creates one in some block of memory you give it. You can place the heap where you like, and even have more of them. A good idea, if you want to create lots of the same small object, is to create a heap just for them. This will avoid the problem of heap fragmentation, because all of your free blocks will be the same size.

# DCPUC syntax

## Statements

Statements take only a few forms. 
-An assignment; expression = expression, where the first expression is an lvalue, such as a variable name or the result of dereferencing. Operation forms such as += and *= are also supported.
-A declaration, either of a variable or function.
-A control-flow statement; if or while.
-A label. Labels are an identifier proceeded by a colon, such as :label. Labels can't be part of another statement - the parser will confuse them with type-aliases.
-A return, break, or goto statement.
-An ASM block.

### Variable declarations : modifier name (:type alias) ([array size]) = initial value;

- 'modifier' can be either static or local. Local variables are allocated on the stack. Static variables are allocated in a static memory location.
- Type aliases are entirely optional. Their purpose is to allow the easy access of struct members. Variables are not typed and types are not checked.
- If declaring an array, the array size must be a compile-time constant.
- The initial value of static variables must be a compile-time constant.
- An array can be initialized with the syntax local foo[2] = { a, b };. All values within the array must be compile-time constants.

### Function declarations : function name(parameters)(:type alias) { code; }

- Parameters take the form of name (:optional type alias). Parameter types aren't checked, because variables don't have types. Parameter count is checked.
- A type alias on a function allows you to apply operator. to the results of the function.
- All functions are assumed to return a value. If the function doesn't return anything meaningful, just ignore it.

### Struct declarations : struct name { members; }

- Members are just names with an optional type-alias. They cannot be initialized, nor do they have modifiers. They can be arrays.

### Control flow

- DCPUC includes if/else statements and while loops. Both will be simplified if the conditional is a constant expression. If statements may be simplified further if the body of the else or then block is a single instruction.
- If the conditional results in 0, it fails. Any other value is deemed to be true.
- If statements can be chained, however there is no 'elseif' keyword. Put a space between the else and the chained if statement as in 'if (a) {} else if (b) {}'

### ASM blocks

An ASM block allows the programmer to write assembly code in their DCPUC program. This is useful for places where DCPUC provides no abstraction over the hardware. For example, accessing hardware devices like the Lem display. An ASM block looks like

    asm (A = expression)
    {
    	ADD A, 2
    }

'A' can be any DCPU register name. 'expression' is evaluated and assigned to that register. Any of the registers can be used to pass values into the asm block, however, using J is bad practice. J is used by the language as a frame pointer and assigning to J can make the register assignments behave in odd ways. It's a good idea to declare all the registers you will be using in the assembly block in the asm block header, even if you only assign 0 to them. This will let DCPUC know you've used these registers so they can be properly preserved by the enclosing function.
ASM blocks support labels and dat statements as well.

## Expressions

An expression is the basic building block of code. They can be any of
-A variable or function name.
-A binary operation.
-A unary operation.
-A dereference.
-An indexing.
-A member access.
-A cast.
-An address taking.
-A function call.

### Binary operations

A binary operation takes the form expression operator expression. The supported operators are
- + : Addition
- - : Subtraction
- * : Multiplication
- / : Division
- % : Modulus
- -+ : Signed addition
- -- : Signed subtraction
- -* : Signed multiplication
- -/ : Signed division
- -% : Signed modulus
- == : Equality
- != : Inequality
- > : Greater than
- -> : Signed greater than
- < : Less than
- -< : Signed less than
- >> : Shift right
- << : Shift left
- & : Binary and
- | : Binary or
- ^ : Binary xor

### Unary operations

A unary operation takes the form operator expression. The supported operators are
- ! : Binary not
- - : Negate. This compiles to an xor with 0x8000.
- * : Dereference. See dereferencing.
- & : Address-of. See address taking.

### Dereferencing

In DCPUC, any expression can be dereferenced. It looks like this: *(a). The result of a is treated as a pointer.

   local a = 5;
   local b = &a;
   local c = *b;

In this example, c now equals 5. 

### Indexing

An indexing looks like this: a[b]. This is transformed in the syntax tree to *(a + b). The index operator can be applied to any expression.

### Member access

The members of a struct can be accessed using operator. It looks like this: a.member-name. Member-name must be the name of a member and a must be type-aliased. A variable name is type-aliased if the variable is type-aliased; the result of a function call will also be type-aliased if the function is. An index operation carries the type-alias of the indexed item. If you need to access struct members and the pointer to the struct isn't aliased, you can always cast it.
- Member access treats the item being accessed as if it is a pointer. 'a.b' will compile to '*(a + offsetof(type-alias-of(a), b))'

### Casts

Casts look just like a type-alias in a declaration. a:struct-name. Casts add nothing to the compiled code; they exist to allow operator. to be applied to arbitrary expressions.

### Address-taking

The address can be taken of variables and functions. &variable-or-function-name. 

### Function calls

Function calls look like this: a(arguments).
- a can be any expression. The result of the expression is treated as the address of the function and will be jumped to.
- If a is the name of a function, the number of arguments will be checked, and the compiler will issue an error if an incorrect number is supplied.

## The preprocessor

The preprocessor is very basic. It supports uncomplicated defines with or without arguments. You can also include other files. It supports conditional compilation with #ifdef/#ifndef/#endif.