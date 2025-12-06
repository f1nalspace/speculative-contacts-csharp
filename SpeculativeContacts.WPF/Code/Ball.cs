namespace SpeculativeContacts.Code
{
	public class Ball : RigidBody
	{
        public double Radius => this.Width * 0.5;

        public override BodyType Type => InvI > 0 ? BodyType.Dynamic : BodyType.Static;
        
		public Ball() : base()
		{
		}

		public override void Initialise(double invMass, Vector2 initialVelocity, double initialAngulatVelocity, double initialFriction = 0.0)
		{
			base.Initialise(invMass, initialVelocity, initialAngulatVelocity, initialFriction);

			// calc inertia tensor
			if (invMass > 0)
			{
				double mass = 1 / invMass;
				double I = mass * Radius * Radius / 4;
				InvI = 1 / I;
			}
			else
                InvI = 0;
		}

		public override List<Contact> GetClosestPoints(RigidBody rb)
		{
			List<Contact> contacts = new List<Contact>();

			if (rb is Ball)
			{
				Ball a = this;
				Ball b = (Ball)rb;

				Vector2 delta = b.Position-a.Position;
				Vector2 n;

				if (delta.LenSquared > 0)
				{
					// get normal
					n = delta.Unit();
				}
				else
				{
					// default
					n = new Vector2(1,0);
				}

				// generate closest points
				Vector2 pa = a.Position + n*a.Radius;
				Vector2 pb = b.Position - n*b.Radius;

				// get distance
				double dist = (b.Position - a.Position).Length - (a.Radius + b.Radius);

				// add contact
				contacts.Add( new Contact(a, b, pa, pb, n, dist) );
			}
			else if (rb is Rectangle)
			{
				Rectangle rectB = (Rectangle)rb;

				contacts = rectB.GetClosestPoints(this);
				Contact.FlipContacts(contacts);
			}
			else if (rb is Plane)
			{
				Plane planeB = (Plane)rb;
				contacts = planeB.GetClosestPoints(this);
				//Contact.FlipContacts(contacts);
			}
			else
			{
				throw new NotImplementedException();
			}

			return contacts;
		}

		public override void GenerateMotionAABB(double dt)
        {
			// get half extents
			Vector2 r = new Vector2(Radius, Radius);

            // get bounds now
            AABB boundsNow = AABB.BuildAABB(Transformation, r);

            // work out transform for next frame
            Matrix23 matrixNextFrame = new Matrix23(Angle + AngularVel * dt, Position + Vel * dt);

            // bounds then
            AABB boundsNextFrame = AABB.BuildAABB(matrixNextFrame, r);

            // bound both
            MotionBounds = new AABB(boundsNow, boundsNextFrame);
        }
    }
}
