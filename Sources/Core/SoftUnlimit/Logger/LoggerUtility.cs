namespace SoftUnlimit.Logger
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoggerUtility
    {
        /// <summary>
        /// Update the current correlation context with the correlation in the parameter.
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="context"></param>
        /// <param name="correlationId"></param>
        public static void SafeUpdateCorrelationContext(ICorrelationContextAccessor accessor, ICorrelationContext context, string correlationId)
        {
            if (context.CorrelationId == null)
                context.SetCorrelationId(correlationId);
            if (accessor.Context == null)
                accessor.Context = context;
        }
    }
}
