using System;

namespace SpeculativeContacts.Engine
{
    public readonly struct Matrix23
    {
        public Vector2 Row0 { get; }
        public Vector2 Row1 { get; }
        public Vector2 Offset { get; }

        public double Angle => Math.Atan2(Row0.Y, Row0.X);

        public Matrix23(double angle, Vector2 pos)
        {
            Row0 = Vector2.FromAngle(angle);
            Row1 = Row0.Perp();
            Offset = pos;
        }

        public Matrix23(Vector2 row0, Vector2 row1, Vector2 pos)
        {
            Row0 = row0;
            Row1 = row1;
            Offset = pos;
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
            return RotateBy(v) + Offset;
        }

        /// <summary>
        /// Defines the identity of <see cref="Matrix23"/>.
        /// </summary>
        public static ref readonly Matrix23 Identity => ref _identity;

        private static readonly Matrix23 _identity = new Matrix23(new Vector2(1, 0), new Vector2(0, 1), Vector2.Zero);
    }
}
