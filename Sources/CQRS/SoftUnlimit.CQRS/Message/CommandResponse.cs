using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Web.Client;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// Base class for all CommandResponse 
    /// </summary>
    public interface ICommandResponse : IResponse
    {
        /// <summary>
        /// Command where source of response.
        /// </summary>
        ICommand Command { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CommandResponse<T> : Response<T>, ICommandResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText"></param>
        internal protected CommandResponse(ICommand command, int code, T body, string uiText)
        {
            Code = code;
            Command = command;
            Body = body;
            UIText = uiText;
        }

        /// <summary>
        /// Command where source of response.
        /// </summary>
        public ICommand Command { get; set; }
    }
}
