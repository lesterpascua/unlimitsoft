using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoftUnlimit.CQRS.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CommandResponse
    {
        /// <summary>
        /// 
        /// </summary>
        internal protected CommandResponse() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="code"></param>
        /// <param name="uiText"></param>
        internal protected CommandResponse(ICommand command, int code, string uiText)
        {
            this.Code = code;
            this.UIText = uiText;
            this.Command = command;
        }

        /// <summary>
        /// Http code of the notification
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Message diplayed to the user. With generic information.
        /// </summary>
        public string UIText { get; set; }
        /// <summary>
        /// Command where source of response.
        /// </summary>
        public ICommand Command { get; set; }
        /// <summary>
        /// Indicate if command is success. This is only when code beteen 200 and 299.
        /// </summary>
        public bool IsSuccess => 200 <= this.Code && this.Code < 300;

        /// <summary>
        /// Get body.
        /// </summary>
        /// <returns></returns>
        public abstract object GetBody();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Type GetBodyType();
    }
    /// <summary>
    /// 
    /// </summary>
    public class CommandResponse<T> : CommandResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public CommandResponse() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText"></param>
        public CommandResponse(ICommand command, int code, T body, string uiText)
            : base(command, code, uiText)
        {
            this.Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        public T Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object GetBody() => this.Body;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Type GetBodyType() => this.Body?.GetType();
    }
}
