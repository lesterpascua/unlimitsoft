using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftUnlimit.CQRS.Command
{
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
    /// <summary>
    /// 
    /// </summary>
    public static class ICommandExtenssion
    {
        /// <summary>
        /// Search if command has MaterEventAttribute and get type asociate.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetMasterEvent(this ICommand command)
        {
            var attr = command.GetType()
                .GetCustomAttributes(typeof(MasterEventAttribute), true)
                .Cast< MasterEventAttribute>();

            return attr?.Select(s => s.EventType);
        }

        /// <summary>
        /// Generate a success response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static CommandResponse OkResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? new SealedCommand(command.GetProps<CommandProps>()) : command, 200, body, uiText ?? Resources.Response_OkResponse);
        /// <summary>
        /// Generate a bad response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static CommandResponse BadResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? new SealedCommand(command.GetProps<CommandProps>()) : command, 400, body, uiText ?? Resources.Response_BadResponse);
        /// <summary>
        /// Generate a error response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static CommandResponse ErrorResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? new SealedCommand(command.GetProps<CommandProps>()) : command, 500, body, uiText ?? Resources.Response_ErrorResponse);
        /// <summary>
        /// Generate a not found response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static CommandResponse NotFoundResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? new SealedCommand(command.GetProps<CommandProps>()) : command, 404, body, uiText ?? Resources.Response_NotFoundResponse);
        /// <summary>
        /// Generate a response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static CommandResponse Response<T>(this ICommand command, int code, T body, string uiText, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? new SealedCommand(command.GetProps<CommandProps>()) : command, code, body, uiText);
    }
}
