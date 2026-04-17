using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace OIU.Libs.Data.PostgreSQL;


/// <summary>
/// 
/// </summary>
public static class PropertyBuilderExtensions
{
    extension (PropertyBuilder<Point?> @this)
    {
        /// <summary>
        /// Configure the property as a geo location with longitude and latitude using PostGIS geometry type. The SRID 4326 is used for WGS 84 coordinate system, which is commonly used for geographic data. This configuration allows you to store and query spatial data efficiently in PostgreSQL.
        /// </summary>
        /// <returns></returns>
        public PropertyBuilder<Point?> HasGeoLocation()
        {
            return @this.HasColumnType("geometry(Point,4326)").HasComment("Geo location with longitude and latitude");
        }
    }
}
