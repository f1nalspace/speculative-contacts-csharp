using SpeculativeContacts.Engine;
using System.Windows.Media;

namespace SpeculativeContacts.Code
{
    public class Plane : RigidBody
    {
        public Vector2 Point
        {
            get => _point;
            private set => _point = value;
        }
        private Vector2 _point;

        public Vector2 Normal
        {
            get => _normal;
            private set => _normal = value;
        }
        private Vector2 _normal;

        public override BodyType Type => BodyType.Static;

        public Plane() : base()
        {
        }

        public override void Initialise(double invMass, Vector2 initialVelocity, double initialAngulatVelocity, double initialFriction = 0.0)
        {
            base.Initialise(invMass, initialVelocity, initialAngulatVelocity, initialFriction);

            // calc inertia tensor
            InvI = 0;

            TransformGroup tg = (TransformGroup)this.RenderTransform;

            Normal = new Vector2(tg.Value.M21, tg.Value.M22);

            // point on plane
            Point = Position + Normal * -Height * 0.5;
        }

        public double DistanceToPoint(Vector2 p)
        {
            Vector2 d = Point - p;
            double dist = d.Dot(Normal);
            return dist;
        }

        public Vector2 ProjectPointOntoPlane(Vector2 p)
        {
            Vector2 d = p - Point;
            double dist = d.Dot(Normal);
            return p - Normal * dist;
        }

        public override List<Contact> GetClosestPoints(RigidBody b)
        {
            List<Contact> contacts = new List<Contact>();

            if (b is Ball)
            {
                Ball rb = (Ball)b;
                double dist = DistanceToPoint(rb.Position) - rb.Radius;

                Vector2 pointOnPlane = ProjectPointOntoPlane(rb.Position);
                Vector2 pointOnBall = rb.Position + Normal * dist;

                contacts.Add(new Contact(this, rb, pointOnPlane, pointOnBall, -Normal, dist));
            }
            else if (b is Rectangle)
            {
                Rectangle rb = (Rectangle)b;

                contacts = rb.GetClosestPoints(this);
                Contact.FlipContacts(contacts);
            }
            else
            {
                throw new NotImplementedException();
            }

            return contacts;
        }

        public override void GenerateMotionAABB(double dt)
        {
            // A plane cannot move, so we don't have to account for the next frame
            Vector2 n = Normal;
            Vector2 t = n.Perp();
            
            double halfWidth = Width * 0.5;
            double halfHeight = Height * 0.5;

            double normalScale = 1.0;
            double tangentScale = 1.0;

            Vector2 p = Point;

            // Compute the four corners of the rotated plane surface
            Vector2 topLeft = p - t * halfWidth;
            Vector2 topRight = p + t * halfWidth;
            Vector2 bottomRight = topRight + n * Height;
            Vector2 bottomLeft = topLeft + n * Height;

            /*
            // Extend each corner in the opposite normal and tangent directions
            Vector2 normalExt = -n * halfHeight * normalScale;
            Vector2 tangentExt = t * halfWidth * tangentScale;

            // Create extended quad
            Vector2 e1 = topLeft - tangentExt + normalExt;
            Vector2 e2 = topRight + tangentExt + normalExt;
            Vector2 e3 = bottomRight + tangentExt;
            Vector2 e4 = bottomLeft - tangentExt;
            */

            Vector2 e1 = topLeft;
            Vector2 e2 = topRight;
            Vector2 e3 = bottomRight;
            Vector2 e4 = bottomLeft;

            // Compute AABB from all points
            Vector2 min = Vector2.Min(Vector2.Min(e1, e2), Vector2.Min(e3, e4));
            Vector2 max = Vector2.Max(Vector2.Max(e1, e2), Vector2.Max(e3, e4));
            Vector2 ext = (max - min) * 0.5;

            MotionBounds = new AABB(min + ext, ext);
        }
    }
}
