using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Web.Security;
using System;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


public class MyCommand : Command<MyCommandProps>
{
    public MyCommand() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    public MyCommand(Guid id, IdentityInfo user)
    {
        Props = new MyCommandProps
        {
            Id = id,
            Name = GetType().Name,
            User = user
        };
    }
}
