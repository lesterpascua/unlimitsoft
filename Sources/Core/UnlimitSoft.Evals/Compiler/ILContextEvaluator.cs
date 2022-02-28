using System;
using System.Collections.Generic;

namespace UnlimitSoft.Evals.Compiler;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class ILContextEvaluator<T>
{
    private readonly bool _useCache;
    private readonly ICompiler<T> _compiler;
    private readonly Dictionary<string, ArithmeticExpression<T>> _cache;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="compiler"></param>
    /// <param name="useCache"></param>
    public ILContextEvaluator(ICompiler<T> compiler, bool useCache)
    {
        _compiler = compiler;
        _useCache = useCache;
        _cache = new Dictionary<string, ArithmeticExpression<T>>();
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
        var dynamicMethod = GetDynamicMethod(expression);
        return dynamicMethod.Invoke(provider, context);
    }

    #region Private Methods
    private ArithmeticExpression<T> GetDynamicMethod(string expression)
    {
        if (!_useCache)
            return _compiler.Compile(expression, string.Empty);

        if (!_cache.TryGetValue(expression, out var dynamicMethod))
            lock (_cache)
                if (!_cache.TryGetValue(expression, out dynamicMethod))
                    _cache.Add(expression, dynamicMethod = _compiler.Compile(expression, string.Empty));
        return dynamicMethod;
    }
    #endregion
}