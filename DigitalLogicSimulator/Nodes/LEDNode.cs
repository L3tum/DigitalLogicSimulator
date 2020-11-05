namespace DigitalLogicSimulator.Nodes
{
    public class LEDNode : Node
    {
        public LEDNode(int id) : base(id, 1, 1, 10)
        {
        }

        public override bool Simulate()
        {
            if (Inputs[0] != Outputs[0])
            {
                Outputs[0] = Inputs[0];
                return true;
            }

            return false;
        }
    }
}