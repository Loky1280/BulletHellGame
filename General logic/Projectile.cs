
using System.Windows.Media;
using System.Windows;

namespace General_logic
{
    public class Projectile
    {
        public Point Position { get; set; }
        public Vector Velocity { get; set; }
        public int Damage { get; set; }
        public double Speed { get; set; }
        public object Owner { get; set; } 

        public Rect Bounds => new Rect(Position.X - 4, Position.Y - 4, 8, 8);

        public void Update(double dt)
        {
            var oldPos = Position;
            Position += Velocity * Speed * dt;
        }

        public void Draw(DrawingContext dc, Point camera)
        {
            
        }
    }

}
