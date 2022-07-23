using System;

namespace UnlimitSoft.Data
{
    /// <summary>
    /// Base entity implementation.
    /// </summary>
    /// <typeparam name="Key"></typeparam>f
    public class Entity<Key> : IEntity
    {
        private int? _requestedHashCode;

        /// <inheritdoc/>
        public Key Id { get; set; }

        /// <inheritdoc/>
        public bool IsTransient()
        {
            if (typeof(Key) == typeof(long) || typeof(Key) == typeof(int) || typeof(Key) == typeof(Guid))
                return Id.Equals(default(Key));

            return false;
        }
        /// <inheritdoc />
        public object GetId() => Id;
        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = Id.GetHashCode() ^ 31;

                return _requestedHashCode.Value;    // XOR for random distribution. See: http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-forgethashcode.aspx
            } else
                return base.GetHashCode();
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null or not Entity<Key>)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (GetType() != obj.GetType())
                return false;
            var item = (Entity<Key>)obj;
            return !item.IsTransient() && !IsTransient() && item.Id.Equals(Id);
        }
    }
}
