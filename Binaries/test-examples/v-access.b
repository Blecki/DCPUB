struct sample_struct
{
  a;
  b;
  c;
};

static ss1:sample_struct;

static v1;

local ss2:sample_struct;

local v2;

//sequential sets in one static struct:
ss1.a = 1;
ss1.b = 2;
ss1.c = 3;
//editing static struct values based on their previous values
ss1.c = ss1.c + 1;

//set on a static variable
v1 = 1;
//editing a static variable's value based on its previous value
v1 = v1 + 1;

//sequential sets in one local struct
ss2.a = 1;
ss2.b = 2;
ss2.c = 3;
//editing local struct values based on their previous values
ss2.c = ss2.c + 1;

//set on a local veriable
v2 = 1;
//editing a local variable based on its previous value
v2 = v2 + 1;
