using System.Collections.Generic;
using System.Text;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.Data.SqlServer;


/// <summary>
/// 
/// </summary>
public static class Statment
{
    /// <summary>
    /// Gets the ORDER BY statement based on the specified order and default column.
    /// </summary>
    /// <param name="order">The list of column names and sort order.</param>
    /// <param name="defaultColumn">The default column name.</param>
    /// <param name="query"></param>
    /// <returns>The ORDER BY statement.</returns>
    public static void AppendOrderStatment(IReadOnlyList<ColumnName> order, string defaultColumn, StringBuilder query)
    {
        switch (order.Count)
        {
            case 0:
                query.Append("ORDER BY [").Append(defaultColumn).Append(']');
                break;
            case 1:
                query.Append("ORDER BY [").Append(order[0].Name).Append("] ").Append(order[0].Asc ? "ASC" : "DESC");
                break;
            default:
                query.Append("ORDER BY");
                foreach (var entry in order)
                    query.Append(" [").Append(entry.Name).Append(']').Append(entry.Asc ? " ASC" : " DESC");
                break;
        }
    }
}
