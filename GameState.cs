using System;
using System.Linq;

namespace TicTacBoom
{
    [Serializable()]
    public struct GameState
    {
        public PlayerType CurrentPlayer;
        public FieldState[] Fields;
        public int BigBombsRemaining;
        public int PlusBombsRemaining;
        public GameState Copy(int width, int height)
        {
            var duplicate = (GameState)this.MemberwiseClone();
            // TODO: is this necessary?
            duplicate.Fields = Fields.Clone() as FieldState[];
            return duplicate;
        }

        public int CountHiddenFields()
        {
            return Fields.Sum(f => f == FieldState.Hidden ? 1 : 0);
        }

        public string Visualize(int width, int height)
        {
            string visualization = "";
            for(int y = 0; y < height + width; y++)
            {
                if(y == height + width - 1)
                {
                    break;
                }
                for(int x = 0; x < width + height; x++)
                {
                    char nextChar = '!';
                    if(x == width + height - 1)
                    {
                        nextChar = '\n';
                    }
                    else if(y%2 == 1)
                    {
                        nextChar = '-';
                    }
                    else if(x%2 == 1)
                    {
                        nextChar = '|';
                    }
                    else
                    {
                        FieldState currField = Fields[StateMachine.FieldIndex(x/2, y/2, width)];
                        if(currField == FieldState.Noughts)
                        {
                            nextChar = 'O';
                        }
                        else if (currField == FieldState.Crosses)
                        {
                            nextChar = 'X';
                        }
                        else if (currField == FieldState.Hidden)
                        {
                            nextChar = '?';
                        }
                        else
                        {
                            nextChar = ' ';
                        }
                    }
                    visualization += nextChar;
                }
            }
            return visualization;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            GameState s = (GameState)obj;
            return CurrentPlayer.Equals(s.CurrentPlayer) && BigBombsRemaining.Equals(s.BigBombsRemaining) && PlusBombsRemaining.Equals(s.PlusBombsRemaining) &&  Enumerable.SequenceEqual(Fields, s.Fields);
        }

        public override int GetHashCode()
        {
            int fieldHash = 0;
            for (var i = 0;i< this.Fields.Length; i++)
            {
                fieldHash = HashCode.Combine(this.Fields[i], fieldHash);
            }
            return HashCode.Combine(CurrentPlayer, fieldHash, BigBombsRemaining, PlusBombsRemaining);
        }


        // Use ToString to serialize state in a FEN inspired manner
        public override string ToString()
        {
            string outString = "";
            // Serialize fields
            foreach(var field in Fields)
            {
                if(field == FieldState.Hidden)
                    outString += '?';
                else if(field == FieldState.Crosses)
                    outString += 'X';
                else if(field == FieldState.Noughts)
                    outString += 'O';
                else if(field == FieldState.None)
                    outString += '-';
            }

            outString += ' ';
            
            // Serialize current player
            if(CurrentPlayer == PlayerType.Noughts)
                outString += 'O';
            else
                outString += 'X';
            
            outString += ' ';

            // Serialize bomb counts
            outString += BigBombsRemaining;
            outString += ' ';
            outString += PlusBombsRemaining;
            return outString;
        }

        public static GameState FromString(string stateString)
        {
            var splitString = stateString.Split(' ');

            // Parse fields
            FieldState[] fields = new FieldState[splitString[0].Length];
            for(int i = 0; i < fields.Length; i++)
            {
                char fieldChar = splitString[0][i]; 
                if(fieldChar == '?')
                    fields[i] = FieldState.Hidden;
                else if(fieldChar == 'X')
                    fields[i] = FieldState.Crosses;
                else if(fieldChar == 'O')
                    fields[i] = FieldState.Noughts;
                else if(fieldChar == '-')
                    fields[i] = FieldState.None;
                else
                    throw new ArgumentException("Field part in string contains illegal characters. Legal characters are ?,X,O,-");
            }

            // Parse current player
            PlayerType currentPlayer = PlayerType.None;
            if(splitString[1] == "X")
            {
                currentPlayer = PlayerType.Crosses;
            }
            else if(splitString[1] == "O")
            {
                currentPlayer = PlayerType.Noughts;
            }
            else
            {
                throw new ArgumentException("Current player part in string contains illegal characters X,O");
            }

            // Parse bomb counts
            int bigBombs = Int32.Parse(splitString[2]);
            int plusBombs = Int32.Parse(splitString[3]);

            return new GameState{Fields = fields, CurrentPlayer = currentPlayer, BigBombsRemaining = bigBombs, PlusBombsRemaining = plusBombs};
        }
    }
}