using FluentValidation;

namespace UnlimitSoft.CQRS.Command.Validation
{
    /// <summary>
    /// Class for validate if data of command is correct. 
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public sealed class CommandValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        public CommandValidator()
        {
            CascadeMode = DefaultCascadeMode;
        }

        /// <summary>
        /// Default value used for this validator
        /// </summary>
        public static CascadeMode DefaultCascadeMode { get; set; } = CascadeMode.Stop;
    }
}
