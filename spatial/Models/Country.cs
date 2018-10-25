using System.Collections.Generic;
using GeoAPI.Geometries;

namespace SpatialDemo.Models
{
    class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IGeometry Border { get; set; }

        public ICollection<State> States { get; } = new HashSet<State>();
    }
}
