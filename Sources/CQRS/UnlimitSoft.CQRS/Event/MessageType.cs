namespace UnlimitSoft.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// The event arrive as standard .net object
        /// </summary>
        Event = 1,
        /// <summary>
        /// The event arrive in json format.
        /// </summary>
        Json = 2,
    }
}
