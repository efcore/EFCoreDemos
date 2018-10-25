using GeoAPI.Geometries;

namespace SpatialDemo.Models
{
    class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IPoint Location { get; set; }
        public State State { get; set; }
    }
}
