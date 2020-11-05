namespace DigitalLogicSimulator
{
    public class Wire : Node
    {
        public int InputAtNodeConnectedTo;
        public Node NodeToOutputTo;

        public Wire(int id) : base(id, 1, 1, 0)
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