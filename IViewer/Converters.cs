using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace IViewer {
  public class MatchingIntToBooleanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value != null && parameter as string == ((int)value).ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (!(value is bool b)) {
        return -1;
      }

      var i = System.Convert.ToInt32((parameter ?? "0") as string);

      return b ? System.Convert.ChangeType(i, targetType) : 0;
    }
  }

  public class StringLongConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return System.Convert.ToInt64((value ?? "0") as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is string s && long.TryParse(s, out var l) ? l : 0;
    }
  }

  public class StringDoubleConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return System.Convert.ToDouble((value ?? "0") as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is string s && double.TryParse(s, out var l) ? l : 0;
    }
  }

  public class ValueDescriptionPair {
    public string Description;
    public Enum Value;
  }

  public static class EnumHelper {
    public static string Description(this Enum value) {
      var attributes = value.GetType().GetField(value.ToString())
        .GetCustomAttributes(typeof(DescriptionAttribute), false);
      if (attributes.Any()) { 
        
        return (attributes.First() as DescriptionAttribute)?.Description;
      }

      TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
      return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
    }

    public static IEnumerable<ValueDescriptionPair> GetAllValuesAndDescriptions(Type t) {
      if (!t.IsEnum) {
        throw new ArgumentException($"{nameof(t)} must be an enum type");
      }

      return Enum.GetValues(t)
        .Cast<Enum>()
        .Select(e => new ValueDescriptionPair {Value = e, Description = e.Description()})
        .ToList();
    }
  }

  [ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescriptionPair>))]
  public class EnumToCollectionConverter : MarkupExtension, IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value != null ? EnumHelper.GetAllValuesAndDescriptions(value.GetType()) : Enumerable.Empty<ValueDescriptionPair>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return this;
    }
  }
}