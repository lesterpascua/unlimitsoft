using System;

namespace UnlimitSoft.Evals.Compiler
{
    /// <summary>
    /// Dinamic compliled expression function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="provider"></param>
    /// <param name="argument"></param>
    /// <returns></returns>
    public delegate double ArithmeticExpression<in T>(IServiceProvider provider, T argument);
}
