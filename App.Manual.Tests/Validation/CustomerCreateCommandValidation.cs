using FluentValidation;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.Test.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test.Validation
{


    /// <summary>
    /// 
    /// </summary>
    public sealed class CustomerCreateCommandValidationCache
    {
        /// <summary>
        /// 
        /// </summary>
        public string V1 { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    [CacheType(typeof(CustomerCreateCommandValidationCache))]
    public class CustomerCreateCommandValidation : AbstractCommandValidator<CustomerCreateCommand>
    {
        public CustomerCreateCommandValidation()
        {
            this.RuleFor(p => p.Command.Name).MustAsync((instance, name, pc) => {
                //Thread.Sleep(5000);
                ((CustomerCreateCommandValidationCache)instance.Cache).V1 = "Probando Cache 1";
                return Task.FromResult(true);
            });
            this.RuleFor(p => p.Command.Name).MustAsync((instance, name, pc) => {
                ((CustomerCreateCommandValidationCache)instance.Cache).V1 = "Probando Cache 2";
                return Task.FromResult(true);
            });
        }
    }
}
