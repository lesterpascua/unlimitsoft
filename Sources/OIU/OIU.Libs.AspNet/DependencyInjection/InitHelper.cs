using System.Text.Json;
using UnlimitSoft.Json;
using UnlimitSoft.Text.Json;

namespace OIU.Libs.AspNet.DependencyInjection;


/// <summary>
/// Initialization helper
/// </summary>
public static class InitHelper
{
    /// <summary>
    /// Initialize the application
    /// </summary>
    /// <param name="setup"></param>
    public static void Init(Action<JsonSerializerOptions, JsonSerializerOptions>? setup = null)
    {
        if (JsonUtil.IsInitialize)
            return;

        lock (Constants.Lock.Instance)
        {
            if (JsonUtil.IsInitialize)
                return;

            // This code allow multiples initialization at the same time (necesary to run test in parallel)
            if (setup is not null)
                setup(DefaultJsonSerializer.SerializerSettings, DefaultJsonSerializer.DeserializerSettings);

            JsonUtil.Default = new DefaultJsonSerializer();
        }
    }
}
