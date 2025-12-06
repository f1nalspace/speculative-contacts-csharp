namespace SpeculativeContacts.Engine
{
    public readonly struct Transform
    {
        public Matrix22 Rotation { get; }
        public Vector2 Position { get; }

        public Transform(Vector2 position, Matrix22 rotation) : this()
        {
            Position = position;
            Rotation = rotation;
        }

        public Matrix23 ToMatrix23() => new Matrix23(Rotation.Row0, Rotation.Row1, Position);
    }
}
