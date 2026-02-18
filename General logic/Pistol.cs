using System.Windows;
using System.Collections.Generic;

namespace General_logic
{
    public class Pistol : Weapon
    {
        public override string TextureUriLeft => "pack://application:,,,/Resource/Guns/Pistol_L.png";
        public override string TextureUriRight => "pack://application:,,,/Resource/Guns/Pistol_R.png";
        public override int TextureWidth => 16;
        public override int TextureHeight => 10;

        public Pistol()
        {
            ShotSpeed = 220;
            Damage = 1;
            AmmoInWeapon = 4;
            MaxAmmo = 4;
            ReloadTime = 1.5;
            FireRate = 0.8;
            Range = 300;
            Recoil = 0;
            FireTimer = 0;
            ReloadTimer = 3;
            IsReloading = false;
        }

        public override void Update(double dt)
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
                if (IsEnemyWeapon && AmmoInWeapon <= 0)
                {
                    IsReloading = true;
                    ReloadTimer = ReloadTime;
                }
            }
        }

        public override Projectile CreateProjectile(Point from, Vector direction, object owner, out List<Projectile> additionalPellets)
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

            System.Diagnostics.Debug.WriteLine($"[AMMOINWEAPON]: {AmmoInWeapon}");
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

        public void Reload()
        {
            if (!IsReloading)
            {
                IsReloading = true;
                ReloadTimer = ReloadTime;
            }
        }
    }
}