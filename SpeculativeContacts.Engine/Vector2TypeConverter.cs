using System;
using System.ComponentModel;
using System.Globalization;

namespace SpeculativeContacts.Engine
{
    public class Vector2TypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                culture ??= CultureInfo.InvariantCulture;
                // allow "x,y" or "x, y" or "x y"
                var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2
                    && double.TryParse(parts[0].Trim(), NumberStyles.Float | NumberStyles.AllowThousands, culture, out var x)
                    && double.TryParse(parts[1].Trim(), NumberStyles.Float | NumberStyles.AllowThousands, culture, out var y))
                {
                    return new Vector2(x, y);
                }
                return new Vector2();
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector2 v)
            {
                culture ??= CultureInfo.InvariantCulture;
                return string.Format(culture, "{0}, {1}", v.X, v.Y);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}