using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CloverleafTrack.Web.Utilities;

/// <summary>
/// Extension methods for working with session state
/// </summary>
public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    public static bool TryGet<T>(this ISession session, string key, out T? value)
    {
        var sessionValue = session.GetString(key);
        if (sessionValue != null)
        {
            value = JsonSerializer.Deserialize<T>(sessionValue);
            return true;
        }
        
        value = default;
        return false;
    }
}
