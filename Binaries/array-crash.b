struct task
{
  env:env_struct;
  pc;
  proc:process;
  mem_block:mblock;
  parent:task;
  flags;
  stack[64];
  stackptr;
};

struct process
{
  root_env:env_elem;
  main_thread:task;
  portmap_internal[256];
  perms;
  err;
  mem_allocd;
  last_malloc;
  running_tasks:task[16];
};
