# DCPUB syntax

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

- DCPUB includes if/else statements and while loops. Both will be simplified if the conditional is a constant expression. If statements may be simplified further if the body of the else or then block is a single instruction.
- If the conditional results in 0, it fails. Any other value is deemed to be true.
- If statements can be chained, however there is no 'elseif' keyword. Put a space between the else and the chained if statement as in 'if (a) {} else if (b) {}'

### ASM blocks

An ASM block allows the programmer to write assembly code in their DCPUB program. This is useful for places where DCPUB provides no abstraction over the hardware. For example, accessing hardware devices like the Lem display. An ASM block looks like

    asm (A = expression)
    {
    	ADD A, 2
    }

'A' can be any DCPU register name. 'expression' is evaluated and assigned to that register. Any of the registers can be used to pass values into the asm block, however, using J is bad practice. J is used by the language as a frame pointer and assigning to J can make the register assignments behave in odd ways. It's a good idea to declare all the registers you will be using in the assembly block in the asm block header, even if you only assign 0 to them. This will let DCPUB know you've used these registers so they can be properly preserved by the enclosing function.
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

In DCPUB, any expression can be dereferenced. It looks like this: *(a). The result of a is treated as a pointer.

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
