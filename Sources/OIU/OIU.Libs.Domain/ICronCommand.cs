using UnlimitSoft.CQRS.Command;

namespace OIU.Libs.Domain;


/// <summary>
/// 
/// </summary>
public interface ICronCommand : ICommand
{
    /// <summary>
    /// Indicate the retry is enable or not. By default retry is not enable in cron commands
    /// </summary>
    /// <returns></returns>
    bool IsRetryEnable();
}
