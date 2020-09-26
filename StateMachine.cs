using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TicTacBoom
{
    public class IllegalMoveException : System.Exception
    {
        public IllegalMoveException() { }
        public IllegalMoveException(string message) : base(message) { }
        public IllegalMoveException(string message, System.Exception inner) : base(message, inner) { }
        protected IllegalMoveException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class StateMachine
    {
        public int Width, Height;
        public GameState CurrentState;
        [NonSerialized()]
        private Random rand;
        [NonSerialized()]
        private PlusBomb plusBomb;
        [NonSerialized()]
        private BigBomb bigBomb;

        public StateMachine()
        {
            rand = new Random();
            bigBomb = new BigBomb();
            plusBomb = new PlusBomb();
        }

        public static int FieldIndex(int x, int y, int width)
        {
            return y*width + x;
        }

        public int FieldIndex(int x, int y)
        {
            return y*Width + x;
        }
        
        public void Start(int width, int height)
        {
            Width = width;
            Height = height;
            FieldState[] initialFields = new FieldState[Width*Height];
            for(int i = 0; i < Width*Height; i++)
            {
                initialFields[i] = FieldState.Hidden;
            }

            CurrentState = new GameState{Fields = initialFields, CurrentPlayer = PlayerType.Crosses, BigBombsRemaining = 1, PlusBombsRemaining = 2};
        }

        public IEnumerable<Position> LegalMoves(GameState state)
        {
            List<Position> legalMoves = new List<Position>();
            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    Position move = new Position{x = x, y = y};
                    if(MoveLegal(state, move))
                    {
                        legalMoves.Add(move);
                    }
                }
            }
            return legalMoves;
        }

        public IEnumerable<Position> LegalMoves()
        {
           return LegalMoves(CurrentState);
        }

        public bool MoveLegal(GameState state, Position position)
        {
            // x bounds check
            bool legal = position.x < Width && position.x >= 0;
            // y bounds check
            legal &= position.y < Height && position.y >= 0;
            // no piece on position check
            var field = state.Fields[FieldIndex(position.x, position.y)];
            legal &= field == FieldState.None || field == FieldState.Hidden;
            return legal;
        }

        public GameState NextState(GameState state, Position position)
        {
            if(MoveLegal(state, position))
            {
                GameState newState = state.Copy(Width, Height);
                newState.Fields[FieldIndex(position.x, position.y)] = (FieldState)newState.CurrentPlayer;
                if(state.Fields[FieldIndex(position.x, position.y)] == FieldState.Hidden)
                {
                    int hiddenCount = state.CountHiddenFields();
                    int randValue = rand.Next(0, hiddenCount);
                    // TODO: Improve on this method
                    IExplodable currentBomb = null; 
                    if(randValue < state.BigBombsRemaining)
                    {
                        currentBomb = bigBomb;
                        newState.BigBombsRemaining--;
                    }
                    else if(randValue < state.BigBombsRemaining + state.PlusBombsRemaining)
                    {
                        currentBomb = plusBomb;
                        newState.PlusBombsRemaining--;
                    }
                    if(currentBomb != null)
                    {
                        foreach(var exploded in currentBomb.Explode(position, Width, Height))
                        {
                            newState.Fields[FieldIndex(exploded.x, exploded.y)] = newState.Fields[FieldIndex(exploded.x, exploded.y)] != FieldState.Hidden ? FieldState.None : FieldState.Hidden;
                        }
                    }
                }
                newState.CurrentPlayer = newState.CurrentPlayer == PlayerType.Noughts ? PlayerType.Crosses : PlayerType.Noughts;
                return newState;
            }
            throw new IllegalMoveException();
        }

        public GameState NextState(Position position)
        {
            return NextState(CurrentState, position);
        }

        public void MakeMove(Position position)
        {
            CurrentState = NextState(position);
        }

        public EndStatus CheckEndGame(GameState state)
        {
             // true = horizontal, false = vertical
            FieldState LineCheck(bool direction)
            {
                int outerLimit = direction ? Height : Width; 
                int innerLimit = direction ? Width : Height;
                for(int outer = 0; outer < Height; outer++)
                {
                    FieldState initialPlayer = direction ? state.Fields[FieldIndex(0, outer)] : state.Fields[FieldIndex(outer, 0)];
                    for(int inner = 0; inner < Width; inner++)
                    {
                        FieldState next = direction ? state.Fields[FieldIndex(inner, outer)] : state.Fields[FieldIndex(outer, inner)];
                        // If the current cell is None or not the same as row/column check,  
                        if(next == FieldState.None || initialPlayer != next)
                        {
                            initialPlayer = FieldState.None;
                            break;
                        }
                    }
                    if(initialPlayer != FieldState.None)
                    {
                        return initialPlayer;
                    }
                }
                return FieldState.None;
            }
            // true = top left to bottom right, false = top right to bottom left
            FieldState DiagonalCheck(bool direction)
            {
                int x = direction ? 0 : Width - 1;
                int y = 0;
                FieldState initialPlayer = state.Fields[FieldIndex(x, y)];
                while(x < Width && y < Height)
                {
                    FieldState next = state.Fields[FieldIndex(x, y)];
                    // If the current cell is None or not the same as row/column check,  
                    if(next == FieldState.None || initialPlayer != next)
                    {
                        initialPlayer = FieldState.None;
                        break;
                    }
                    x = direction ? x + 1 : x - 1;
                    y++;
                }
                return initialPlayer;
            }
            FieldState winner = FieldState.None;

            // Horizontal check
            winner = LineCheck(true);
            if(winner != FieldState.None)
            {
                return (EndStatus)winner;
            }

            // Vertical check
            winner = LineCheck(false);
            if(winner != FieldState.None)
            {
                return (EndStatus)winner;
            }

            // Diagonal check
            winner = DiagonalCheck(true);
            if(winner != FieldState.None)
            {
                return (EndStatus)winner;
            }

            winner = DiagonalCheck(false);
            if(winner != FieldState.None)
            {
                return (EndStatus)winner;
            }

            // Tie check
            bool allFieldsPlaced = true;
            foreach(var field in state.Fields)
            {
                if(field == FieldState.None || field == FieldState.Hidden)
                {
                    allFieldsPlaced = false;
                    break;
                }
            }
            if(allFieldsPlaced)
            {
                return EndStatus.Tie;
            }

            return EndStatus.Ongoing;
        }

        public EndStatus CheckEndGame()
        {
           return CheckEndGame(CurrentState);
        }
    }
}