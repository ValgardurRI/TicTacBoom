using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TicTacBoom
{
    class Program
    {
        static int size = 3;
        static bool EndCheck(StateMachine game)
        {
            var status = game.CheckEndGame(); 
            if(status != EndStatus.Ongoing)
            {
                if(status == EndStatus.Tie)
                {
                    Console.WriteLine("Tie!");
                }
                else
                {
                    Console.WriteLine(status.ToString() + " wins!");
                }
                return true;
            }
            return false;
        }
        static void StartPvP(StateMachine game)
        {
            while(true)
            {
                string input = Console.ReadLine();
                if(input == "quit")
                {
                    break;
                }
                var split = input.Split(' ');
                int x, y;
                if(split.Length > 1 && Int32.TryParse(split[0], out x) && Int32.TryParse(split[1], out y))
                {
                    var move = new Position{x = x, y = y};
                    game.MakeMove(move);
                    Console.WriteLine(game.CurrentState.Visualize(game.Width, game.Height));
                }
                var status = game.CheckEndGame(); 
                if(EndCheck(game))
                {
                    break;
                }
            }
        }

        static void TrainAgent(StateMachine game, int iterations = 100000, string agentName = "agent")
        {
            var agent = File.Exists(agentName + ".json") ? SarsaAgent.DeserializeJson(agentName + ".json", game) : new SarsaAgent(game);
            int crossWins = 0;
            int ties = 0;
            int noughtWins = 0;

            for(int i = 0; i < iterations; i++)
            {
                if(i == 0  || i%(iterations/10) == 0)
                    Console.WriteLine("Iteration " + i + " of " + iterations + " complete.");
                var states = new List<Tuple<GameState, Position?>>();
                while(game.CheckEndGame() == EndStatus.Ongoing)
                {
                    Position move = (Position)agent.Policy(game.CurrentState);
                    states.Add(new Tuple<GameState, Position?>(game.CurrentState, move));
                    game.MakeMove(move);
                }

                var endState = game.CheckEndGame(); 
                if(endState == EndStatus.Crosses)
                    crossWins++;
                else if(endState == EndStatus.Noughts)
                    noughtWins++;
                else if(endState == EndStatus.Tie)
                    ties++;

                states.Add(new Tuple<GameState, Position?>(game.CurrentState, null));
                GameState? statePrime = null; 
                Position? movePrime = null;
                for(int s = states.Count - 1; s >= 0; s--)
                {
                    var (state, move) = states[s];
                    agent.SarsaUpdate(state, move, statePrime, movePrime);
                    statePrime = state;
                    movePrime = move;
                }
                
                game.Start(size,size);
            }
            Console.WriteLine(agent.EmptyPositions());
            Console.WriteLine("Discovered states: " + agent.DiscoveredStates());
            Console.WriteLine("Wins (nought/tie/cross): " + noughtWins + "/" + ties + "/" + crossWins);
            if(agentName == null)
            {
                Console.WriteLine("Type name of agent: ");
                agentName = Console.ReadLine();
            }
            agent.SerializeJson(agentName + ".json");
        }

        static void PlayerVsAgent(StateMachine game, string agentName = "agent", bool playerStart = true)
        {
            var agent = SarsaAgent.DeserializeJson(agentName + ".json", game);
            bool AgentMove()
            {
                var move = agent.GreedyPolicy(game.CurrentState);
                game.MakeMove((Position)move);
                Console.WriteLine(game.CurrentState.Visualize(game.Width, game.Height));
                return EndCheck(game);
            }
            
            bool PlayerMove()
            {
                string input = Console.ReadLine();
                var split = input.Split(' ');
                if(input == "quit")
                {
                    System.Environment.Exit(0);
                }
                int x, y;
                if(split.Length > 1 && Int32.TryParse(split[0], out x) && Int32.TryParse(split[1], out y))
                {
                    var playerMove = new Position{x = x, y = y};
                    game.MakeMove(playerMove);
                    Console.WriteLine(game.CurrentState.Visualize(game.Width, game.Height));
                }
                return EndCheck(game);
            }
            while(true)
            {
                game.Start(size,size);
                Console.WriteLine(game.CurrentState.Visualize(game.Width, game.Height));
                Func<bool> firstMove = PlayerMove;
                Func<bool> secondMove = AgentMove;
                if(!playerStart)
                {
                    firstMove = AgentMove;
                    secondMove = PlayerMove;
                }
                while(true)
                {
                    if(firstMove())
                    {
                        break;
                    }
                    else if(secondMove())
                    {
                        break;
                    }
                }
            }
        }
        static void GetWinRatio(StateMachine game, int iterations = 1000, string agentName = "agent")
        {
            var agent = SarsaAgent.DeserializeJson(agentName + ".json", game);
            int crossWins = 0;
            int noughtWins = 0;
            int ties = 0;
            Random rand = new Random();
            bool AgentMove()
            {
                var move = agent.GreedyPolicy(game.CurrentState);
                game.MakeMove((Position)move);

                var status = game.CheckEndGame(); 
                if(status != EndStatus.Ongoing)
                {
                    if(status == EndStatus.Tie)
                    {
                        ties++;
                    }
                    else if(status == EndStatus.Crosses)
                    {
                        crossWins++;
                    }
                    else if(status == EndStatus.Noughts)
                    {
                        noughtWins++;
                    }
                    return true;
                }
                return false;
            }

            bool RandomMove()
            {
                var legalMoves = game.LegalMoves();
                var move = legalMoves.ElementAt(rand.Next(legalMoves.Count()));
                game.MakeMove(move);
                var status = game.CheckEndGame(); 
                if(status != EndStatus.Ongoing)
                {
                    if(status == EndStatus.Tie)
                    {
                        ties++;
                    }
                    else if(status == EndStatus.Crosses)
                    {
                        crossWins++;
                    }
                    else if(status == EndStatus.Noughts)
                    {
                        noughtWins++;
                    }
                    return true;
                }
                return false;
            }
            for(int i = 0; i < iterations; i++)
            {
                if(i == 0  || i%(iterations/10) == 0)
                    Console.WriteLine("Iteration " + i + " of " + iterations + " complete.");

                game.Start(size,size);
                while(true)
                {
                    if(AgentMove())
                    {
                        break;
                    }
                    else if(AgentMove())
                    {
                        break;
                    }
                }
            }
            Console.WriteLine("Wins (nought/tie/cross): " + noughtWins + "/" + ties + "/" + crossWins);
        }
        static void Main(string[] args)
        {
            if(args.Length >= 1)
            {
                var game = new TicTacBoom.StateMachine();
                game.Start(size,size);
                if(args[0].ToLower().Equals("train"))
                {
                    if(args.Length >= 3)
                        TrainAgent(game, Int32.Parse(args[1]), args[2]);
                    else if(args.Length >= 2)
                        TrainAgent(game, Int32.Parse(args[1]));
                    else
                        TrainAgent(game);
                }
                else if(args[0].ToLower().Equals("vsagent"))
                {
                    if(args.Length >= 2)
                        PlayerVsAgent(game, args[1], false);
                    else
                        PlayerVsAgent(game);
                }
                else if(args[0].ToLower().Equals("winratio"))
                {
                    if(args.Length >= 3)
                        GetWinRatio(game, Int32.Parse(args[1]), args[2]);
                    else if(args.Length >= 2)
                        GetWinRatio(game, agentName: args[1]);
                    else
                        GetWinRatio(game);
                }
                else if(args[0].ToLower().Equals("pvp"))
                {
                    StartPvP(game);
                }
                else
                {
                    Console.WriteLine("Command does not match any available command. Exiting.");
                }
            }
            else
            {
                Console.WriteLine("No argument passed to program, exiting.");
            }
        }
    }
}
