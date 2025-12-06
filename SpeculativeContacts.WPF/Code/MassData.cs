namespace SpeculativeContacts.Code
{
    public readonly struct MassData
    {
        public double InvMass { get; }
        public double InvInertia { get; }

        public MassData(double invMass, double invInertia)
        {
            InvMass = invMass;
            InvInertia = invInertia;
        }
    }
}
