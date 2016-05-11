/*
*    _________.________________.      __.___  ____________________
*    \______   \   \__    ___/  \    /  \   |/   _____/\_   _____/
*     |    |  _/   | |    |  \   \/\/   /   |\_____  \  |    __)_ 
*     |    |   \   | |    |   \        /|   |/        \ |        \ 
*     |______  /___| |____|    \__/\  / |___/_______  //_______  /
*            \/                     \/              \/         \/ 
*
*       (c) BITWISE SCIENCES 1984 - 2108
*
*       COPY DISC 
*
*       A utility program for copying a disc. The device must have two m35fd 
*       devices with appropriate media.
*
*/

#include default_environment.b
#include m35fd.b

local drive_ids[2];
m35fd_enumerate(drive_ids, 2);

if ((drive_ids[0] == 0) | (drive_ids[1] == 0))
{
   printf("YOU MUST HAVE TWO DRIVES TO COPY DISCS");
   while (1) {}
}

printf("REMOVE BOOT DISC\n");

local ds = m35fd_poll_state(drive_ids[0]);
while (ds != M35FD_STATE_NO_MEDIA) ds = m35fd_poll_state(drive_ids[0]);

printf("INSERT SOURCE DISC IN FIRST DRIVE\n");

ds = m35fd_poll_state(drive_ids[0]);
while (ds != M35FD_STATE_READY) ds = m35fd_poll_state(drive_ids[0]);

printf("INSERT DESTINATION DISC IN SECOND DRIVE\n");

ds = m35fd_poll_state(drive_ids[1]);
while (ds != M35FD_STATE_READY) ds = m35fd_poll_state(drive_ids[1]);

printf("COPYING. DO NOT REMOVE DISCS\n");

local buffer = malloc(M35FD_SECTOR_SIZE);
local counter = 0;

while (counter < M35FD_SECTOR_COUNT)
{
   local status = m35fd_blocking_read(drive_ids[0], counter, buffer);
   if (status != 0)
   {
        if (status == M35FD_BAD_SECTOR) printf("BAD SECTOR ON SOURCE: %N\n", counter);
        else 
        {
            printf("AN ERROR HAS OCCURED.\n");
            while (1) {}
         }
   }

   status = m35fd_blocking_write(drive_ids[1], counter, buffer);
   if (status != 0)
   {
      if (status == M35FD_BAD_SECTOR) printf("BAD SECTOR ON DESTINATION: %N\n", counter);
      else
      {
         printf("AN ERROR HAS OCCURED.\n");
         while (1) {}
      }
   }

   printf(".");
   counter += 1;

}

printf("COPY COMPLETE.");

