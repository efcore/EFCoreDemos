using System.Collections.Generic;
using GeoAPI.Geometries;

namespace SpatialDemo.Models
{
    class State
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IGeometry Border { get; set; }
        public Country Country { get; set; }

        public ICollection<City> Cities { get; } = new HashSet<City>();
    }
}
