using Force.DeepCloner;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Data.MongoDb
{
    public class MongoEntity<TKey> : IEntity
    {
        private int? _requestedHashCode;

        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public TKey Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsTransient()
        {
            if (typeof(TKey) == typeof(long) || typeof(TKey) == typeof(int) || typeof(TKey) == typeof(Guid))
                return this.Id.Equals(default(TKey));

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
                    _requestedHashCode = this.Id.GetHashCode() ^ 31;

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
            if (obj == null || !(obj is MongoEntity<TKey>))
                return false;
            if (Object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            MongoEntity<TKey> item = (MongoEntity<TKey>)obj;
            return !item.IsTransient() && !IsTransient() && item.Id.Equals(this.Id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object Clone() => this.DeepClone();


        #region Private Methods

        object IEntity.Id => Id;

        #endregion
    }
}
