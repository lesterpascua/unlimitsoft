namespace OIU.Libs;


/// <summary>
/// Environment in the system
/// </summary>
public enum EnvType
{
    /// <summary>
    /// Local develop environment
    /// </summary>
    LOCAL = 0,
    /// <summary>
    /// Test environment is running unit test or integration test
    /// </summary>
    TEST = 1,
    /// <summary>
    /// Azure Develop environment
    /// </summary>
    DEV = 2,
    /// <summary>
    /// 
    /// </summary>
    QA = 3,
    /// <summary>
    /// UAT 
    /// </summary>
    UAT = 4,
    /// <summary>
    /// Preprod environment
    /// </summary>
    BETA = 5,
    /// <summary>
    /// 
    /// </summary>
    PROD = 6
}
/// <summary>
/// Helper to assign name to the environments
/// </summary>
public static class EnvTypeHelper
{
    private static int _envType;
    private static object? _logger;

    /// <summary>
    /// ILogger by default used when unit test is running
    /// </summary>
    public static object? Logger => _logger;
    /// <summary>
    /// Get environment name
    /// </summary>
    /// <returns></returns>
    public static EnvType Id => (EnvType)_envType;
    /// <summary>
    /// Get environment name
    /// </summary>
    /// <returns></returns>
    public static string Name => ((EnvType)_envType).ToString();
    /// <summary>
    /// Verify if is test environment
    /// </summary>
    /// <returns></returns>
    public static bool IsTestEnv => _envType == (int)EnvType.TEST;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public static void SetLogger(object? logger) => Interlocked.CompareExchange(ref _logger, logger, null);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="envType"></param>
    public static void SetEnvType(EnvType envType) => Interlocked.CompareExchange(ref _envType, (int)envType, 0);
}
