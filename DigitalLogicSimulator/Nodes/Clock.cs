namespace DigitalLogicSimulator.Nodes
{
    public class Clock : Node
    {
        public Clock(int id, long stepInMilliseconds) : base(id, 0, 1, stepInMilliseconds * 1000 * 1000)
        {
        }

        public override bool Simulate()
        {
            if (Outputs[0] == 0)
            {
                Outputs[0] = 1;
                return true;
            }

            if (Outputs[0] == 1)
            {
                Outputs[0] = 0;
                return true;
            }

            return false;
        }
    }
}