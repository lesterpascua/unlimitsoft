using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntity : ICloneable
    {
        /// <summary>
        /// Entity identifier.
        /// </summary>
        object ID { get; }

        /// <summary>
        /// Indicate is not initialized yet.
        /// </summary>
        /// <returns></returns>
        bool IsTransient();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    [Serializable]
    public class Entity<Key> : IEntity
    {
        private int? _requestedHashCode;

        /// <summary>
        /// Identificator of entity.
        /// </summary>
        public Key ID { get; set; }

        #region Public Methods


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsTransient()
        {
            if (typeof(Key) == typeof(long) || typeof(Key) == typeof(int) || typeof(Key) == typeof(Guid))
                return this.ID.Equals(default(Key));

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (!this.IsTransient())
            {
                if (!this._requestedHashCode.HasValue)
                    _requestedHashCode = this.ID.GetHashCode() ^ 31;

                return _requestedHashCode.Value;    // XOR for random distribution. See: http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-forgethashcode.aspx
            } else
                return base.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Entity<Key>))
                return false;
            if (Object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            Entity<Key> item = (Entity<Key>)obj;
            return !item.IsTransient() && !IsTransient() && item.ID.Equals(this.ID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object Clone() => this.DeepClone();


        #endregion

        #region Explicit Interface Implementation

        object IEntity.ID => this.ID;

        #endregion
    }
}
