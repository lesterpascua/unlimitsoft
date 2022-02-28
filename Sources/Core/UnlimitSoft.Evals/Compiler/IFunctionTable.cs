using System.Reflection;

namespace UnlimitSoft.Evals.Compiler
{
    /// <summary>
    /// Stote the function availables to compile the expression.
    /// </summary>
    public interface IFunctionTable
    {
        /// <summary>
        /// Get invocable method asociate with name. If not exist return null.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        MethodInfo? Get(string method);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Invoke(string method, object[] parameters);
    }
}
