using SpeculativeContacts.Engine;
using System.Windows.Media;

namespace SpeculativeContacts.Extensions
{
    static class EngineExtensions
    {
        /// <summary>
        /// Rotates the specified <paramref name="position"/> into the space defined by the given <paramref name="matrix"/>.
        /// </summary>
        /// <param name="position">The <see cref="Vector2"/>.</param>
        /// <param name="matrix">The <see cref="Matrix"/>.</param>
        /// <returns>The resulting <see cref="Vector2"/>.</returns>
        public static Vector2 RotateIntoSpaceOf(this Vector2 position, Matrix matrix)
        {
            Matrix23 m = matrix.ToMatrix23();
            return position.RotateIntoSpaceOf(m);
        }

        public static Vector2 RotateBy(this Vector2 position, Matrix matrix)
        {
            Matrix23 m = matrix.ToMatrix23();
            return position.RotateBy(m);
        }
    }
}
