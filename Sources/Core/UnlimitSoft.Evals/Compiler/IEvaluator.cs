using System;

namespace UnlimitSoft.Evals.Compiler
{
    /// <summary>
    /// Allow evaluate some expression in a context.
    /// </summary>
    public interface IEvaluator<in T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="provider"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        double Eval(IServiceProvider provider, string expression, T context);
    }
}
