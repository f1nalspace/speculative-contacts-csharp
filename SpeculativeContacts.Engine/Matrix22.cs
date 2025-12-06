namespace SpeculativeContacts.Engine
{
    public readonly struct Matrix22
    {
        public Vector2 Row0 { get; }

        public Vector2 Row1 { get; }

        public Matrix22(Vector2 row0, Vector2 row1)
        {
            Row0 = row0;
            Row1 = row1;
        }

        public Matrix22(double angle)
        {
            Row0 = Vector2.FromAngle(angle);
            Row1 = Row0.Perp();
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
            return RotateBy(v);
        }

        /// <summary>
        /// Defines the identity of <see cref="Matrix22"/>.
        /// </summary>
        public static ref readonly Matrix22 Identity => ref _identity;
        private static readonly Matrix22 _identity = new Matrix22(Vector2.RightAxis, Vector2.UpAxis);
    }
}
