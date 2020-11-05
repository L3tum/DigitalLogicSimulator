using System.Collections.Generic;
using System.Linq;

namespace DigitalLogicSimulator
{
    public class Node
    {
        public long DelayInNanoseconds = 0;
        public int ID;
        public List<ulong> Inputs = new List<ulong>();
        public List<ulong> Outputs = new List<ulong>();
        public Dictionary<int, List<Wire>> OutputWiresConnected = new Dictionary<int, List<Wire>>();

        public Node(int id, int numberOfInputs, int numberOfOutputs, long delayInNanoseconds)
        {
            ID = id;
            Inputs.AddRange(Enumerable.Repeat<ulong>(0, numberOfInputs));
            Outputs.AddRange(Enumerable.Repeat<ulong>(0, numberOfOutputs));
            DelayInNanoseconds = delayInNanoseconds;

            for (var i = 0; i < numberOfOutputs; i++)
            {
                OutputWiresConnected.Add(i, new List<Wire>());
            }
        }

        public virtual bool Simulate()
        {
            return false;
        }

        public virtual Sample Sample()
        {
            return new Sample
            {
                NodeId = ID,
                NodeType = GetType(),
                InputValues = Inputs.FindAll(_ => true),
                OutputValues = Outputs.FindAll(_ => true)
            };
        }
    }
}