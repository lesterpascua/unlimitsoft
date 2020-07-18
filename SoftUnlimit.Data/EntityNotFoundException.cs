using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public EntityNotFoundException(long id)
            : base($"Not fount entity with Id: {id}")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        public EntityNotFoundException(Type entity, long id)
            : base($"Not fount entity of type: {entity} with Id: {id}")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="innerException"></param>
        public EntityNotFoundException(long id, Exception innerException)
            : base($"Not fount entity with Id: {id}", innerException)
        {
        }
    }
}
