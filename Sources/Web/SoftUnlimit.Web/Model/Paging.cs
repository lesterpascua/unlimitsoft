namespace SoftUnlimit.Web.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Paging
    {
        /// <summary>
        /// Curren diplayed page
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Amount of element displayed per page.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
