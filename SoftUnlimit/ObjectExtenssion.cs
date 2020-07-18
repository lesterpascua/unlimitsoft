using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;

namespace SoftUnlimit
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObjectExtenssion
    {
        //public static ExpandoObject AsDynamic(this object obj, bool recursive = true)
        //{
        //    IDictionary<string, object> expando = new ExpandoObject();

        //    foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
        //    {
        //        var value = property.GetValue(obj);
        //        if (recursive && value != null && !IsPrimitiveType(value))
        //        {
        //            if (value is IEnumerable enumerable)
        //            {
        //                Collection<ExpandoObject> collection = new Collection<ExpandoObject>();
        //                foreach (var entry in enumerable)
        //                    collection.Add(entry.AsDynamic());
        //                value = collection;
        //            } else
        //                value = value.AsDynamic();
        //        }

        //        expando.Add(property.Name, value);
        //    }
        //    return expando as ExpandoObject;
        //}

        //private static bool IsPrimitiveType(object value)
        //{
        //    Type type = value.GetType();
        //    return type.IsValueType || type == typeof(string);
        //}
    }
}
