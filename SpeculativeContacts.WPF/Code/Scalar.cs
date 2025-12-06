using System;

namespace SpeculativeContacts.Code
{
	public class Scalar
	{
		public static double DegToRad(double degrees)
		{
			return (degrees / 180) * Math.PI;
		}

		public static double RadToDeg(double radians)
		{
			return (radians / Math.PI) * 180;
		}

		public static double Clamp(double a, double min, double max)
		{
			return Math.Min( Math.Max(a, min), max );
		}

		public static double AngleFromVector(Vector2 v)
		{
			return Math.Atan2(v.Y, v.X);
		}

        public static double ComputeTimeStepScale(double sourceValue, double sourceMin, double sourceMax, double targetMin, double targetMax)
        {
            // Normalize factor to 0.0 - 1.0
            double normalized = (sourceValue - sourceMin) / (sourceMax - sourceMin);

			if (targetMin > 0 && targetMax > 0)
			{
				// Interpolate logarithmically
				double logMin = Math.Log10(targetMin);
				double logMax = Math.Log10(targetMax);
				double logInterpolated = logMin + (logMax - logMin) * normalized;

				// Convert back from log scale
				return Math.Pow(10, logInterpolated);
			} 
			else
				return 1.0 - normalized;
        }
    }
}
