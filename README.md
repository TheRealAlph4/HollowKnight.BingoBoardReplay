# BingoBoardReplay
This mod was made to help tournament organizers restream bingosync boards when the players have a stream delay.  
It connects to two bingosync rooms as specified by the user, and when it receives a goal-mark on the "source" board, it waits the specified time and marks the same slot with the same color on the "destination" board.  

# How to use
Install and enable the mod.
> Note: If this is your first time using the mod, start the game and close it again immediately. This creates the global settings file.  

Find the global settings file at `AppData\LocalLow\Team Cherry\Hollow Knight\BingoBoardReplay.GlobalSettings.json`.  
Paste the room info into the correct places. The room link can be the full link, or just the room code from the end of the link.  
"Source" is where the game is being played, "destination" is where the replay will happen.  
The delay should be given as a (non-negative) whole number in seconds.  
After it's set up, save the global settings file, and open Hollow Knight.  
You should see "ReplayBot" join both rooms, reveal both cards, and write a message in the destination room confirming source room and delay.  
Leave the game running while the match is being played.  

Notes:
- There is currently no way to change any of the information from within the game. Close the game, edit the settings file and restart the game if you need to change anything.
- There are no checks making sure that the boards are the same.
- The same *slot* gets marked, regardless of the goals on either board.
- Generating new boards while the bot is running should be fine. The bot should automatically reveal the new card. If it does not, restart the game.
- There should **not** be several people replaying to the same destination board. This mod replays all marks, regardless of color.
- It doesn't matter who has this mod installed, it can just be a TO and none of the players. 

