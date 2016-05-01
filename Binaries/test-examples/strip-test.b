function a() {}
function b() { c(); }
function c() {}
function d() { e(); }
function e() { local a = &f; }
function f() { }
function g() { h(); }
function h() { g(); }


c();
d();


