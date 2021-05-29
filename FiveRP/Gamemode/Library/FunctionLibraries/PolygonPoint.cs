using System.Collections.Generic;

namespace FiveRP.Gamemode.Library.FunctionLibraries
{
    public static class PolygonPoint
    {
        private static bool IsPointInPolygon2D(List<Point> polygon, Point point)
        {
            var isInside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }

    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}