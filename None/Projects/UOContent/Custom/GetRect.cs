using System;
using System.Linq;
using Server;
using Server.Commands;
using Server.Items;
using Server.Targeting;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Server.Mobiles;
using Server.Network;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;


namespace Server.Custom
{
    public class GetRectCommand
    {
        private static List<int> RoofTileIds = new List<int> { 0x519, 0x51A, 0x51B, 0x51C, 0x51D, 0x51E, 0x51F, 0x520, 0x521, 0x522, 0x523, 0x524 };

        public class BoundingBox
        {
            [JsonPropertyName("$type")]
            public string Type { get; set; }

            [JsonPropertyName("Map")]
            public string Map { get; set; }

            [JsonPropertyName("Name")]
            public string Name { get; set; }

            [JsonPropertyName("Priority")]
            public int? Priority { get; set; } // Add Priority property

            [JsonPropertyName("Entrance")]
            public Point3D? Entrance { get; set; } // Add Entrance property

            [JsonPropertyName("GoLocation")]
            public Point3D? GoLocation { get; set; } // Add GoLocation property

            [JsonPropertyName("Area")]
            public List<Rectangle2D> Area { get; set; }

            [JsonPropertyName("Music")]
            public string Music { get; set; } // Add Music property
        }


        private static Point2D ParseCoordinatesFromLine(string line)
        {
            var coordinatesPattern = @"\((\d+),\s*(\d+)\)";
            var match = Regex.Match(line, coordinatesPattern);

            if (match.Success && match.Groups.Count == 3)
            {
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                return new Point2D(x, y);
            }

            return Point2D.Zero;
        }


        public static void Initialize()
        {
            CommandSystem.Register("getrect", AccessLevel.Player, new CommandEventHandler(GetRect_OnCommand));
        }

        [Usage("getrect")]
        [Description("Reads input coordinates from a file, calculates building bounding boxes, and outputs the results to a JSON file.")]
        private static void GetRect_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            string inputFilePath = "Data\\City_Shops.txt";
            string outputFilePath = "Data\\City_ShopsRegions.json";

            List<BoundingBox> boundingBoxes = new List<BoundingBox>();

            // Read input data
            List<(Point2D Entrance, string Type, string Name, string Map)> inputCoordinatesData = ReadInputCoordinatesFile(inputFilePath);

            foreach ((Point2D Entrance, string Type, string Name, string Map) inputData in inputCoordinatesData)
            {
                Map map = from.Map;

                // Convert Point2D to Point3D with an appropriate Z-value (e.g., 0)
                Point3D location = new Point3D(inputData.Entrance.X, inputData.Entrance.Y, 0);

                Dictionary<Point2D, List<StaticTile>> tiles = GetRoofTiles(map, location);
                List<Rectangle2D> buildingRectangles = GetRects(tiles, map);

                boundingBoxes.Add(new BoundingBox { Type = inputData.Type, Entrance = null, GoLocation = location, Map = inputData.Map, Name = inputData.Name, Area = buildingRectangles });
            }

            WriteBoundingBoxesToJsonFile(boundingBoxes, outputFilePath);
            from.SendMessage($"Processed {inputCoordinatesData.Count} input coordinates and wrote the results to {outputFilePath}");
        }

        private static List<(Point2D Entrance, string Type, string Name, string Map)> ReadInputCoordinatesFile(string path)
        {
            var inputCoordinates = new List<(Point2D Entrance, string Type, string Name, string Map)>();

            string currentRegion = "";
            bool isFelucca = false;
            bool isTrammel = false;

            using (StreamReader sr = File.OpenText(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#Region Name "))
                    {
                        currentRegion = line.Substring(13);
                    }
                    else if (line.StartsWith("#Facet "))
                    {
                        isFelucca = line.Contains("Felucca");
                        isTrammel = line.Contains("Trammel");
                    }
                    else if (Regex.IsMatch(line, @"\w+: "))
                    {
                        var match = Regex.Match(line, @"(\w+): (.+): \((\d+), (\d+)\)");

                        if (match.Success)
                        {
                            string type = match.Groups[1].Value;
                            string name = match.Groups[2].Value;
                            int x = int.Parse(match.Groups[3].Value);
                            int y = int.Parse(match.Groups[4].Value);
                            Point2D point = new Point2D(x, y);

                            if (isFelucca)
                            {
                                inputCoordinates.Add((Entrance: point, Type: type, Name: name, Map: "Felucca"));
                            }

                            if (isTrammel)
                            {
                                inputCoordinates.Add((Entrance: point, Type: type, Name: name, Map: "Trammel"));
                            }
                        }
                    }
                }
            }

            return inputCoordinates;
        }

        private static string SerializeBoundingBoxes(List<BoundingBox> boundingBoxes)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                //IgnoreNullValues = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            string json = JsonSerializer.Serialize(boundingBoxes, options);

            // Adjust the output string to match the desired format
            json = json.Replace("\r\n      ", "\r\n");
            json = json.Replace("\r\n        {\r\n          ", " {");
            json = json.Replace("\r\n        }", " }");
            json = json.Replace("\r\n      ],", "],");

            return json;
        }

        public class Rectangle2DJsonConverter : JsonConverter<Rectangle2D>
        {
            public override Rectangle2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, Rectangle2D value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("x1", value.X);
                writer.WriteNumber("y1", value.Y);
                writer.WriteNumber("x2", value.X + value.Width - 1);
                writer.WriteNumber("y2", value.Y + value.Height - 1);
                writer.WriteEndObject();
            }
        }

        private static void WriteBoundingBoxesToJsonFile(List<BoundingBox> boundingBoxes, string outputFilePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new Rectangle2DJsonConverter() } // Register the custom JSON converter
            };
            string json = JsonSerializer.Serialize(boundingBoxes, options);
            File.WriteAllText(outputFilePath, json);


        }


        private class GetRectTarget : Target
        {
            public GetRectTarget() : base(12, false, TargetFlags.None) { }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile mobile && mobile == from)
                {
                    Map map = from.Map;
                    Point3D startPoint = from.Location;

                    // Calculate building rectangle based on roof tiles
                    Dictionary<Point2D, List<StaticTile>> tiles = GetRoofTiles(from.Map, from.Location);
                    List<Rectangle2D> buildingRectangles = GetRects(tiles, map);

                    int rectIndex = 1;
                    foreach (Rectangle2D buildingRectangle in buildingRectangles)
                    {
                        from.SendMessage($"Rectangle {rectIndex} of the building: ({buildingRectangle.X}, {buildingRectangle.Y}), ({buildingRectangle.X + buildingRectangle.Width - 1}, {buildingRectangle.Y}), ({buildingRectangle.X}, {buildingRectangle.Y + buildingRectangle.Height - 1}), ({buildingRectangle.X + buildingRectangle.Width - 1}, {buildingRectangle.Y + buildingRectangle.Height - 1})");
                        rectIndex++;
                    }
                }
                else
                {
                    from.SendMessage("You must target yourself.");
                }
            }

        }

        private static Dictionary<Point2D, List<StaticTile>> GetRoofTiles(Map map, Point3D playerLocation)
        {
            int range = 22; // You can adjust the range as needed
            Dictionary<Point2D, List<StaticTile>> roofTiles = new Dictionary<Point2D, List<StaticTile>>();

            for (int x = playerLocation.X - range; x <= playerLocation.X + range; ++x)
            {
                for (int y = playerLocation.Y - range; y <= playerLocation.Y + range; ++y)
                {
                    var tiles = map.Tiles.GetStaticTiles(x, y, true);
                    if (tiles.Length > 0)
                    {
                        var point = new Point2D(x, y);
                        var tileList = new List<StaticTile>();

                        foreach (var tile in tiles)
                        {
                            var itemData = TileData.ItemTable[tile.ID & TileData.MaxItemValue];
                            bool isRoofTile = (itemData.Flags & TileFlag.Roof) != 0;
                            bool isSurfaceTile = (itemData.Flags & TileFlag.Surface) != 0 && tile.Z > 65;

                            if (isRoofTile || isSurfaceTile)
                            {
                                tileList.Add(tile);
                            }
                        }

                        if (tileList.Count > 0)
                        {
                            roofTiles.Add(point, tileList);
                        }
                    }
                }
            }

            return roofTiles;
        }

        private static List<Point2D> GetAdjacentTiles(Point2D point, Map map)
        {
            List<Point2D> neighbors = new List<Point2D>();

            for (int x = point.X - 1; x <= point.X + 1; ++x)
            {
                for (int y = point.Y - 1; y <= point.Y + 1; ++y)
                {
                    if (x == point.X && y == point.Y)
                    {
                        continue;
                    }

                    // Check for wall tiles and exclude them from the list of neighbors
                    StaticTile[] staticTiles = map.Tiles.GetStaticTiles(x, y, true);
                    bool isWallTile = false;

                    foreach (var tile in staticTiles)
                    {
                        ItemData itemData = TileData.ItemTable[tile.ID & TileData.MaxItemValue];
                        if ((itemData.Flags & TileFlag.Wall) != 0)
                        {
                            isWallTile = true;
                            break;
                        }
                    }

                    if (!isWallTile)
                    {
                        neighbors.Add(new Point2D(x, y));
                    }
                }
            }

            return neighbors;
        }


        private static List<Rectangle2D> GetRects(Dictionary<Point2D, List<StaticTile>> tiles, Map map)
        {
            var rects = new List<Rectangle2D>();

            while (tiles.Count > 0)
            {
                var seed = tiles.Keys.First();
                var q = new Queue<Point2D>();
                var min = seed;
                var max = seed;

                q.Enqueue(seed);
                tiles.Remove(seed);

                while (q.Count > 0)
                {
                    var p = q.Dequeue();

                    if (p.X < min.X) min.X = p.X;
                    if (p.Y < min.Y) min.Y = p.Y;
                    if (p.X > max.X) max.X = p.X;
                    if (p.Y > max.Y) max.Y = p.Y;

                    for (var x = p.X - 1; x <= p.X + 1; ++x)
                    {
                        for (var y = p.Y - 1; y <= p.Y + 1; ++y)
                        {
                            var neighbor = new Point2D(x, y);

                            if (tiles.ContainsKey(neighbor))
                            {
                                q.Enqueue(neighbor);
                                tiles.Remove(neighbor);
                            }
                        }
                    }
                }

                rects.Add(new Rectangle2D(min, max));
            }

            return rects;
        }

    }
}
