using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Snippets;

public static class EnumDisplayNameGetter
{
    private static readonly ConcurrentDictionary<Enum, string> DisplayNameCache = new();

    public static string GetDisplayName(this Enum enumValue)
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        return DisplayNameCache.GetOrAdd(enumValue,
            ev =>
            {
                DisplayAttribute? displayAttribute = GetDisplayAttribute(ev);
                return displayAttribute?.GetName() ?? ev.ToString();
            }
        );
    }

    private static DisplayAttribute? GetDisplayAttribute(Enum enumValue)
    {
        Type enumType = enumValue.GetType();
        FieldInfo? fieldInfo = enumType.GetField(enumValue.ToString(),
            BindingFlags.Public | BindingFlags.Static);
        return fieldInfo?.GetCustomAttribute<DisplayAttribute>();
    }
}