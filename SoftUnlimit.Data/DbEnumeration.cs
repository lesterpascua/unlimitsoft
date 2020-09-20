﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DbEnumeration<TKey> : Entity<TKey>, IDbEnumeration
        where TKey : Enum
    {
        /// <summary>
        /// 
        /// </summary>
        protected DbEnumeration()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        protected DbEnumeration(TKey id, string name)
        {
            this.Id = id;
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
        public int CompareTo(object other) => this.Id.CompareTo(((DbEnumeration<TKey>)other).Id);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.Id.GetHashCode();
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
            if (!(obj is DbEnumeration<TKey> otherValue))
                return false;

            bool valueMatches = this.Id.Equals(otherValue.Id);
            bool typeMatches = this.GetType().Equals(obj.GetType());

            return typeMatches && valueMatches;
        }
    }
}