using System.Threading.Tasks;

namespace UnlimitSoft.Threading;


/// <summary>
/// 
/// </summary>
/// <param name="Name"></param>
/// <param name="Task"></param>
public sealed record TaskPoolInfo(string Name, Task Task);