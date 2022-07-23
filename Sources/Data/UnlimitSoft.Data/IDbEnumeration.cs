using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UnlimitSoft.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbEnumeration : IEntity, IComparable
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class EnumerationHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable GetAll(Type type)
        {
            var fields = type.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            foreach (var info in fields)
            {
                IDbEnumeration instance = (IDbEnumeration)Activator.CreateInstance(type);
                if (info.GetValue(instance) is IDbEnumeration locatedValue)
                    yield return locatedValue;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>() where T : IDbEnumeration, new() => (IEnumerable<T>)GetAll(typeof(T));
    }
}