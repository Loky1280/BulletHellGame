using System.Windows;
using System.Collections.Generic;

namespace General_logic
{
    public class AutomaticWeapon : Weapon
    {
        public override string TextureUriLeft => "pack://application:,,,/Resource/Guns/AutomaticWeapon_L.png";
        public override string TextureUriRight => "pack://application:,,,/Resource/Guns/AutomaticWeapon_R.png";
        public override int TextureWidth => 32;
        public override int TextureHeight => 13;

        private bool isReloading = false;
        private double reloadTimer = 0;

        public AutomaticWeapon()
        {
            ShotSpeed = 300.0;
            Damage = 1;
            MaxAmmo = 30;
            AmmoInWeapon = MaxAmmo;
            ReloadTime = 3.2;
            FireRate = 0.3; 
            Range = 600;
            Recoil = 1.0;
        }

        public override Projectile CreateProjectile(Point from, Vector direction, object owner, out List<Projectile> additionalPellets)
        {
            additionalPellets = new List<Projectile>();
            if (isReloading || FireTimer > 0)
                return null;

            if (double.IsNaN(from.X) || double.IsNaN(from.Y))
                return null;

            if (AmmoInWeapon <= 0)
            {
                StartReload();
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

        public override void Update(double dt)
        {
            base.Update(dt);

            if (isReloading)
            {
                reloadTimer += dt;
                if (reloadTimer >= ReloadTime)
                {
                    isReloading = false;
                    reloadTimer = 0;
                    AmmoInWeapon = MaxAmmo;
                }
            }
        }

        public void StartReload()
        {
            if (!isReloading && AmmoInWeapon < MaxAmmo)
            {
                isReloading = true;
                reloadTimer = 0;
            }
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
