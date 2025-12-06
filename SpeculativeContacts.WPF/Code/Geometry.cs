using SpeculativeContacts.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpeculativeContacts.Code
{
    public class Geometry
    {
        public enum FeaturePairType
        {
            Undefined,
            VertexAFaceB,
            VertexBFaceA
        }

        public readonly struct FeaturePair
        {
            public double Distance { get; }
            public int Vertex { get; }
            public int Face { get; }
            public FeaturePairType Feature { get; }
            public double DistanceToCenter { get; }

            internal FeaturePair(double distance, int vertex, int face, FeaturePairType featurePair, double distanceToCenter)
            {
                Distance = distance;
                Vertex = vertex;
                Face = face;
                Feature = featurePair;
                DistanceToCenter = distanceToCenter;
            }
        }

        public static Vector2 ProjectPointOntoEdge(Vector2 p, Vector2 e0, Vector2 e1)
        {
            Vector2 v = p - e0;
            Vector2 e = e1 - e0;

            // time along edge
            double t = e.Dot(v) / e.LenSquared;

            // clamp to edge bounds
            t = Scalar.Clamp(t, 0, 1);

            // form point
            return e0 + e * t;
        }

        private static void FeaturePairJudgement(double dist, double centreDist, int edge, int supportV, FeaturePairType fpc, ref FeaturePair mostSeparated, ref FeaturePair mostPenetrating, Vector2 e0, Vector2 e1)
        {
            if (dist > 0)
            {
                // separating axis, but we want to track closest points anyway

                // recompute distance to clamped edge
                Vector2 p = ProjectPointOntoEdge(new Vector2(), e0, e1);

                // recompute distance
                dist = p.Length;

                if (dist < mostSeparated.Distance)
                {
                    mostSeparated = new FeaturePair(dist, supportV, edge, fpc, centreDist);
                }
                else if (dist == mostSeparated.Distance && fpc == mostSeparated.Feature)
                {
                    // got to pick the right one - pick one closest to centre of A
                    if (centreDist < mostSeparated.DistanceToCenter)
                    {
                        mostSeparated = new FeaturePair(dist, supportV, edge, fpc, centreDist);
                    }
                }
            }
            else
            {
                // penetration
                if (dist > mostPenetrating.Distance)
                {
                    mostPenetrating = new FeaturePair(dist, supportV, edge, fpc, centreDist);
                }
                else if (dist == mostPenetrating.Distance && fpc == mostPenetrating.Feature)
                {
                    // got to pick the right one - pick one closest to centre of A
                    if (centreDist < mostPenetrating.DistanceToCenter)
                    {
                        mostPenetrating = new FeaturePair(dist, supportV, edge, fpc, centreDist);
                    }
                }
            }
        }

        public static List<Contact> RectRectClosestPoints(Rectangle A, Rectangle B)
        {
            List<Contact> contacts = new List<Contact>();

            FeaturePair mostSeparated = new FeaturePair(double.MaxValue, -1, -1, FeaturePairType.Undefined, double.MaxValue);
            FeaturePair mostPenetrating = new FeaturePair(-double.MaxValue, -1, -1, FeaturePairType.Undefined, double.MaxValue);

            // face of A, vertices of B
            for (int i = 0; i < A.LocalSpaceNormals.Length; i++)
            {
                // get world space normal
                Vector2 wsN = A.GetWorldSpaceNormal(i);
                Vector2 wsV0 = A.GetWorldSpacePoint(i);
                Vector2 wsV1 = A.GetWorldSpacePoint((i + 1) % A.LocalSpaceNormals.Length);

                // get supporting vertices of B, most opposite face normal
                SupportVertex[] s = B.GetSupportVertices(-wsN);

                for (int j = 0; j < s.Length; j++)
                {
                    // form point on plane of minkowski face
                    Vector2 mfp0 = s[j].Vertex - wsV0;
                    Vector2 mfp1 = s[j].Vertex - wsV1;

                    // distance from origin to face
                    double dist = mfp0.Dot(wsN);

                    // distance to centre of A
                    double centreDist = (s[j].Vertex - A.Position).LenSquared;

                    // pick correct feature pair
                    FeaturePairJudgement(dist, centreDist, i, s[j].Index, FeaturePairType.VertexBFaceA, ref mostSeparated, ref mostPenetrating, mfp0, mfp1);
                }
            }

            // faces of B, vertices of A
            for (int i = 0; i < B.LocalSpaceNormals.Length; i++)
            {
                // get world space normal
                Vector2 wsN = B.GetWorldSpaceNormal(i);
                Vector2 wsV0 = B.GetWorldSpacePoint(i);
                Vector2 wsV1 = B.GetWorldSpacePoint((i + 1) % B.LocalSpaceNormals.Length);


                // get supporting vertices of A, most opposite face normal
                SupportVertex[] s = A.GetSupportVertices(-wsN);

                for (int j = 0; j < s.Length; j++)
                {
                    // form point on plane of minkowski face
                    Vector2 mfp0 = s[j].Vertex - wsV0;
                    Vector2 mfp1 = s[j].Vertex - wsV1;

                    // distance from origin to face
                    double dist = mfp0.Dot(wsN);

                    // distance to centre of B
                    double centreDist = (s[j].Vertex - B.Position).LenSquared;

                    // pick correct feature pair
                    FeaturePairJudgement(dist, centreDist, i, s[j].Index, FeaturePairType.VertexAFaceB, ref mostSeparated, ref mostPenetrating, mfp0, mfp1);
                }
            }

            FeaturePair featureToUse;
            Rectangle vertexRect = null;
            Rectangle faceRect = null;

            if (mostSeparated.Distance > 0 && mostSeparated.Feature != FeaturePairType.Undefined)
            {
                // objects are separated
                featureToUse = mostSeparated;
            }
            else if (mostPenetrating.Distance <= 0)
            {
                // objects are penetrating
                Debug.Assert(mostPenetrating.Feature != FeaturePairType.Undefined);

                featureToUse = mostPenetrating;
            }
            else
            {
                throw new Exception("RectRectClosestPoints(): Impossible condition!");
            }

            if (featureToUse.Feature == FeaturePairType.VertexAFaceB)
            {
                vertexRect = A;
                faceRect = B;
            }
            else
            {
                vertexRect = B;
                faceRect = A;
            }

            // world space vertex
            Vector2 worldN = faceRect.GetWorldSpaceNormal(featureToUse.Face);

            // other vertex adjcent which makes most parallel normal with the collision normal
            Vector2[] worldV = vertexRect.GetSecondSupport(featureToUse.Vertex, worldN);

            // world space edge
            Vector2 worldEdge0 = faceRect.GetWorldSpacePoint(featureToUse.Face);
            Vector2 worldEdge1 = faceRect.GetWorldSpacePoint((featureToUse.Face + 1) % faceRect.LocalSpacePoints.Length);

            // form contact
            Vector2[] pointsOnA = new Vector2[2];
            Vector2[] pointsOnB = new Vector2[2];

            if (featureToUse.Feature == FeaturePairType.VertexAFaceB)
            {
                // project vertex onto edge
                pointsOnA[0] = ProjectPointOntoEdge(worldEdge0, worldV[0], worldV[1]);
                pointsOnA[1] = ProjectPointOntoEdge(worldEdge1, worldV[0], worldV[1]);

                pointsOnB[0] = ProjectPointOntoEdge(worldV[1], worldEdge0, worldEdge1);
                pointsOnB[1] = ProjectPointOntoEdge(worldV[0], worldEdge0, worldEdge1);

                worldN = -worldN;
            }
            else
            {
                pointsOnA[0] = ProjectPointOntoEdge(worldV[1], worldEdge0, worldEdge1);
                pointsOnA[1] = ProjectPointOntoEdge(worldV[0], worldEdge0, worldEdge1);

                pointsOnB[0] = ProjectPointOntoEdge(worldEdge0, worldV[0], worldV[1]);
                pointsOnB[1] = ProjectPointOntoEdge(worldEdge1, worldV[0], worldV[1]);
            }

            double d0 = (pointsOnB[0] - pointsOnA[0]).Dot(worldN);
            double d1 = (pointsOnB[1] - pointsOnA[1]).Dot(worldN);

            contacts.Add(new Contact(A, B, pointsOnA[0], pointsOnB[0], worldN, d0));
            contacts.Add(new Contact(A, B, pointsOnA[1], pointsOnB[1], worldN, d1));

            return contacts;
        }
    }
}
