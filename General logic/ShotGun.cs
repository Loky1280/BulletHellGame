using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace General_logic
{
    public class ShotGun : Weapon
    {
        public override string TextureUriLeft => "pack://application:,,,/Resource/Guns/ShotGun_L.png";
        public override string TextureUriRight => "pack://application:,,,/Resource/Guns/ShotGun_R.png";
        public override int TextureWidth => 32;
        public override int TextureHeight => 12;

        private const int PELLETS_COUNT = 4; 
        private const double SPREAD_ANGLE = 50.0; 

        public ShotGun()
        {
            ShotSpeed = 190.0;
            Damage = 1; 
            MaxAmmo = 3;
            AmmoInWeapon = MaxAmmo;
            ReloadTime = 2.3;
            FireRate = 1.1;
            Range = 300;
            Recoil = 1.0;
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

            List<Projectile> pellets = new List<Projectile>();

            for (int i = 0; i < PELLETS_COUNT; i++)
            {
                double angle = (i - (PELLETS_COUNT - 1) / 2.0) * (SPREAD_ANGLE / (PELLETS_COUNT - 1));
                double angleRad = angle * System.Math.PI / 180.0;

                Vector rotatedDirection = new Vector(
                    direction.X * System.Math.Cos(angleRad) - direction.Y * System.Math.Sin(angleRad),
                    direction.X * System.Math.Sin(angleRad) + direction.Y * System.Math.Cos(angleRad)
                );

                var pellet = new Projectile
                {
                    Position = from,
                    Velocity = rotatedDirection,
                    Speed = ShotSpeed,
                    Damage = Damage,
                    Owner = owner
                };

                pellets.Add(pellet);
            }

            additionalPellets.AddRange(pellets.Skip(1));

            return pellets[0];
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
