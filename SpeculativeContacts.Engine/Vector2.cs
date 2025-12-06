using System;
using System.ComponentModel;

namespace SpeculativeContacts.Engine
{
    [TypeConverter(typeof(Vector2TypeConverter))]
    public struct Vector2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2(Vector2 p)
        {
            X = p.X;
            Y = p.Y;
        }

        public Vector2(double c)
        {
            X = c;
            Y = c;
        }

        public static Vector2 FromAngle(double radians)
        {
            return new Vector2(Math.Cos(radians), Math.Sin(radians));
        }

        public static double Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Returns the left perpendicular vector of <paramref name="s"/> cross with v <paramref name="v"/>.
        /// </summary>
        /// <param name="s">The scalar value.</param>
        /// <param name="v">The <see cref="Vector2"/>.</param>
        /// <returns>The resulting <see cref="Vector2"/>.</returns>
        public static Vector2 Cross(double s, Vector2 v)
        {
            return new Vector2(-s * v.Y, s * v.X);
        }

        /// <summary>
        /// Returns the right perpendicular vector of <paramref name="s"/> cross with v <paramref name="v"/>.
        /// </summary>
        /// <param name="v">The <see cref="Vector2"/>.</param>
        /// <param name="s">The scalar value.</param>
        /// <returns>The resulting <see cref="Vector2"/>.</returns>
        public static Vector2 Cross(Vector2 v, double s)
        {
            return new Vector2(s * v.Y, -s * v.X);
        }

        public static double Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public readonly float ToAngle()
        {
            float angle = (float)Math.Atan2(Y, X);

            // make the returned range 0 -> 2*PI
            if (angle < 0.0f)
            {
                angle += 2 * (float)Math.PI;
            }
            return angle;
        }

        public readonly double LenSquared => Dot(this);

        public readonly double Length => Math.Sqrt(LenSquared);

        public readonly double Dot(Vector2 v)
        {
            return X * v.X + Y * v.Y;
        }

        public readonly Vector2 Perp()
        {
            return new Vector2(-Y, X);
        }

        public readonly Vector2 Floor()
        {
            return new Vector2(Math.Floor(X), Math.Floor(Y));
        }

        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max)
        {
            return Max(min, Min(v, max));
        }

        public readonly Vector2 Unit() => this / Length;

        public readonly Vector2 Abs() => new Vector2(Math.Abs(X), Math.Abs(Y));

        public readonly Vector2 Frac()
        {
            Vector2 abs = Abs();
            return abs - abs.Floor();
        }

        public readonly double Angle() => Math.Atan2(Y, X);

        public readonly Vector2 MajorAxis()
        {
            Vector2 a = Abs();
            if (a.X > a.Y)
                return new Vector2(Math.Sign(a.X), 0.0f);
            else
                return new Vector2(0.0f, Math.Sign(a.Y));
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }
        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }
        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X / b.X, a.Y / b.Y);
        }
        public static Vector2 operator *(Vector2 a, double fB)
        {
            return new Vector2(a.X * fB, a.Y * fB);
        }
        public static Vector2 operator *(double fA, Vector2 b)
        {
            return new Vector2(b.X * fA, b.Y * fA);
        }
        public static Vector2 operator /(Vector2 a, double fB)
        {
            return new Vector2(a.X / fB, a.Y / fB);
        }
        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.X, -v.Y);
        }

        public override string ToString()
        {
            return "x=" + X + ",y=" + Y;
        }

        public readonly Vector2 RotateIntoSpaceOf(Matrix22 m)
        {
            Vector2 row0 = m.Row0;
            Vector2 row1 = m.Row1;
            return new Vector2(Dot(row0), Dot(row1));
        }

        public readonly Vector2 RotateIntoSpaceOf(Matrix23 m)
        {
            Vector2 row0 = m.Row0;
            Vector2 row1 = m.Row1;
            return new Vector2(Dot(row0), Dot(row1));
        }

        public readonly Vector2 RotateBy(Matrix22 m)
        {
            Vector2 row0 = m.Row0;
            Vector2 row1 = m.Row1;
            return row0 * X + row1 * Y;
        }

        public readonly Vector2 RotateBy(Matrix23 m)
        {
            Vector2 row0 = m.Row0;
            Vector2 row1 = m.Row1;
            return row0 * X + row1 * Y;
        }

        /// <summary>
        /// Defines a zero <see cref="Vector2"/.
        /// </summary>
        public static ref readonly Vector2 Zero => ref _zero;
        private static readonly Vector2 _zero = new Vector2(0.0f, 0.0f);

        /// <summary>
        /// Defines a right axis <see cref="Vector2"/>.
        /// </summary>
        public static ref readonly Vector2 RightAxis => ref _right;
        private static readonly Vector2 _right = new Vector2(1.0f, 0.0f);

        /// <summary>
        /// Defines a up axis <see cref="Vector2"/>.
        /// </summary>
        public static ref readonly Vector2 UpAxis => ref _up;
        private static readonly Vector2 _up = new Vector2(0.0f, 1.0f);
    }
}
