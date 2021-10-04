using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftUnlimit.Document.Provider
{
    /// <summary>
    /// ROundRound robin pick selection.
    /// </summary>
    public class RoundRobinSelectionAlgorithm : ISelectionAlgorithm
    {
        #region Field

        private int _current;
        private readonly IDocumentProvider[] _providers;

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        public RoundRobinSelectionAlgorithm(IEnumerable<IDocumentProvider> providers)
        {
            this._current = 0;
            this._providers = providers.ToArray();
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Select next provider where document is goint to save.
        /// </summary>
        /// <param name="allocSpace"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If not exist space to allocate a file.</exception>
        public IDocumentProvider Pick(long allocSpace)
        {
            int iteration = 0;
            IDocumentProvider provider = null;
            do
            {
                if (iteration == this._providers.Length)
                    throw new InvalidOperationException("Not enouch space exception");

                IDocumentProvider aux = this._providers[this._current];
                if (aux.MaxSpace < aux.CurrentSpace + allocSpace)
                {
                    if (this._providers.Length <= ++this._current)
                        this._current = 0;
                    iteration++;
                } else
                    provider = aux;
            } while (provider == null);

            return provider;
        }

        #endregion
    }
}
