namespace DigitalLogicSimulator.Nodes
{
    public class NXORNode : Node
    {
        public NXORNode(int id) : base(id, 2, 1, 10)
        {
        }

        public override bool Simulate()
        {
            var oldOutput = Outputs[0];
            Outputs[0] = (Inputs[0] ^ Inputs[1]) == 1 ? 0uL : 1uL;

            return oldOutput != Outputs[0];
        }
    }
}