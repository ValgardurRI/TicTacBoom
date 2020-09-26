# Tic-Tac-Boom
An overengineered variation of the ever-classic TicTacToe for the terminal. Also features a tablular sarsa player.

The game plays the same as tic-tac-toe, with the same win conditions. However, the game starts with 3 bombs that may be hidden under any of the fields. If a player places a piece on a bomb the bomb explodes, destroying some of the pieces on the board.
I thought it would be interesting to see how the game strategy changes with this rule change.

This could probably use much more explanation, especially with how the command line interface works.

To run the program, navigate into the base directory with your favourite terminal and type the command:
    
    dotnet run
This of course requires the dotnet runtime environment (project made with .net 3.1.402 but should run with any version, just change version in TicTacBoom.csproj).
