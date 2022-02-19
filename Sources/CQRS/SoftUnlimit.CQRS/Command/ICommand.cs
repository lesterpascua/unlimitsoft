using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

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
                .Cast<MasterEventAttribute>();

            return attr?.Select(s => s.EventType);
        }        

        /// <summary>
        /// Generate a acepted response using command data.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="uiText"></param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static ICommandResponse AceptedResponse(this ICommand command, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<object>(skipCommandInfo ? null : command, 202, null, uiText ?? Resources.Response_OkResponse);
        /// <summary>
        /// Generate a acepted response using command data.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText"></param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static ICommandResponse AceptedResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<object>(skipCommandInfo ? null : command, 202, body, uiText ?? Resources.Response_OkResponse);

        /// <summary>
        /// Generate a success response using command data.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="uiText"></param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static ICommandResponse OkResponse(this ICommand command, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<object>(skipCommandInfo ? null : command, 200, null, uiText ?? Resources.Response_OkResponse);
        /// <summary>
        /// Generate a success response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static ICommandResponse OkResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? null : command, 200, body, uiText ?? Resources.Response_OkResponse);

        /// <summary>
        /// Generate a bad response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static ICommandResponse BadResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? null : command, 400, body, uiText ?? Resources.Response_BadResponse);
        
        /// <summary>
        /// Generate a error response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static ICommandResponse ErrorResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? null : command, 500, body, uiText ?? Resources.Response_ErrorResponse);
        
        /// <summary>
        /// Generate a not found response using command data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static ICommandResponse NotFoundResponse<T>(this ICommand command, T body, string uiText = null, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? null : command, 404, body, uiText ?? Resources.Response_NotFoundResponse);
        
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
        public static ICommandResponse Response<T>(this ICommand command, int code, T body, string uiText, bool skipCommandInfo = true) => new CommandResponse<T>(skipCommandInfo ? null : command, code, body, uiText);
    }
}
