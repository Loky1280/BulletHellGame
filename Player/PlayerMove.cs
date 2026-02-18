using System.Windows;
using Enemies;
using System.Windows.Input;
using General_logic;

namespace Player
{
    public class PlayerMove
    {
        public Character Character { get; set; }

        public double X { get; private set; }
        public double Y { get; private set; }
        private double Speed { get; } = 6.5;

        private bool MoveUp;
        private bool MoveDown;
        private bool MoveLeft;
        private bool MoveRight;

        public bool isDashing = false;
        private double dashTimer = 0;
        private double dashCooldownTimer = 0;
        private Vector dashDirection = new Vector(0, 0);
        private const double DashDuration = 0.18;
        private const double DashDistance = 250; 
        private const double DashCooldown = 1.2;
        public bool IsImmune { get; private set; } = false;
        private double DashSpeed = 0; 


        public System.Windows.Vector Velocity { get; set; }

        public PlayerMove(double startX, double startY)
        {
            X = startX;
            Y = startY;
        }

        public void OnKeyDown(Key key)
        {
            if (key == Key.W) MoveUp = true;
            if (key == Key.S) MoveDown = true;
            if (key == Key.A) MoveLeft = true;
            if (key == Key.D) MoveRight = true;
            if (key == Key.R) Reload();
            if (key == Key.Space) Dash();
        }

        public void OnKeyUp(Key key)
        {
            if (key == Key.W) MoveUp = false;
            if (key == Key.S) MoveDown = false;
            if (key == Key.A) MoveLeft = false;
            if (key == Key.D) MoveRight = false;
        }
        public void Update(List<Rect> allowedAreas, List<Enemy> enemies = null, double dt = 1.0)
        {
            double nextX = X;
            double nextY = Y;

            if (dashCooldownTimer > 0) dashCooldownTimer -= dt;

            if (isDashing)
            {
                nextX += dashDirection.X * DashSpeed * dt;
                nextY += dashDirection.Y * DashSpeed * dt;
                dashTimer -= dt;
                IsImmune = true;
                if (dashTimer <= 0)
                {
                    isDashing = false;
                    IsImmune = false;
                }
            }
            else
            {
                IsImmune = false;
                if (MoveUp) nextY -= Speed;
                if (MoveDown) nextY += Speed;
                if (MoveLeft) nextX -= Speed;
                if (MoveRight) nextX += Speed;
            }

            Rect nextRect = new Rect(nextX, nextY, 30, 30);

            bool isInsideAllowedArea = false;

            foreach (var area in allowedAreas)
            {
                if (area.IntersectsWith(nextRect))
                {
                    isInsideAllowedArea = true;
                    break;
                }
            }

            bool collidesWithEnemy = false;
            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy.IsAlive && nextRect.IntersectsWith(enemy.Bounds))
                    {
                        collidesWithEnemy = true;
                        break;
                    }
                }
            }

            if (isInsideAllowedArea && !collidesWithEnemy)
            {
                X = nextX;
                Y = nextY;
            }

        }
        public Projectile Shoot(Point target, out List<Projectile> additionalPellets)
        {
            additionalPellets = new List<Projectile>();
            if (Character?.Gun == null) return null;

            Vector direction = target - new Point(X, Y);
            direction.Normalize();

            var projectile = Character.Gun.CreateProjectile(new Point(X, Y), direction, this, out additionalPellets);
            return projectile;
        }

        public void Reload()
        {
            if (Character.Gun is Pistol pistol)
                pistol.Reload();
            if (Character.Gun is AutomaticWeapon automatic)
                automatic.Reload();
        }

        public void Dash()
        {
            if (isDashing || dashCooldownTimer > 0) return;

            dashDirection = new Vector(0, 0);
            if (MoveUp) dashDirection.Y -= 1;
            if (MoveDown) dashDirection.Y += 1;
            if (MoveLeft) dashDirection.X -= 1;
            if (MoveRight) dashDirection.X += 1;
            if (dashDirection.Length == 0) dashDirection.Y = -1; 

            dashDirection.Normalize();
            DashSpeed = DashDistance / DashDuration;
            isDashing = true;
            dashTimer = DashDuration;
            dashCooldownTimer = DashCooldown;
        }
    }
}
