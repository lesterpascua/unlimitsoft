using System;
using System.Collections.Generic;
using System.Text;

namespace Own.Web.Filter
{
    /// <summary>
    /// Allow null response (not override to 404 http response)
    /// </summary>
    public class AllowNullResultAttribute : Attribute
    {
    }
}
