using System;

namespace UnlimitSoft.Evals.Compiler;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IContextEvaluator<T>
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