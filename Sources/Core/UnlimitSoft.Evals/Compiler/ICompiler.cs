namespace UnlimitSoft.Evals.Compiler
{
    /// <summary>
    /// Compile the code into delegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompiler<in T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ArithmeticExpression<T> Compile(string expression, string name);
    }
}
