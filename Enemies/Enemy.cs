using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using General_logic;

namespace Enemies
{
    public class Enemy
    {
        public Point Position { get; set; }
        protected Vector Direction { get; set; }

        protected int _currentFrame;
        protected double _animTimer;
        protected double _animTimerLength = 0.1;

        protected int Health { get; set; }
        protected int MaxHealth { get; set; }

        protected double MoveSpeed { get; set; }

        public Weapon Gun { get; set; }
 
        public Rect Bounds => new(Position.X - 15, Position.Y - 15, 30, 30);
 
        protected int Damage { get; set; }
  
        protected double AttackCooldown { get; set; }
        protected double AttackTimer { get; set; }
 
        public bool IsAlive { get; set; }
  
        protected bool IsBoss { get; set; }
        protected double AttackRadius { get; set; }
        protected double CloseRange { get; set; }
        protected double NormalAttackCooldown { get; set; }
        protected double FastAttackCooldown { get; set; } 

        public event Action OnEnemyKilled;

        public Enemy(Point position, Weapon weapon)
        {
            Position = position;
            Gun = weapon;
            Health = 100;
            MaxHealth = 100;
            MoveSpeed = 100;
            Damage = 10;
            AttackCooldown = 1.0;
            AttackTimer = 0;
            IsAlive = true;
            AttackRadius = 200;
            CloseRange = 50;
            NormalAttackCooldown = 1.0;
            FastAttackCooldown = 0.5;
        }

        public virtual Shape GetShape()
        {
            return new Rectangle
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.Red,
                Stroke = null
            };
        }

        protected virtual BitmapSource[] GetAnimationFrames() => null;

        public BitmapSource GetCurrentFrameBitmap()
        {
            var f = GetAnimationFrames();
            if (f == null || f.Length == 0) return null;
            return f[_currentFrame % f.Length];
        }

        public virtual void UpdateAnimation(double dt)
        {
            var f = GetAnimationFrames();
            if (f == null || f.Length == 0) return;
            _animTimer -= dt;
            if (_animTimer <= 0)
            {
                _animTimer = _animTimerLength;
                _currentFrame = (_currentFrame + 1) % f.Length;
            }
        }

        public virtual int GetAnimationFrameWidth() => 0;
        public virtual int GetAnimationFrameHeight() => 0;

        public Projectile Shoot(Point target, out List<Projectile> additionalPellets)
        {
            additionalPellets = new List<Projectile>();
            if (Gun == null) return null;

            Vector direction = target - Position;
            direction.Normalize();

            var projectile = Gun.CreateProjectile(Position, direction, this, out additionalPellets);
            return projectile;
        }

        public virtual void UpdateAI(Point playerPos, bool playerInRoom, double dt, List<Projectile> projectiles, List<Enemy> allEnemiesInRoom, double peaceFulTime)
        {
            if (!playerInRoom || !IsAlive)
                return;

            if (Gun == null)
                return;

            Gun.Update(dt);

            double dist = (playerPos - Position).Length;
            double attackRange = AttackRadius;
            double bufferZone = 30;

            if (dist <= attackRange - bufferZone)
            {
                Direction = new Vector(0, 0);
                AttackCooldown = FastAttackCooldown;
            }
            else if (dist <= attackRange)
            {
                Direction = new Vector(0, 0);
                AttackCooldown = NormalAttackCooldown;
            }
            else
            {
                Vector toPlayer = playerPos - Position;
                toPlayer.Normalize();
                Direction = toPlayer;
                Vector nextStep = Direction * MoveSpeed * dt;
                Point nextPos = Position + nextStep;

                Rect enemyRect = new Rect(nextPos.X - 15, nextPos.Y - 15, 30, 30);
                Rect playerRect = new Rect(playerPos.X - 15, playerPos.Y - 15, 30, 30);

                bool canMove = true;
                if (enemyRect.IntersectsWith(playerRect))
                {
                    canMove = false;
                }
                else if (allEnemiesInRoom != null)
                {
                    foreach (var other in allEnemiesInRoom)
                    {
                        if (other == this || !other.IsAlive) continue;
                        if (enemyRect.IntersectsWith(other.Bounds))
                        {
                            canMove = false;
                            break;
                        }
                    }
                }

                if (canMove)
                {
                    Position = nextPos;
                }
                AttackCooldown = NormalAttackCooldown;
            }

            if (peaceFulTime > 0)
                return;

            AttackTimer -= dt;
            if (AttackTimer <= 0)
            {
                Vector direction = playerPos - Position;
                if (direction.Length > 0.01)
                {
                    direction.Normalize();
                    List<Projectile> additionalPellets;
                    var projectile = Gun.CreateProjectile(Position, direction, this, out additionalPellets);
                    if (projectile != null)
                    {
                        projectiles.Add(projectile);
                        projectiles.AddRange(additionalPellets);
                    }
                }
                AttackTimer = AttackCooldown;
            }
        }

        public virtual void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                IsAlive = false;
                OnEnemyKilled?.Invoke();
            }
        }
        
    }
}
