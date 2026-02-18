using System.Windows;

namespace General_logic
{
    public class Pistol : Weapon
    {
        public Pistol()
        {
            ShotSpeed = 500;  // Швидкість польоту кулі
            Damage = 1;       // Урон від пострілу
            MaxAmmo = 4;       // Максимальна кількість патронів
            AmmoInWeapon = 4;  // Початкова кількість патронів
            ReloadTime = 1.0;  // Час перезарядки в секундах
            FireRate = 0.5;    // Час між пострілами в секундах
            Range = 0;         // Дальність стрільби
            Recoil = 0;        // Віддача
            FireTimer = 0;     // Початковий стан таймера стрільби
            ReloadTimer = 0;   // Початковий стан таймера перезарядки
            IsReloading = false; // Початковий стан перезарядки
            IsEnemyWeapon = false; // За замовчуванням - зброя гравця
        }

        public void Reload()
        {
            if (!IsReloading)
            {
                IsReloading = true;
                ReloadTimer = ReloadTime;
            }
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
                // Додаємо перезарядку для ворогів, коли закінчуються патрони
                if (AmmoInWeapon <= 0)
                {
                    Reload();
                }
            }
        }

        public override Projectile CreateProjectile(Point from, Vector direction, object owner)
        {
            if (IsReloading || FireTimer > 0)
                return null;

            if (AmmoInWeapon <= 0)
            {
                Reload();
                return null;
            }

            AmmoInWeapon--;
            FireTimer = FireRate;
            direction.Normalize();

            return new Projectile
            {
                Position = from,
                Velocity = direction,
                Speed = ShotSpeed,
                Damage = Damage,
                Owner = owner
            };
        }
    }
} 