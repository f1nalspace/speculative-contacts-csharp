using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace SpeculativeContacts.Behaviors
{
    /// <summary>
    /// Design-time helper: when UseTranslateTransform="True" in the designer,
    /// copies Canvas.Left/Top into (or creates) a TranslateTransform in the element's RenderTransform.
    /// No effect at runtime.
    /// </summary>
    public static class DesignerCanvasTranslateSync
    {
        public static readonly DependencyProperty UseTranslateTransformProperty =
            DependencyProperty.RegisterAttached(
                "UseTranslateTransform",
                typeof(bool),
                typeof(DesignerCanvasTranslateSync),
                new PropertyMetadata(false, OnUseTranslateTransformChanged));

        public static void SetUseTranslateTransform(DependencyObject element, bool value) =>
            element.SetValue(UseTranslateTransformProperty, value);

        public static bool GetUseTranslateTransform(DependencyObject element) =>
            (bool)element.GetValue(UseTranslateTransformProperty);

        // store handlers per element so we can remove them cleanly
        private sealed class HandlerPair
        {
            public EventHandler LeftHandler;
            public EventHandler TopHandler;
        }

        private static readonly ConditionalWeakTable<FrameworkElement, HandlerPair> s_handlers = new();

        private static void OnUseTranslateTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement fe)
                return;

            // Only act when in design mode
            bool isDesign = DesignerProperties.GetIsInDesignMode(fe);
            if (!isDesign)
            {
                // Ensure any previously attached handlers are removed if switching at runtime
                RemoveHandlersIfPresent(fe);
                return;
            }

            if ((bool)e.NewValue)
            {
                AddHandlers(fe);
                // initial sync
                UpdateTranslateFromCanvas(fe);
            }
            else
            {
                RemoveHandlersIfPresent(fe);
            }
        }

        private static void AddHandlers(FrameworkElement fe)
        {
            if (s_handlers.TryGetValue(fe, out _))
                return; // already added

            // create handlers
            HandlerPair pair = new()
            {
                LeftHandler = (s, a) => UpdateTranslateFromCanvas(fe),
                TopHandler = (s, a) => UpdateTranslateFromCanvas(fe)
            };

            var dpdLeft = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(FrameworkElement));
            var dpdTop = DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(FrameworkElement));

            dpdLeft?.AddValueChanged(fe, pair.LeftHandler);
            dpdTop?.AddValueChanged(fe, pair.TopHandler);

            // Also remove handlers when element is unloaded (designer may rehost)
            fe.Unloaded += OnElementUnloaded;

            s_handlers.Add(fe, pair);
        }

        private static void RemoveHandlersIfPresent(FrameworkElement fe)
        {
            if (!s_handlers.TryGetValue(fe, out var pair))
                return;

            var dpdLeft = DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(FrameworkElement));
            var dpdTop = DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(FrameworkElement));

            if (dpdLeft != null)
                dpdLeft.RemoveValueChanged(fe, pair.LeftHandler);
            if (dpdTop != null)
                dpdTop.RemoveValueChanged(fe, pair.TopHandler);

            fe.Unloaded -= OnElementUnloaded;
            s_handlers.Remove(fe);
        }

        private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                // Clean up handlers; the designer may re-load elements often
                RemoveHandlersIfPresent(fe);
            }
        }

        private static void UpdateTranslateFromCanvas(FrameworkElement fe)
        {
            // Only update when in designer and inside a Canvas
            if (!DesignerProperties.GetIsInDesignMode(fe))
                return;

            if (VisualTreeHelper.GetParent(fe) is not Canvas)
                return;

            double left = Canvas.GetLeft(fe);
            double top = Canvas.GetTop(fe);
            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;

            // Ensure RenderTransform is a TransformGroup with a TranslateTransform (append if needed)
            TransformGroup tg = fe.RenderTransform as TransformGroup;
            if (tg is null)
            {
                // create a transform group with common ordering (Scale, Skew, Rotate, Translate)
                tg = new TransformGroup();
                tg.Children.Add(new ScaleTransform());
                tg.Children.Add(new SkewTransform());
                tg.Children.Add(new RotateTransform());
                tg.Children.Add(new TranslateTransform());
                fe.RenderTransform = tg;
            }

            // find or create translate transform (prefer last child)
            TranslateTransform tt = null;
            for (int i = tg.Children.Count - 1; i >= 0; i--)
            {
                if (tg.Children[i] is TranslateTransform t)
                {
                    tt = t;
                    break;
                }
            }

            if (tt is null)
            {
                tt = new TranslateTransform();
                tg.Children.Add(tt);
            }

            // update values if changed
            tt.X = left;
            tt.Y = top;
        }
    }
}
