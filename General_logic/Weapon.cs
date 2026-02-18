using System;
using System.Drawing;
using System.Windows;

namespace General_logic
{
    public class Weapon
    {
        // Швидкість польоту снаряда
        protected double ShotSpeed { get; set; }

        // Урон від одного пострілу
        protected int Damage { get; set; }

        // Кількість патронів у магазині (або нескінченність, якщо не використовується)
        protected int AmmoInWeapon { get; set; }

        // Максимальна кількість патронів у магазині
        protected int MaxAmmo { get; set; }

        // Час перезарядки (секунди)
        protected double ReloadTime { get; set; }

        // Час між пострілами (секунди)
        protected double FireRate { get; set; }

        // Відстань, на яку летить снаряд (або 0 — без обмеження)
        protected double Range { get; set; }

        // Віддача (можна використати для ефекту)
        protected double Recoil { get; set; }

        // Час до наступного пострілу
        protected double FireTimer { get; set; }

        // Час до кінця перезарядки
        protected double ReloadTimer { get; set; }

        // Чи відбувається перезарядка
        protected bool IsReloading { get; set; }

        // Чи це зброя ворога
        public bool IsEnemyWeapon { get; set; }

        public Weapon()
        {
            IsEnemyWeapon = false;
        }

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

        public virtual Projectile CreateProjectile(Point from, Vector direction, object owner)
        {
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