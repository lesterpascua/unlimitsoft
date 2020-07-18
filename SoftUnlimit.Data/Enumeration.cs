using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SoftUnlimit.Data
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
    public abstract class Enumeration<TKey> : Entity<TKey>, IDbEnumeration
        where TKey : Enum
    {
        /// <summary>
        /// 
        /// </summary>
        protected Enumeration()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        protected Enumeration(TKey id, string name)
        {
            this.ID = id;
            this.Name = name;
        }


        /// <summary>
        /// 
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(object other) => this.ID.CompareTo(((Enumeration<TKey>)other).ID);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.ID.GetHashCode();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.Name;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Enumeration<TKey> otherValue))
                return false;

            bool valueMatches = this.ID.Equals(otherValue.ID);
            bool typeMatches = this.GetType().Equals(obj.GetType());

            return typeMatches && valueMatches;
        }
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
        public static IEnumerable<T> GetAll<T>() where T : IDbEnumeration, new() => (IEnumerable<T>)EnumerationHelper.GetAll(typeof(T));
    }
}