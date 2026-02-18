using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace General_logic
{
    public class Projectile
    {
        public Point Position { get; set; }
        public Vector Velocity { get; set; }
        public double Speed { get; set; }
        public int Damage { get; set; }
        public object Owner { get; set; }
        public bool IsActive { get; set; } = true;

        public Shape GetShape()
        {
            return new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Red,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };
        }

        public void Update(double dt)
        {
            if (!IsActive) return;

            Position += Velocity * Speed * dt;
        }

        public bool IsOutOfBounds(double width, double height)
        {
            return Position.X < 0 || Position.X > width || Position.Y < 0 || Position.Y > height;
        }
    }
} 