namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// 
/// </summary>
public interface ICommand
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetProps<T>() where T : CommandProps;
}