static a;
static b[2] = { 0xDEAD, 0xBEEF };
static c = &a;
static d[2] = { &a, b };
function e() {}
static f = &e;
static g = "abcdef";
static h[3] = { d, &e, &a };
local i[2] = { 1, 2 };
local j[1] = { 3 };

