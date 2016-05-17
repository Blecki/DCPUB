/*
*    _________.________________.      __.___  ____________________
*    \______   \   \__    ___/  \    /  \   |/   _____/\_   _____/
*     |    |  _/   | |    |  \   \/\/   /   |\_____  \  |    __)_ 
*     |    |   \   | |    |   \        /|   |/        \ |        \ 
*     |______  /___| |____|    \__/\  / |___/_______  //_______  /
*            \/                     \/              \/         \/ 
*
*                (c) BITWISE SCIENCES 1984 - 2108
*
*                           PROGRAMMING IN B
*
*/

/* B is a language designed to make programming easier, without losing the
   power and flexibility associated with assembly language programming for 
   the DCPU. */
   
// The preprocessor allows B programs to include others, as if the entire
// source text of the included file had been copied into this file.
// The preprocessor is capable of much more, but that is a topic for a 
// later section.. for now, we need to include the default environment so
// we can display the results of our excercises.
#include default_environment.b

// Section 1: Variables.

// A variable is a named value that can change, and are declared by stating
// the /variable type/, followed by a name and an optional initialization.
local x = 3;
local y;

// Assignment is accomplished with the '=' operator.
y = 4;

// There are three types of variables. Static, local, and extern. Static
// variables are stored in a specific place in memory and can be accessed
// from anywhere. Local variables are always stored on the stack.
static z = 5;

// Static variables must be initialized with a static value. Uncomment the 
// next line to see the error message an improper initialization produces.
// static causes_error = x * y;

// Lets do some simple operations with our variables. We'll print some 
// numbers too. printf is declared by default_environment.b and is useful
// for getting some numbers onto the screen quickly.

printf("X = %N, Y = %N\n", x, y);
z = x * y;
printf("Z = %N\n", z);

// Section 2: Expressions

// Expression are any series of operations applied to one or more variables 
// or function calls. Expression cannot exist on their own. They are always
// part of a larger statement.

// Expressions can use many operators, such as +, -, *, /, &, | and ^.
// Some of these operators can also be combined with the = operator to
// perform an operation and an assignment in a single line.

// Section 3: Functions

// Section 4: Pointers

// The * operator is a unary operator that treat's its operand as a memory
// pointer and retreives the value at that location in memory. Operator * can
// also appear on the left hand side of operator =, in which case the 
// value at the that location in memory is overwritten rather than retreived.

local ptr = &z;
*ptr = 0xBEEF;
printf("*%X = %X\n", ptr, *ptr);

// The & operator is a unary operator that returns the address of it's operand.

// Index-syntax alows the programmer to write operations such as *(a + b) more 
// concisely as a[b]. The index syntax can be applied to any value.
