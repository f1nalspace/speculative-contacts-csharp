using SpeculativeContacts.Engine;
using System.Diagnostics;

namespace SpeculativeContacts.Code
{
	/// <summary>
	/// 
	/// </summary>
	public class Contact
	{
        public RigidBody A { get; private set; }
        public RigidBody B { get; private set; }
        public Vector2 PointA { get; private set; }
        public Vector2 PointB { get; private set; }
        public Vector2 RadiusArmA { get; private set; }
        public Vector2 RadiusArmB { get; private set; }
        public Vector2 Normal { get; private set; }
        public double Impulse { get; set; }
        public double TangentImpulse { get; set; }
        public double Distance { get; }
        public double InvDenom { get; }
        public double InvDenomTan { get; }
		public double Friction { get; }

        /// <summary>
        /// Computes the relative velocity (vB + wB x rB - vA - wA x rA).
        /// </summary>
        public Vector2 ComputeRelativeVelocity() => B.Vel + Vector2.Cross(B.AngularVel, RadiusArmB) - A.Vel - Vector2.Cross(A.AngularVel, RadiusArmA);

        public Contact(RigidBody a, RigidBody b, Vector2 pa, Vector2 pb, Vector2 n, double dist)
		{
			A = a;
			B = b;
			PointA = pa;
			PointB = pb;
			Normal = n;
			Distance = dist;
			Impulse = 0;
            Friction = Scalar.Clamp(Math.Max(a.Friction, b.Friction), 0.0, 1.0);

            Debug.Assert(!double.IsNaN(dist) && !double.IsInfinity(dist));

            Debug.Assert(!double.IsNaN(pa.X) && !double.IsInfinity(pa.X));
            Debug.Assert(!double.IsNaN(pa.Y) && !double.IsInfinity(pa.Y));
            Debug.Assert(!double.IsNaN(pb.X) && !double.IsInfinity(pb.X));
            Debug.Assert(!double.IsNaN(pb.Y) && !double.IsInfinity(pb.Y));
            Debug.Assert(!double.IsNaN(n.X) && !double.IsInfinity(n.X));
            Debug.Assert(!double.IsNaN(n.Y) && !double.IsInfinity(n.Y));

            // calculate radius arms
			Vector2 rA = RadiusArmA = PointA - a.Position;
			Vector2 rB = RadiusArmB = PointB - b.Position;

			// compute denominator in impulse equation
			Debug.Assert(a.InvMass > 0 || b.InvMass > 0);
            double rnA = rA.Dot(n);
            double rnB = rB.Dot(n);
			double nRatio = a.InvMass + b.InvMass;
            nRatio += a.InvI * (Vector2.Dot(rA, rA) - rnA * rnA) + b.InvI * (Vector2.Dot(rB, rB) - rnB * rnB);
			InvDenom = nRatio > 0 ? 1.0 / nRatio : 0.0;

			Vector2 t = n.Perp();
			double rtA = rA.Dot(t);
            double rtB = rB.Dot(t);
            double tRatio = a.InvMass + b.InvMass;
            tRatio += a.InvI * (Vector2.Dot(rA, rA) - rtA * rtA) + b.InvI * (Vector2.Dot(rB, rB) - rtB * rtB);
			InvDenomTan = tRatio > 0 ? 1.0 / tRatio : 0.0;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="imp"></param>
		public void ApplyImpulses(Vector2 imp)
		{
			A.ApplyImpulse(imp, RadiusArmA);
            B.ApplyImpulse(-imp, RadiusArmB);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="contacts"></param>
		static public void FlipContacts(List<Contact> contacts)
		{
			for (int i = 0; i < contacts.Count; i++)
			{
				RigidBody tempRb = contacts[i].A;
				Vector2 tempV = contacts[i].PointA;

				contacts[i].A = contacts[i].B;
				contacts[i].B = tempRb;
				contacts[i].PointA = contacts[i].PointB;
				contacts[i].PointB = tempV;

				tempV = contacts[i].RadiusArmA;
				contacts[i].RadiusArmA = contacts[i].RadiusArmB;
				contacts[i].RadiusArmB = tempV;

				contacts[i].Normal = -contacts[i].Normal;
			}
		}
	}
}
