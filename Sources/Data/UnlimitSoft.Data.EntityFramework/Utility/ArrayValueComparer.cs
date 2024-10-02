using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
///
/// </summary>
public sealed class ArrayValueComparer<T> : ValueComparer<T[]?> where T : notnull
{
    private static ArrayValueComparer<T>? _instance;

    /// <summary>
    ///
    /// </summary>
    public ArrayValueComparer()
        : base((op1, op2) => Compare(op1, op2), op => Hash(op))
    {
    }

    /// <summary>
    /// Return instance of the converter.
    /// </summary>
    public static ArrayValueComparer<T> Instance
    {
        get
        {
            if (_instance is not null)
                return _instance;
            Interlocked.CompareExchange(ref _instance, new ArrayValueComparer<T>(), null);
            return _instance;
        }
    }

    #region Private Method
    private static int Hash(T[]? op)
    {
        if (op is null)
            return 0;

        const int PRIME = 31;

        var hash = 1;
        foreach (var item in op)
            hash = hash * PRIME + item.GetHashCode();
        return hash;
    }
    private static bool Compare(T[]? op1, T[]? op2)
    {
        if (op1 is null && op2 is null)
            return true;
        if (op2 is null || op1 is null || op1.Length != op2.Length)
            return false;

        for (var i = 0; i < op1.Length; i++)
            if (!op1[i].Equals(op2[i]))
                return false;
        return true;
    }
    #endregion
}
