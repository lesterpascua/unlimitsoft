﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.Model
{
    /// <summary>
    /// Model with search data.
    /// </summary>
    /// <typeparam name="TFilter"></typeparam>
    public abstract class SearchViewModel<TFilter>
        where TFilter : class
    {
        /// <summary>
        /// Data to be include in query.
        /// </summary>
        public string[] Include { get; set; }
        /// <summary>
        /// Contain page information
        /// </summary>
        public PaggingViewModel Pagging { get; set; }
        /// <summary>
        /// Array of column, the ordenation is in the order on the array.
        /// </summary>
        public ColumnNameViewModel[] Order { get; set; }

        /// <summary>
        /// Contain different filtered paramas.
        /// </summary>
        public TFilter Filter { get; set; }
    }
}