namespace SpeculativeContacts.Code
{
    public readonly struct Matrix23
    {
        public Vector2 Row0 { get; }

        public Vector2 Row1 { get; }

        public Vector2 Pos { get; }

        public Matrix23()
        {
            Row0 = new Vector2(1, 0);
            Row1 = new Vector2(0, 1);
            Pos = new Vector2();
        }

        public Matrix23(double angle, Vector2 pos)
        {
            Row0 = Vector2.FromAngle(angle);
            Row1 = Row0.Perp();
            Pos = pos;
        }

        public Vector2 RotateIntoSpaceOf(Vector2 v)
        {
            return new Vector2(v.Dot(Row0), v.Dot(Row1));
        }

        public Vector2 RotateBy(Vector2 v)
        {
            return v.X * Row0 + v.Y * Row1;
        }

        public Vector2 TransformBy(Vector2 v)
        {
            return RotateBy(v) + Pos;
        }
    }
}
