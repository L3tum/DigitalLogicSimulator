namespace DigitalLogicSimulator.Nodes
{
    public class XORNode : Node
    {
        public XORNode(int id) : base(id, 2, 1, 8)
        {
        }

        public override bool Simulate()
        {
            var oldOutput = Outputs[0];
            Outputs[0] = Inputs[0] ^ Inputs[1];

            return oldOutput != Outputs[0];
        }
    }
}