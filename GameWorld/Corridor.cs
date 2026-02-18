using System;
using System.Collections.Generic;
using System.Windows;

namespace GameWorld
{
    public class Corridor
    {
        public Room RoomA { get; }
        public Room RoomB { get; }
        public List<Rect> Segments { get; } = new List<Rect>();

        public Corridor(Room from, Room to, double thickness = 40)
        {
            RoomA = from;
            RoomB = to;

            var start = from.GetCenter();
            var end = to.GetCenter();

            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            bool primaryIsHorizontal = Math.Abs(dx) > Math.Abs(dy);

            Point p1, p2;

            if (primaryIsHorizontal)
            {
                p1 = new Point(start.X + dx / 2, start.Y);
                Segments.Add(MakeSegment(start, p1, thickness));

                p2 = new Point(p1.X, end.Y);
                Segments.Add(MakeSegment(p1, p2, thickness));

                Segments.Add(MakeSegment(p2, end, thickness));
            }
            else
            {
                p1 = new Point(start.X, start.Y + dy / 2);
                Segments.Add(MakeSegment(start, p1, thickness));

                p2 = new Point(end.X, p1.Y);
                Segments.Add(MakeSegment(p1, p2, thickness));

                Segments.Add(MakeSegment(p2, end, thickness));
            }
        }

        private Rect MakeSegment(Point from, Point to, double thickness)
        {
            if (Math.Abs(from.X - to.X) > 0.1)
            {
                double x = Math.Min(from.X, to.X);
                return new Rect(x, from.Y - thickness / 2, Math.Abs(from.X - to.X), thickness);
            }
            else
            {
                double y = Math.Min(from.Y, to.Y);
                return new Rect(from.X - thickness / 2, y, thickness, Math.Abs(from.Y - to.Y));
            }
        }
    }
}
