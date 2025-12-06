namespace SpeculativeContacts.Code
{
    public class Solver
    {
        public static void Solve(IEnumerable<Contact> contacts, int numIterations, SolverType solverType, double dt)
        {
            int numSolved = 0;

            for (int j = 0; j < numIterations; j++)
            {
                foreach (Contact con in contacts)
                {
                    Vector2 n = con.Normal;

                    // Compute relative velocity
                    Vector2 relVel = con.ComputeRelativeVelocity();

                    // Get normal relative velocity
                    double relNv = relVel.Dot(n);

                    // Get tangential velocity
                    Vector2 tangent = n.Perp();

                    // Get friction
                    double friction = con.Friction;

                    // we want to remove only the amount which leaves them touching next frame
                    if (solverType == SolverType.Speculative)
                    {
                        double remove = relNv + con.Distance / dt;

                        if (remove < 0)
                        {
                            // Normal impulse
                            double mag = remove * con.InvDenom;
                            Vector2 impulse = con.Normal * mag;
                            con.ApplyImpulses(impulse);

                            // Recompute relative velocity
                            relVel = con.ComputeRelativeVelocity();

                            // Friction impulse
                            double absMag = Math.Abs(mag) * friction;
                            double relTv = relVel.Dot(tangent);
                            mag = relTv * con.InvDenomTan;
                            double frictionMag = Scalar.Clamp(mag, -absMag, absMag);
                            
                            impulse = tangent * frictionMag;
                            con.ApplyImpulses(impulse);

                            numSolved++;
                        }
                    }
                    else if (solverType == SolverType.Discrete)
                    {
                        double remove = relNv + 0.4 * (con.Distance + 1) / dt;

                        if (remove < 0 && con.Distance < 0)
                        {
                            // Normal impulse
                            double mag = remove * con.InvDenom;

                            Vector2 impulse = con.Normal * mag;
                            con.ApplyImpulses(impulse);

                            // Recompute relative velocity
                            relVel = con.ComputeRelativeVelocity();

                            // Friction impulse
                            double absMag = Math.Abs(mag) * friction;
                            double relTv = relVel.Dot(tangent);
                            double frictionMag = Scalar.Clamp(relTv * con.InvDenomTan, -absMag, absMag);

                            impulse = tangent * frictionMag;
                            con.ApplyImpulses(impulse);

                            numSolved++;
                        }
                    }
                    else if (solverType == SolverType.DiscreteSequential)
                    {
                        if (con.Distance < 0)
                        {
                            double remove = relNv + 0.4 * (con.Distance + 1) / dt;

                            // Normal impulse
                            double mag = remove * con.InvDenom;
                            double newImpulse = Math.Min(mag + con.Impulse, 0);
                            double change = newImpulse - con.Impulse;

                            Vector2 imp = con.Normal * change;

                            con.ApplyImpulses(imp);
                            con.Impulse = newImpulse;

                            // Recompute relative velocity
                            relVel = con.ComputeRelativeVelocity();

                            // Friction impulse
                            double absMag = Math.Abs(con.Impulse) * friction;
                            double relTv = relVel.Dot(tangent);
                            mag = relTv * con.InvDenomTan;
                            newImpulse = Scalar.Clamp(mag + con.TangentImpulse, -absMag, absMag);
                            change = newImpulse - con.TangentImpulse;
                            imp = tangent * change;

                            con.ApplyImpulses(imp);
                            con.TangentImpulse = newImpulse;

                            numSolved++;
                        }
                    }
                    else if (solverType == SolverType.SpeculativeSequential)
                    {
                        double remove = relNv + con.Distance / dt;

                        // Normal impulse
                        double mag = remove * con.InvDenom;
                        double newImpulse = Math.Min(mag + con.Impulse, 0);
                        double change = newImpulse - con.Impulse;

                        Vector2 imp = con.Normal * change;

                        con.ApplyImpulses(imp);
                        con.Impulse = newImpulse;

                        // Recompute relative velocity
                        relVel = con.ComputeRelativeVelocity();

                        // Friction impulse
                        double absMag = Math.Abs(con.Impulse) * friction;
                        double relTv = relVel.Dot(tangent);
                        mag = relTv * con.InvDenomTan;
                        newImpulse = Scalar.Clamp(mag + con.TangentImpulse, -absMag, absMag);
                        change = newImpulse - con.TangentImpulse;
                        imp = tangent * change;

                        con.ApplyImpulses(imp);
                        con.TangentImpulse = newImpulse;

                        numSolved++;
                    }
                }
            }
        }
    }
}
