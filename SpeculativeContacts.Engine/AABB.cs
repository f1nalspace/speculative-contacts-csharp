namespace SpeculativeContacts.Engine
{
	public readonly struct AABB
	{
		public Vector2 Center { get; }
		public Vector2 HalfExtents { get; }

        public AABB(Vector2 centre, Vector2 halfExtents)
		{
			Center = centre;
			HalfExtents = halfExtents;
		}

		public AABB(AABB a, AABB b)
		{
			Vector2 minA = a.Center - a.HalfExtents;
			Vector2 maxA = a.Center + a.HalfExtents;

			Vector2 minB = b.Center - b.HalfExtents;
			Vector2 maxB = b.Center + b.HalfExtents;

			Vector2 min = Vector2.Min(minA, minB);
			Vector2 max = Vector2.Max(maxA, maxB);

			Center = (min + max) / 2;
			HalfExtents = (max - min) / 2;
		}

		public static bool Overlap(AABB a, AABB b)
		{
			Vector2 d = (b.Center - a.Center).Abs() - (a.HalfExtents+b.HalfExtents);
			return d.X < 0 && d.Y < 0;
		}

		public static AABB BuildAABB(Matrix23 m, Vector2[] points)
		{
			Vector2 min = new Vector2(double.MaxValue, double.MaxValue);
			Vector2 max = -min;

			foreach (Vector2 p in points)
			{
				Vector2 v = m.TransformBy(p);
                min = Vector2.Min(v, min);
				max = Vector2.Max(v, max);
			}

			return new AABB((min + max) / 2, (max - min) / 2);
		}

        public static AABB BuildAABB(Matrix23 m, Vector2 halfExtents)
        {
			double halfWidth = halfExtents.X;
            double halfHeight = halfExtents.Y;

            Vector2 p = m.Pos;
			Vector2 n = m.Row0;
            Vector2 t = m.Row1;

            Vector2 topLeft = p - t * halfWidth - n * halfHeight;
            Vector2 topRight = p + t * halfWidth - n * halfHeight;
            Vector2 bottomRight = p + t * halfWidth + n * halfHeight;
            Vector2 bottomLeft = p - t * halfWidth + n * halfHeight;

            Vector2 min = Vector2.Min(Vector2.Min(topLeft, topRight), Vector2.Min(bottomRight, bottomLeft));
            Vector2 max = Vector2.Max(Vector2.Max(topLeft, topRight), Vector2.Max(bottomRight, bottomLeft));
            Vector2 ext = (max - min) * 0.5;
			Vector2 center = min + ext;

            return new AABB(center, ext);
        }

        public AABB OffsetBy(Vector2 offset) => new AABB(Center + offset, HalfExtents);
    }
}
