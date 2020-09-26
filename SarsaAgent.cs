using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace TicTacBoom
{
    using QTuple = Tuple<GameState, Position?>;
    [Serializable]
    class SarsaAgent
    {
        public SarsaAgent()
        {
            rand = new Random();
        }

        public SarsaAgent(StateMachine game)
        {
            this.machine = game;
            StepSize = 0.001f;
            Discount = 0.9f;
            Epsilon = 0.2f;
            rand = new Random();
            QTable = new Dictionary<QTuple, float>();
        }

        public float StepSize { get; set; }
        public float Discount { get; set; }
        public float Epsilon { get; set; }
        public Dictionary<QTuple, float> QTable { get; set; }
        [NonSerialized]
        StateMachine machine;
        [NonSerialized]
        Random rand;

        public void SarsaUpdate(GameState state, Position? action, GameState? statePrime, Position? actionPrime)
        {
            IndexTable(state, action);
            var tuple = new QTuple(state, action);
            if(statePrime != null)
            {
                IndexTable((GameState)statePrime, actionPrime);
                var tuplePrime = new QTuple((GameState)statePrime, actionPrime);
                float value = QTable[tuple] + StepSize*(StateReward(state) - Discount*QTable[tuplePrime] - QTable[tuple]);
                QTable[tuple] = value;
            }
            else
            {
                QTable[tuple] = StateReward(state);
            }
        }

        public Position? Policy(GameState state)
        {
            if(rand.NextDouble() > Epsilon)
            {
                return GreedyPolicy(state);
            }
            else
            {
                var legalMoves = machine.LegalMoves(state);
                if(legalMoves.Count() == 0)
                {
                    return null;
                }
                else
                {
                    return legalMoves.ElementAt(rand.Next(legalMoves.Count()));
                }
            }
        }

        public Position? GreedyPolicy(GameState state)
        {
            var legalMoves = machine.LegalMoves(state);
            if(legalMoves.Count() != 0)
            {
                var orderedMoves = legalMoves.OrderByDescending(move => IndexTable(state, move));
                return orderedMoves.First();
            }
            return null;
        }

        private float IndexTable(GameState state, Position? move)
        {
            var tuple = new QTuple(state, move);
            if(!QTable.ContainsKey(tuple))
            {
                QTable.Add(tuple, 0);
            }
            return QTable[tuple];
        }

        private float StateReward(GameState state)
        {
            var status = machine.CheckEndGame(state);

            if(status == EndStatus.Crosses || status == EndStatus.Noughts)
            {
                if((PlayerType)status == state.CurrentPlayer)
                {
                    return 100;
                }
                else
                {
                    return -100;
                }
            }
            else if(status == EndStatus.Tie)
            {
                return -20;
            }
            else
            {
                return 0;
            }
        }

        public string EmptyPositions()
        {
            string output = "";
            foreach(var ((state, action), value) in QTable)
            {
                int hiddenCount = state.CountHiddenFields();
                if(hiddenCount == state.Fields.Count())
                {
                    output += state.Visualize(machine.Width, machine.Height);
                    output += "bombs remaining: " + (state.PlusBombsRemaining + state.BigBombsRemaining) + '\n';
                    if(action != null)
                        output += "action: " + action.ToString() + "\n";
                    else
                        output += "action: null\n";
                    output += "value: " + value + "\n\n";
                }
            }
            return output;
        }

        public int DiscoveredStates()
        {
            if(QTable != null)
            {
                return QTable.Count();
            }
            return 0;
        }
        public void SerializeJson(string filename)
        {
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new QTableTValueConverter());
            serializeOptions.WriteIndented = true;
            serializeOptions.IgnoreReadOnlyProperties = false;
            string jsonString = JsonSerializer.Serialize<SarsaAgent>(this, serializeOptions);
            File.WriteAllText(filename, jsonString);
        }

        public static SarsaAgent DeserializeJson(string filename, StateMachine stateMachine)
        {
            string jsonString = File.ReadAllText(filename);
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new QTableTValueConverter());
            serializeOptions.WriteIndented = true;
            serializeOptions.IgnoreReadOnlyProperties = false;
            SarsaAgent agent = JsonSerializer.Deserialize<SarsaAgent>(jsonString, serializeOptions);
            agent.rand = new Random();
            agent.machine = stateMachine;
            return agent;
        }

        public void Serialize(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                // create BinaryFormatter
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, this);
            }
        }

        public static SarsaAgent Deserialize(string filename, StateMachine stateMachine)
        {
            var ret = new SarsaAgent();
            using (var stream = File.Open(filename, FileMode.Open))
            {
                // create BinaryFormatter
                BinaryFormatter bin = new BinaryFormatter();
                ret = (SarsaAgent)bin.Deserialize(stream);
                ret.rand = new Random();
                ret.machine = stateMachine;
            }
            return ret;
        }
    }
}