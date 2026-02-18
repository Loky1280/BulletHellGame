using System.Windows;
using System.Collections.Generic;

namespace General_logic
{
    public class Weapon
    {
        public double ShotSpeed { get; set; }

        public int Damage { get; set; }

        public int AmmoInWeapon { get; set; }

        public int MaxAmmo { get; set; }

        public double ReloadTime { get; set; }

        public double FireRate { get; set; }

        protected double Range { get; set; }

        protected double Recoil { get; set; }

        public double FireTimer { get; set; }

        protected double ReloadTimer { get; set; }

        public virtual bool IsReloading { get; set; }
        public bool IsEnemyWeapon { get; set; }

        public virtual string TextureUriLeft => null;
        public virtual string TextureUriRight => null;
        public virtual string TextureUri => TextureUriRight ?? TextureUriLeft;
        public virtual int TextureWidth => 0;
        public virtual int TextureHeight => 0;

        public virtual void Update(double dt)
        {
            if (IsReloading)
            {
                ReloadTimer -= dt;
                if (ReloadTimer <= 0)
                {
                    AmmoInWeapon = MaxAmmo;
                    IsReloading = false;
                }
            }
            else
            {
                FireTimer -= dt;
            }
        }

        public virtual Projectile CreateProjectile(
            Point from,
            Vector direction,
            object owner,
            out List<Projectile> additionalPellets)
        {
            additionalPellets = new List<Projectile>();

            if (!CanShoot())
                return null;

            if (!IsValidPosition(from))
                return null;

            PrepareShot();

            direction = NormalizeDirection(direction);

            return GenerateProjectile(from, direction, owner);
        }

        private bool CanShoot()
        {
            if (IsReloading || FireTimer > 0)
                return false;

            if (AmmoInWeapon <= 0)
            {
                StartReload();
                return false;
            }

            return true;
        }
        private bool IsValidPosition(Point from)
        {
            return !double.IsNaN(from.X) && !double.IsNaN(from.Y);
        }

        private void PrepareShot()
        {
            AmmoInWeapon--;
            FireTimer = FireRate;
        }

        private Vector NormalizeDirection(Vector direction)
        {
            if (direction.Length == 0)
                return new Vector(1, 0);

            direction.Normalize();
            return direction;
        }

        private Projectile GenerateProjectile(Point from, Vector direction, object owner)
        {
            return new Projectile
            {
                Position = from,
                Velocity = direction,
                Speed = ShotSpeed,
                Damage = Damage,
                Owner = owner
            };
        }

        private void StartReload()
        {
            IsReloading = true;
            ReloadTimer = ReloadTime;
        }


        public virtual void Reload()
        {
            if (!IsReloading)
            {
                IsReloading = true;
                ReloadTimer = ReloadTime;
            }
        }
    }
}