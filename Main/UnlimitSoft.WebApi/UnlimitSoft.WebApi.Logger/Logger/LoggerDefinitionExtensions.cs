namespace UnlimitSoft.WebApi.Logger.Logger
{
    public static partial class LoggerDefinitionExtensions
    {
        private static readonly Action<ILogger, object, Exception?> MyCustomLog = LoggerMessage.Define<object>(LogLevel.Information, 0, "Test logger {@data}");

        public static void TestLogger(this ILogger logger, object data)
        {
            MyCustomLog(logger, data, null);
        }
    }
}
