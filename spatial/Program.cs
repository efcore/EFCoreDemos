using System;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SpatialDemo.Models;

namespace SpatialDemo
{
    class Program
    {
        static void Main()
        {
            var currentLocation = new Point(-122.128822, 47.643703) { SRID = 4326 };

            using (var db = new WideWorldImportersContext())
            {
                var nearestCity = db.Cities
                    .OrderBy(c => c.Location.Distance(currentLocation))
                    .FirstOrDefault();
                Console.WriteLine($"Nearest city: {nearestCity.Name}");

                var currentState = db.States
                    .FirstOrDefault(s => s.Border.Contains(currentLocation));
                Console.WriteLine($"Current state: {currentState.Name}");

                var route = new GeoJsonReader().Read<ILineString>(File.ReadAllText("seattle-to-new-york.json"));
                route.SRID = 4326;

                var statesCrossed = Enumerable.ToList(
                    from s in db.States
                    where s.Border == null ? false : s.Border.Intersects(route)
                    orderby s.Border.Distance(currentLocation)
                    select s);
                Console.WriteLine("States crossed:");
                foreach (var state in statesCrossed)
                {
                    Console.WriteLine($"\t{state.Name}");
                }

                Console.WriteLine();
                Console.Write("Press any key to continue . . . ");
                Console.ReadKey(intercept: true);
            }
        }
    }
}
