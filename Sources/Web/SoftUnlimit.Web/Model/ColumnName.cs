namespace SoftUnlimit.Web.Model
{
    /// <summary>
    /// Colum name and how is goint to sorting
    /// </summary>
    public class ColumnName
    {
        /// <summary>
        /// Column's name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Indicate if is sorting ascending or descending.
        /// </summary>
        public bool Asc { get; set; } = true;
    }
}
