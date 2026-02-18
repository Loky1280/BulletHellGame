using Enemies;
using System.Windows;

namespace GameWorld
{
    public class Room
    {

        public const double WallThickness = 15;

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Rect Bounds => new Rect(X, Y, Width, Height);

        public Rect WalkableBounds => new Rect(
            X + WallThickness,
            Y + WallThickness,
            Width - 2 * WallThickness,
            Height - 2 * WallThickness);
        public List<Enemy> enemies { get; set; } = new List<Enemy>();
        public bool IsConnectToMainRoom { get; set; }
        public bool NegativeXConnect {  get; set; }
        public bool PositiveXConnect { get; set; }
        public bool NegativeYConnect { get; set; }
        public bool PositiveYConnect {  get; set; }
        public bool IsBossRoom { get; set; }
        public bool IsTreasureRoom { get; set; }
        public bool IsShopRoom { get; set; }
        public bool IsBattleBegin {  get; set; }
        public bool IsVisited { get; set; }
        public double PeaceFulTime { get; set; }
        
        public Room()
        {
            enemies = new List<Enemy>();
        }

        public List<Rect> GetWalls(double thickness = 15)
        {
            return new List<Rect>
        {
            new Rect(X, Y, Width, thickness),                           
            new Rect(X, Y + Height - thickness, Width, thickness),     
            new Rect(X, Y, thickness, Height),                         
            new Rect(X + Width - thickness, Y, thickness, Height)      
        };
        }
        public Point GetCenter()
        {
            return new Point(X + Width / 2, Y + Height / 2);
        }
        public Room FindNearestRoom(List<Room> allRooms)
        {
            Room nearest = null;
            double minDistance = double.MaxValue;

            foreach (var room in allRooms)
            {
                if (room == this)
                    continue;

                double dx = Math.Abs(this.X - room.X);
                double dy = Math.Abs(this.Y - room.Y);

                double distance = dx + dy; 

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = room;
                }
            }

            return nearest;
        }

    }
}
