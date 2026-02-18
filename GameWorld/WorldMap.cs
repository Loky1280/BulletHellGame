using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameWorld
{

    public class WorldMap
    {
        private const double Dpi = 96.0;
        private const int CorridorTileSize = 64;
        private const int FloorTileSize = 128;
        private const int WallTileSize = 128;
        private const double WallThickness = 15;

        public BitmapSource MapBitmap { get; private set; }
        public Rect WorldBounds { get; private set; }
        public BitmapSource MiniMapBitmap { get; private set; }
        public Point MiniMapCenter { get; private set; }

        public void BuildFromLevel(
            List<Room> rooms,
            List<Corridor> corridors,
            string floorTileUri,
            string wallTileUri,
            string corridorTileUri)
        {
            if (rooms == null || rooms.Count == 0)
            {
                WorldBounds = new Rect(0, 0, 0, 0);
                MapBitmap = null;
                return;
            }

            Rect worldRect = ComputeWorldBounds(rooms, corridors);
            int w = (int)Math.Ceiling(worldRect.Width);
            int h = (int)Math.Ceiling(worldRect.Height);
            if (w <= 0 || h <= 0)
            {
                WorldBounds = worldRect;
                MapBitmap = null;
                return;
            }

            var floorBitmap = new BitmapImage(new Uri(floorTileUri));
            var wallBitmap = new BitmapImage(new Uri(wallTileUri));
            var corridorBitmap = new BitmapImage(new Uri(corridorTileUri));

            var rtb = new RenderTargetBitmap(w, h, Dpi, Dpi, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            double ox = worldRect.X;
            double oy = worldRect.Y;

            using (DrawingContext dc = dv.RenderOpen())
            {
                foreach (var corridor in corridors)
                {
                    foreach (var segment in corridor.Segments)
                    {
                        bool isHorizontal = segment.Width >= segment.Height;
                        if (isHorizontal)
                        {
                            double y = segment.Top + (segment.Height - CorridorTileSize) / 2;
                            for (double x = segment.Left; x < segment.Right; x += CorridorTileSize)
                            {
                                double tw = Math.Min(CorridorTileSize, segment.Right - x);
                                double th = CorridorTileSize;
                                dc.DrawImage(corridorBitmap, new Rect(x - ox, y - oy, tw, th));
                            }
                        }
                        else
                        {
                            double x = segment.Left + (segment.Width - CorridorTileSize) / 2;
                            for (double y = segment.Top; y < segment.Bottom; y += CorridorTileSize)
                            {
                                double tw = CorridorTileSize;
                                double th = Math.Min(CorridorTileSize, segment.Bottom - y);
                                dc.DrawImage(corridorBitmap, new Rect(x - ox, y - oy, tw, th));
                            }
                        }
                    }
                }

                foreach (var room in rooms)
                {
                    int tilesX = (int)Math.Ceiling((double)room.Width / FloorTileSize);
                    int tilesY = (int)Math.Ceiling((double)room.Height / FloorTileSize);
                    for (int tx = 0; tx < tilesX; tx++)
                    {
                        for (int ty = 0; ty < tilesY; ty++)
                        {
                            double px = room.X + tx * FloorTileSize - ox;
                            double py = room.Y + ty * FloorTileSize - oy;
                            dc.DrawImage(floorBitmap, new Rect(px, py, FloorTileSize, FloorTileSize));
                        }
                    }
                }
            }

            rtb.Render(dv);
            MapBitmap = rtb;
            WorldBounds = worldRect;
        }

        private static Rect ComputeWorldBounds(List<Room> rooms, List<Corridor> corridors)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var room in rooms)
            {
                minX = Math.Min(minX, room.X);
                minY = Math.Min(minY, room.Y);
                maxX = Math.Max(maxX, room.X + room.Width);
                maxY = Math.Max(maxY, room.Y + room.Height);
            }

            if (corridors != null)
            {
                foreach (var corridor in corridors)
                {
                    foreach (var seg in corridor.Segments)
                    {
                        minX = Math.Min(minX, seg.Left);
                        minY = Math.Min(minY, seg.Top);
                        maxX = Math.Max(maxX, seg.Right);
                        maxY = Math.Max(maxY, seg.Bottom);
                    }
                }
            }

            if (minX == double.MaxValue) return new Rect(0, 0, 0, 0);
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }


        public void BuildMiniMap(List<Room> rooms, List<Corridor> corridors, Point worldCenter, int miniMapSize = 230, double scale = 0.03)
        {
            if (rooms == null || rooms.Count == 0)
            {
                MiniMapBitmap = null;
                MiniMapCenter = worldCenter;
                return;
            }

            var rtb = new RenderTargetBitmap(miniMapSize, miniMapSize, Dpi, Dpi, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            double centerX = miniMapSize / 2.0;
            double centerY = miniMapSize / 2.0;

            using (DrawingContext dc = dv.RenderOpen())
            {
                foreach (var corridor in corridors)
                {
                    foreach (var segment in corridor.Segments)
                    {
                        double offsetX = (segment.X + segment.Width / 2 - worldCenter.X) * scale + centerX;
                        double offsetY = (segment.Y + segment.Height / 2 - worldCenter.Y) * scale + centerY;
                        double width = segment.Width * scale;
                        double height = segment.Height * scale;

                        dc.DrawRectangle(Brushes.DarkGray, null, 
                            new Rect(offsetX - width / 2, offsetY - height / 2, width, height));
                    }
                }

                foreach (var room in rooms)
                {
                    Brush fillBrush;
                    if (room.IsBossRoom)
                        fillBrush = Brushes.DarkRed;
                    else if (room.IsTreasureRoom)
                        fillBrush = Brushes.Yellow;
                    else if (room.IsShopRoom)
                        fillBrush = Brushes.DarkGoldenrod;
                    else
                        fillBrush = Brushes.Gray;

                    double offsetX = (room.X + room.Width / 2 - worldCenter.X) * scale + centerX;
                    double offsetY = (room.Y + room.Height / 2 - worldCenter.Y) * scale + centerY;
                    double width = room.Width * scale;
                    double height = room.Height * scale;

                    dc.DrawRectangle(fillBrush, new Pen(Brushes.White, 1),
                        new Rect(offsetX - width / 2, offsetY - height / 2, width, height));
                }
            }

            rtb.Render(dv);
            MiniMapBitmap = rtb;
            MiniMapCenter = worldCenter;
        }

    }
}
