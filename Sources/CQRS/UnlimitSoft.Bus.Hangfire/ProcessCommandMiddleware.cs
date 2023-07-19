using Hangfire;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Message;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
/// <param name="provider"></param>
/// <param name="command"></param>
/// <param name="meta"></param>
/// <param name="next"></param>
/// <param name="ct"></param>
/// <returns></returns>
public delegate Task<IResult> ProcessCommandMiddleware(IServiceProvider provider, ICommand command, JobActivatorContext meta, Func<ICommand, CancellationToken, Task<IResult>> next, CancellationToken ct = default);