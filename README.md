# BingoBoardReplay
This mod was made to help tournament organizers restream bingosync boards when the players have a stream delay.  
It connects to two bingosync rooms as specified by the user, and when it receives a goal-mark on the "player" board, it waits the specified time and marks the same slot with the same color on the "replay" board.  

# How to use
1. Install and enable the mod in Lumafly, and start the game
2. Go to BingoBoardReplay's mod-menu `Options > Mods > BingoBoardReplay`
3. Input the players' stream delay as the `Main Delay` in seconds
4. Input the links and passwords of the two rooms
5. Click the `Start Replay` button
6. If necessary, click the `Clone Board` button to reproduce the exact state of the player board in the replay room
7. While the game is going, pay attention to whether the replay timing matches the streams, and adjust the `Secondary Delay` accordingly

# Notes
1. As the bot joins the rooms, it will display in-game whether the rooms are in lockout mode or not, you can use this to make sure the player room has the correct setting
2. There is a counter displaying how many goal marks have happened in the player room, that haven't been replayed yet in the replay room
3. The `Clone Board` button will always set the replay room to non-lockout
4. Leaving the mod-menu does not interrupt the replay (as long as you keep the game open)


