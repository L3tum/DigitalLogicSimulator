namespace DigitalLogicSimulator.Nodes
{
    public class NOTNode : Node
    {
        public NOTNode(int id) : base(id, 1, 1, 8)
        {
        }

        public override bool Simulate()
        {
            var changedValue = Inputs[0] == 0 ? 1uL : 0uL;

            if (Outputs[0] != changedValue)
            {
                Outputs[0] = changedValue;
                return true;
            }

            return false;
        }
    }
}