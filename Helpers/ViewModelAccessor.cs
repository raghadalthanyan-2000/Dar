using System.Collections;
using System.Globalization;
using System.Reflection;

namespace dar_system.Helpers;

public static class ViewModelAccessor
{
    public static object? Prop(object? model, string name)
    {
        if (model is null) return null;
        var type = model.GetType();
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(model);
    }

    public static T? Prop<T>(object? model, string name)
    {
        var value = Prop(model, name);
        if (value is null) return default;
        if (value is T typed) return typed;
        try
        {
            return (T?)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
        catch
        {
            return default;
        }
    }

    public static IEnumerable<object> List(object? value)
    {
        if (value is null) return Enumerable.Empty<object>();
        if (value is string) return new[] { value };
        if (value is IEnumerable enumerable) return enumerable.Cast<object>();
        return new[] { value };
    }

    public static string Text(object? value, string fallback = "-")
    {
        if (value is null) return fallback;
        var s = value.ToString();
        return string.IsNullOrWhiteSpace(s) ? fallback : s;
    }

    public static string Date(object? value, string format = "yyyy-MM-dd HH:mm")
    {
        if (value is null) return "-";
        if (value is DateTime dt) return dt.ToString(format, CultureInfo.InvariantCulture);
        if (DateTime.TryParse(value.ToString(), out var parsed)) return parsed.ToString(format, CultureInfo.InvariantCulture);
        return Text(value);
    }

    public static string Money(object? value)
    {
        if (value is null) return "0.00";
        if (decimal.TryParse(value.ToString(), out var decimalValue)) return decimalValue.ToString("0.00", CultureInfo.InvariantCulture);
        return Text(value, "0.00");
    }

    public static int Int(object? value)
    {
        if (value is null) return 0;
        if (value is int i) return i;
        if (int.TryParse(value.ToString(), out var parsed)) return parsed;
        return 0;
    }
}

