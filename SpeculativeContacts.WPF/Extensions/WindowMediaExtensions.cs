using SpeculativeContacts.Engine;
using System.Windows.Media;

namespace SpeculativeContacts.Extensions
{
    static class WindowMediaExtensions
    {
        /// <summary>
        /// Extension method that returns a <see cref="Matrix23"/> from the specified <paramref name="m"/>.
        /// </summary>
        /// <param name="m">The <see cref="Matrix"/>.</param>
        /// <returns>The resulting <see cref="Matrix23"/>.</returns>
        public static Matrix23 ToMatrix23(this Matrix m)
        {
            Vector2 row0 = new Vector2(m.M11, m.M12);
            Vector2 row1 = new Vector2(m.M21, m.M22);
            Vector2 offset = new Vector2(m.OffsetX, m.OffsetY);
            return new Matrix23(row0, row1, offset);
        }
    }
}
