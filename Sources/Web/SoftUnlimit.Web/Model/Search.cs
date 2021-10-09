namespace SoftUnlimit.Web.Model
{
    /// <summary>
    /// Model with search data.
    /// </summary>
    /// <typeparam name="TFilter"></typeparam>
    public class Search<TFilter>
        where TFilter : class
    {
        /// <summary>
        /// Contain page information
        /// </summary>
        public Paging Paging { get; set; } = new Paging();
        /// <summary>
        /// Array of column, the ordenation is in the order on the array.
        /// </summary>
        public ColumnName[] Order { get; set; }

        /// <summary>
        /// Contain different filtered paramas.
        /// </summary>
        public TFilter Filter { get; set; }
    }
}
