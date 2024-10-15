namespace CRB;

public class Maps{
    public class Coordinate
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class TransverseMercator
    {
        public short CentralMeridian { get; set; }
        public double FalseEasting { get; set; }
        public double FalseNorthing { get; set; }
        public double ScaleFactor { get; set; }
        public string MapName { get; set; }
    }

    public static class Projections
    {

        public static readonly TransverseMercator CA = new TransverseMercator
        {
            CentralMeridian = 33,
            FalseEasting = -99516.99999997323,
            FalseNorthing = -4998114.999999984,
            ScaleFactor = 0.9996,
            MapName = "Caucasus"
        };

        public static readonly TransverseMercator MA = new TransverseMercator
        {
            CentralMeridian = 147,
            FalseEasting = 238417.99999989968,
            FalseNorthing = -1491840.000000048,
            ScaleFactor = 0.9996,
            MapName = "MarianaIslands"
        };

        public static readonly TransverseMercator NV = new TransverseMercator
        {
            CentralMeridian = -117,
            FalseEasting = -193996.80999964548,
            FalseNorthing = -4410028.063999966,
            ScaleFactor = 0.9996,
            MapName = "Nevada"
        };

        public static readonly TransverseMercator NO = new TransverseMercator
        {
            CentralMeridian = -3,
            FalseEasting = -195526.00000000204,
            FalseNorthing = -5484812.999999951,
            ScaleFactor = 0.9996,
            MapName = "Normandy"
        };

        public static readonly TransverseMercator PG = new TransverseMercator
        {
            CentralMeridian = 57,
            FalseEasting = 75755.99999999645,
            FalseNorthing = -2894933.0000000377,
            ScaleFactor = 0.9996,
            MapName = "PersianGulf"
        };

        public static readonly TransverseMercator SA = new TransverseMercator
        {
            CentralMeridian = -57,
            FalseEasting = 147639.99999997593,
            FalseNorthing = 5815417.000000032,
            ScaleFactor = 0.9996,
            MapName = "SouthAtlantic"
        };

        public static readonly TransverseMercator SI = new TransverseMercator
        {
            CentralMeridian = 33,
            FalseEasting = 169221.9999999585,
            FalseNorthing = -3325312.9999999693,
            ScaleFactor = 0.9996,
            MapName = "SinaiMap"  //wtf?  thanks DCS for naming conventions
        };

        public static readonly TransverseMercator SY = new TransverseMercator
        {
            CentralMeridian = 39,
            FalseEasting = 282801.00000003993,
            FalseNorthing = -3879865.9999999935,
            ScaleFactor = 0.9996,
            MapName = "Syria"
        };

        public static readonly Dictionary<string, TransverseMercator> TheatreToProjectionMap = new Dictionary<string, TransverseMercator>
        {//TODO: Get new maps
            { "CAUCUSES", CA },
            { "MARIANAS", MA },
            { "NEVADA", NV },
            { "NORMANDY", NO },
            { "PERSIAN GULF", PG },
            { "SINAI", SI },
            { "SOUTH ATLANTIC", SA },
            { "SYRIA", SY }
        };

        public static TransverseMercator ProjectionFromTheatre(string theatre)
        {
            if (TheatreToProjectionMap.TryGetValue(theatre, out var projection))
            {
                return projection;
            }
            else
            {
                throw new ArgumentException($"TransverseMercator not known for {theatre}");
            }
        }

        public static class Bounds
        {
            public static Dictionary<string, Dictionary<string, Coordinate>> Regions = new Dictionary<string, Dictionary<string, Coordinate>>
            {
                { "CAUCUSES", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = 48.387663480938, Lon = 26.778743595881 } },  // top left
                        { "SW", new Coordinate { Lat = 39.608931903399, Lon = 27.637331401126 } },  // bottom left
                        { "SE", new Coordinate { Lat = 38.86511140611, Lon = 47.142314272867 } },   // bottom right
                        { "NE", new Coordinate { Lat = 47.382221906262, Lon = 49.309787386754 } }   // top right
                    }
                },
                { "MARIANAS", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = 22.220143285088, Lon = 136.96126049266 } },
                        { "SW", new Coordinate { Lat = 10.637681299806, Lon = 137.54638410345 } },
                        { "SE", new Coordinate { Lat = 10.739229846557, Lon = 152.12973515767 } },
                        { "NE", new Coordinate { Lat = 22.44081213808, Lon = 152.4517401234 } }
                    }
                },
                { "NEVADA", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = 39.801712624973, Lon = -119.9902311096 } },
                        { "SW", new Coordinate { Lat = 34.400025213159, Lon = -119.78488669575 } },
                        { "SE", new Coordinate { Lat = 34.346907399159, Lon = -112.44599267994 } },
                        { "NE", new Coordinate { Lat = 39.737162541546, Lon = -112.11118674647 } }
                    }
                },
                { "NORMANDY", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = 51.853053209954, Lon = -3.5005307234326 } },
                        { "SW", new Coordinate { Lat = 48.345555267203, Lon = -3.4652619527823 } },
                        { "SE", new Coordinate { Lat = 48.182820700457, Lon = 3.1296001999935 } },
                        { "NE", new Coordinate { Lat = 51.668977027237, Lon = 3.5903264200692 } }
                    }
                },
                { "PERSIAN GULF", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = 32.955527544002, Lon = 46.583433745255 } },
                        { "SW", new Coordinate { Lat = 21.749230188233, Lon = 47.594358099874 } },
                        { "SE", new Coordinate { Lat = 21.869681127563, Lon = 63.997389263298 } },
                        { "NE", new Coordinate { Lat = 33.150981840679, Lon = 64.756585025318 } }
                    }
                },

                { "SINAI", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = 34.55404881, Lon = 25.448642 } },
                        { "SW", new Coordinate { Lat = 25.06909856, Lon = 25.448642 } },
                        { "SE", new Coordinate { Lat = 25.06909856, Lon = 41.69756169 } },
                        { "NE", new Coordinate { Lat = 34.55404881, Lon = 41.69756169 } }
                    }
                },
                { "SOUTH ATLANTIC", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = -45.850907963742, Lon = -84.733179722768 } },
                        { "SW", new Coordinate { Lat = -53.241290032056, Lon = -89.780310307149 } },
                        { "SE", new Coordinate { Lat = -56.442360340952, Lon = -38.172247338514 } },
                        { "NE", new Coordinate { Lat = -48.278746783249, Lon = -41.444185881767 } }
                    }
                },
                { "SYRIA", new Dictionary<string, Coordinate>
                    {
                        { "NW", new Coordinate { Lat = 37.470301761465, Lon = 29.480123666167 } },
                        { "SW", new Coordinate { Lat = 31.683960285685, Lon = 30.123622480902 } },
                        { "SE", new Coordinate { Lat = 31.960960214436, Lon = 41.932899899137 } },  
                        { "NE", new Coordinate { Lat = 37.814134114831, Lon = 42.148931009427 } }  
                    }
                }
            };

            public static string FindRegion(Coordinate point)
            {

                foreach (var region in Regions)
                {
                    var bounds = region.Value;

                    // Get the coordinates of the bounds
                    var nw = bounds["NW"];
                    var sw = bounds["SW"];
                    var se = bounds["SE"];
                    var ne = bounds["NE"];

                    // Check if the point's latitude is between the north and south bounds,
                    // and its longitude is between the west and east bounds
                    if (point.Lat <= nw.Lat && point.Lat >= sw.Lat &&  // Latitude between NW and SW
                        point.Lon >= nw.Lon && point.Lon <= ne.Lon)    // Longitude between NW and NE
                    {
                        return region.Key; // Point falls within this region
                    }
                }

                return null; // Point does not fall in any region
            }
        }
    }
}
