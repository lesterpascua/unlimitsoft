using System;

namespace UnlimitSoft.Evals.Compiler
{
    /// <summary>
    /// 
    /// </summary>
    public class EmulateContextEvaluator<T> : IContextEvaluator<T>
    {
        private readonly IEvaluator<T> _evaluator;


        /// <summary>
        /// Create an instace of the evaluation contex using 
        /// </summary>
        /// <param name="evaluator"></param>
        public EmulateContextEvaluator(IEvaluator<T> evaluator)
        {
            _evaluator = evaluator;
        }

        /// <summary>
        /// Evaluate expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="provider"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public double Eval(IServiceProvider provider, string expression, T context)
        {
            return _evaluator.Eval(provider, expression, context);
        }
    }
}