using System.Windows;
using System.Windows.Media;

namespace SpeculativeContacts.Code
{
    static class WPFUtils
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            int count = VisualTreeHelper.GetChildrenCount(depObj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        public static T FindMatchingAncestor<T>(DependencyObject hit, HashSet<DependencyObject> seen) where T : UIElement
        {
            DependencyObject current = hit;

            while (current != null)
            {
                if (current is T match && !seen.Contains(current))
                    return match;

                // Prefer templated parent if available
                if (current is FrameworkElement fe && fe.TemplatedParent is T templatedMatch && !seen.Contains(fe.TemplatedParent))
                    return templatedMatch;

                current = VisualTreeHelper.GetParent(current);
            }

            // Fallback to logical tree
            current = hit;
            while (current != null)
            {
                var parent = LogicalTreeHelper.GetParent(current);
                if (parent is T logicalMatch && !seen.Contains(parent))
                    return logicalMatch;

                current = parent;
            }

            return null;
        }

        /// <summary>
        /// Returns a collection of <typeparamref name="T"/> that was hit by the specified <paramref name="point"/> inside the visual tree of the specified <paramref name="root"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="UIElement"/> <see cref="Type"/>.</typeparam>
        /// <param name="point">The <see cref="Point"/>.</param>
        /// <param name="root">The <see cref="Visual"/> root.</param>
        /// <returns>The resulting collection of <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> FindElementsInHostCoordinates<T>(Point point, Visual root) where T : UIElement
        {
            if (root is null)
                return Enumerable.Empty<T>();

            var results = new List<T>();
            var seen = new HashSet<DependencyObject>();

            VisualTreeHelper.HitTest(root,
                null,
                new HitTestResultCallback(result =>
                {
                    var hit = result.VisualHit;
                    var matched = FindMatchingAncestor<T>(hit, seen);
                    if (matched != null)
                    {
                        results.Add(matched);
                        seen.Add(matched);
                    }

                    return HitTestResultBehavior.Continue;
                }),
                new PointHitTestParameters(point));

            return results;
        }
    }
}
