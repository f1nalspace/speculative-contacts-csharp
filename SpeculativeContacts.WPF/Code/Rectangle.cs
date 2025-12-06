using SpeculativeContacts.Engine;
using SpeculativeContacts.Extensions;
using System.Diagnostics;
using System.Windows.Media;

namespace SpeculativeContacts.Code
{
    public struct SupportVertex
    {
        public Vector2 Vertex;
        public int Index;
    }

    public class Rectangle : RigidBody
    {
        public Vector2 HalfExtents => _halfExtents;
        private Vector2 _halfExtents;

        public Vector2[] LocalSpaceNormals => _localSpaceNormals;
        private Vector2[] _localSpaceNormals;

        public Vector2[] LocalSpacePoints => _localSpacePoints;
        private Vector2[] _localSpacePoints;

        public override BodyType Type => InvI > 0 ? BodyType.Dynamic : BodyType.Static;

        public Rectangle() : base()
        {
        }

        public Vector2 GetWorldSpacePoint(int i)
        {
            return Transformation.TransformBy(_localSpacePoints[i]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector2 GetWorldSpaceNormal(int i)
        {
            return Transformation.RotateBy(_localSpaceNormals[i]);
        }

        public override void Initialise(double invMass, Vector2 initialVelocity, double initialAngulatVelocity, double initialFriction = 0.0)
        {
            base.Initialise(invMass, initialVelocity, initialAngulatVelocity, initialFriction);

            _halfExtents = new Vector2(this.Width / 2, this.Height / 2);

            // form local space points
            _localSpacePoints = new Vector2[]
            {
                new Vector2(_halfExtents.X, -_halfExtents.Y),
                new Vector2(-_halfExtents.X, -_halfExtents.Y),
                new Vector2(-_halfExtents.X, _halfExtents.Y),
                new Vector2(_halfExtents.X, _halfExtents.Y)
            };

            // and local space normals
            _localSpaceNormals = new Vector2[_localSpacePoints.Length];
            for (int i = 0; i < _localSpacePoints.Length; i++)
            {
                _localSpaceNormals[i] = (_localSpacePoints[(i + 1) % _localSpacePoints.Length] - _localSpacePoints[i]).Unit().Perp();
            }

            // calculate inverse inertia tensor
            if (invMass > 0)
            {
                double mass = 1 / invMass;
                double I = mass * (this.Width * this.Width + this.Height * this.Height) / 12.0f;
                InvI = 1 / I;
            }
            else
                InvI = 0;
        }

        public override List<Contact> GetClosestPoints(RigidBody b)
        {
            List<Contact> contacts = new List<Contact>();

            if (b is Ball)
            {
                Rectangle ra = this;
                Ball rb = (Ball)b;

                // clamp point to exterior of rectangle
                Vector2 delta = rb.Position - ra.Position;

                // rotate into space of render transform
                TransformGroup tg = (TransformGroup)this.RenderTransform;

                Matrix matrix = tg.Value;

                Vector2 rdelta = delta.RotateIntoSpaceOf(matrix);

                Vector2 dClamped = Vector2.Clamp(rdelta, -_halfExtents, _halfExtents);
                Vector2 clampedP = ra.Position + dClamped.RotateBy(matrix);

                // vector from clamped point to circle
                Vector2 d = rb.Position - clampedP;
                Vector2 n = d.Unit();
                if (d.LenSquared == 0)
                    n = delta.MajorAxis();

                // form closest points
                Vector2 pa = clampedP;
                Vector2 pb = rb.Position - n * rb.Radius;

                // return distance
                double dist = d.Length - rb.Radius;

                contacts.Add(new Contact(ra, rb, pa, pb, n, dist));
            }
            else if (b is Plane)
            {
                Plane rb = (Plane)b;

                Vector2[] worldP = new Vector2[_localSpacePoints.Length];
                double[] worldD = new double[_localSpacePoints.Length];
                int i = 0;
                foreach (Vector2 v in _localSpacePoints)
                {
                    // world space rect point
                    worldP[i] = Transformation.TransformBy(v);

                    // distance to plane
                    worldD[i] = rb.DistanceToPoint(worldP[i]);
                    i++;
                }

                int closest = -1;
                int secondClosest = -1;
                double closestD = double.MaxValue;
                double secondClosestD = double.MaxValue;
                for (i = 0; i < _localSpacePoints.Length; i++)
                {
                    if (worldD[i] < closestD)
                    {
                        closestD = worldD[i];
                        closest = i;
                    }
                }
                for (i = 0; i < _localSpacePoints.Length; i++)
                {
                    if (i != closest && worldD[i] < secondClosestD)
                    {
                        secondClosestD = worldD[i];
                        secondClosest = i;
                    }
                }

                Debug.Assert(closest != -1);
                Debug.Assert(secondClosest != -1);

                // normal points from a->b
                Contact ca = new Contact(this, rb, worldP[closest], rb.ProjectPointOntoPlane(worldP[closest]),
                                                    rb.Normal, worldD[closest]);

                Contact cb = new Contact(this, rb, worldP[secondClosest], rb.ProjectPointOntoPlane(worldP[secondClosest]),
                                                    rb.Normal, worldD[secondClosest]);
                contacts.Add(ca);

                contacts.Add(cb);
            }
            else if (b is Rectangle)
            {
                return Geometry.RectRectClosestPoints(this, (Rectangle)b);
            }
            else
            {
                throw new NotImplementedException();
            }

            return contacts;
        }

        public SupportVertex[] GetSupportVertices(Vector2 direction)
        {
            // rotate into rectangle space
            Vector2 v = Transformation.RotateIntoSpaceOf(direction);

            // get axis bits
            int closestI = -1;
            int secondClosestI = -1;
            double closestD = -double.MaxValue;
            double secondClosestD = -double.MaxValue;

            // first support
            for (int i = 0; i < _localSpacePoints.Length; i++)
            {
                double d = v.Dot(_localSpacePoints[i]);

                if (d > closestD)
                {
                    closestD = d;
                    closestI = i;
                }
            }

            // second support
            int num = 1;
            for (int i = 0; i < _localSpacePoints.Length; i++)
            {
                double d = v.Dot(_localSpacePoints[i]);

                if (i != closestI && d == closestD)
                {
                    secondClosestD = d;
                    secondClosestI = i;
                    num++;
                    break;
                }
            }

            // closest vertices
            SupportVertex[] spa = new SupportVertex[num];
            spa[0] = new SupportVertex { Index = closestI, Vertex = Transformation.TransformBy(_localSpacePoints[closestI]) };
            if (num > 1)
            {
                spa[1] = new SupportVertex { Index = secondClosestI, Vertex = Transformation.TransformBy(_localSpacePoints[secondClosestI]) };
            }

            return spa;
        }

        public Vector2[] GetSecondSupport(int v, Vector2 n)
        {
            Vector2 va = GetWorldSpacePoint((v - 1 + _localSpacePoints.Length) % _localSpacePoints.Length);
            Vector2 vb = GetWorldSpacePoint(v);
            Vector2 vc = GetWorldSpacePoint((v + 1) % _localSpacePoints.Length);

            Vector2 na = (vb - va).Perp().Unit();
            Vector2 nc = (vc - vb).Perp().Unit();

            Vector2[] support = new Vector2[2];

            if (na.Dot(n) < nc.Dot(n))
            {
                support[0] = va;
                support[1] = vb;
            }
            else
            {
                support[0] = vb;
                support[1] = vc;
            }

            return support;
        }


        /// <summary>
        /// 
        /// </summary>
        public override void GenerateMotionAABB(double dt)
        {
            // get bounds now
            AABB boundsNow = AABB.BuildAABB(Transformation, _localSpacePoints);

            // work out transform for next frame
            Matrix23 matrixNextFrame = new Matrix23(Angle + AngularVel * dt, Position + Vel * dt);

            // bounds then
            AABB boundsNextFrame = AABB.BuildAABB(matrixNextFrame, _localSpacePoints);

            // bound both
            MotionBounds = new AABB(boundsNow, boundsNextFrame);
        }
    }
}
