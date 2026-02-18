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

        public virtual Projectile CreateProjectile(Point from, Vector direction, object owner, out List<Projectile> additionalPellets)
        {
            additionalPellets = new List<Projectile>();
            if (IsReloading || FireTimer > 0)
                return null;

            if (double.IsNaN(from.X) || double.IsNaN(from.Y))
                return null;

            if (AmmoInWeapon <= 0)
            {
                IsReloading = true;
                ReloadTimer = ReloadTime;
                return null;
            }

            AmmoInWeapon--;
            FireTimer = FireRate;

            if (direction.Length == 0)
            {
                direction = new Vector(1, 0);
            }
            else
            {
                direction.Normalize();
            }

            var projectile = new Projectile
            {
                Position = from,
                Velocity = direction,
                Speed = ShotSpeed,
                Damage = Damage,
                Owner = owner
            };

            return projectile;
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