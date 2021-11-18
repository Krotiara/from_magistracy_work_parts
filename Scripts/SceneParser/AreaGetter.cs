using System.IO;

namespace CableWalker.Simulator.SceneParser
{
    public class AreaGetter
    {
        public static string GetAreaByGPS(string pathToTowerConfig)
        {
            var minLat = float.MaxValue;
            var minLon = float.MaxValue;
            var maxLat = float.MinValue;
            var maxLon = float.MinValue;
            //Привязано к текущему виду конфига
            using (var parser = new StreamReader(Path.Combine(pathToTowerConfig), encoding: System.Text.Encoding.GetEncoding(1251)))
            {
                parser.ReadLine();
                while (!parser.EndOfStream)
                {
                    var line = parser.ReadLine().Split(';');
                    var latitude = float.Parse(line[2]);
                    var longitude = float.Parse(line[3]);
                    if (latitude < minLat)
                        minLat = latitude;
                    if (latitude > maxLat)
                        maxLat = latitude;
                    if (longitude < minLon)
                        minLon = longitude;
                    if (longitude > maxLon)
                        maxLon = longitude;
                }
            }
            var point1 = $"pointMinMin:({minLat},{minLon})";
            var point2 = $"pointMinMax:({minLat},{maxLon})";
            var point3 = $"pointMaxMin:({maxLat},{minLon})";
            var point4 = $"pointMaxMax:({maxLat},{maxLon})";
            return $"Area = [{point1}; {point2}; {point3}; {point4}], point = (latitude, longitude)";
        }
    }
}
