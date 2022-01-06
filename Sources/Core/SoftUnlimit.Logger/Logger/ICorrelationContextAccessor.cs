using System.Threading;

namespace SoftUnlimit.Logger
{
    /// <summary>
    /// Define trace context.
    /// </summary>
    public interface ICorrelationContextAccessor
    {
        /// <summary>
        /// 
        /// </summary>
        ICorrelationContext? Context { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class DefaultCorrelationContextAccessor : ICorrelationContextAccessor
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly AsyncLocal<ContextHolder?> _current = new();

        /// <summary>
        /// Assing trace in current context.
        /// </summary>
        public ICorrelationContext? Context
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
            public ICorrelationContext? Context;
        }
        #endregion
    }
}
