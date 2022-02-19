using System.Threading;

namespace SoftUnlimit.Logger
{
    /// <summary>
    /// Define a several variables in a context
    /// </summary>
    public interface ILoggerContextAccessor
    {
        /// <summary>
        /// 
        /// </summary>
        LoggerContext? Context { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class LoggerContextAccessor : ILoggerContextAccessor
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly AsyncLocal<ContextHolder?> _current = new();

        /// <inheritdoc />
        public LoggerContext? Context
        {
            get => _current.Value?.Context;
            set
            {
                var holder = _current.Value;
                // Clear current HttpContext trapped in the AsyncLocals, as its done.
                if (holder is not null)
                    holder.Context = null;

                // Use an object indirection to hold the CorrelationContext in the AsyncLocal, so it can be cleared in all ExecutionContexts when its cleared.
                if (value is not null)
                    _current.Value = new ContextHolder { Context = value };
            }
        }

        #region Nested Classes
        private class ContextHolder
        {
            public LoggerContext? Context;
        }
        #endregion
    }
}