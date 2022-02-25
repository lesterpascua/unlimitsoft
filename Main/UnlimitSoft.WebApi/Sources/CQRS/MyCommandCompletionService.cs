﻿using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.CQRS
{
    public class MyCommandCompletionService : ICommandCompletionService
    {
        private readonly ILogger<MyCommandCompletionService> _logger;

        public MyCommandCompletionService(ILogger<MyCommandCompletionService> logger)
        {
            _logger = logger;
        }
        public Task CompleteAsync(ICommand command, ICommandResponse response, Exception ex = null, CancellationToken ct = default)
        {
            if (ex is null)
            {
                _logger.LogInformation("Command execution complete witout error.");
            }
            else
                _logger.LogInformation(ex, "Command execution complete with error.");
            return Task.CompletedTask;
        }
    }
}