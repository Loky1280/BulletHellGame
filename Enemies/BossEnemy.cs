using System.Windows;
using General_logic;
using System.Collections.Generic;
using System;

namespace Enemies
{
    public class BossEnemy : Enemy
    {
        private ShotGun shotGun;
        private CircularWeapon circularWeapon;
        private AutomaticWeapon autoWeapon;
        private int burstShotsLeft = 0;
        private double burstCooldown = 0;

        public BossEnemy(Point position) : base(position, null)
        {
            Position = position;
            Direction = new Vector(0, 0);
            Health = MaxHealth = 30;
            MoveSpeed = 20;
            Damage = 1;
            AttackRadius = 1000;
            CloseRange = 30;
            NormalAttackCooldown = 1.0;
            FastAttackCooldown = 0.3;
            AttackCooldown = NormalAttackCooldown;
            AttackTimer = 0;
            IsAlive = true;
            IsBoss = true;

            shotGun = new ShotGun() { IsEnemyWeapon = true };
            shotGun.FireRate = 0.35;
            shotGun.ReloadTime = 0.5;
            circularWeapon = new CircularWeapon() { IsEnemyWeapon = true };
            circularWeapon.FireRate = 0.7;
            circularWeapon.ReloadTime = 0.9;
            autoWeapon = new AutomaticWeapon() { IsEnemyWeapon = true };
            autoWeapon.FireRate = 0.2;
            autoWeapon.ReloadTime = 0.9;

            Gun = shotGun;
        }

        public override void UpdateAI(Point playerPos, bool playerInRoom, double dt, List<General_logic.Projectile> projectiles, List<Enemy> allEnemiesInRoom, double peaceFulTime)
        {
            if (!playerInRoom || !IsAlive)
                return;

            double dist = (playerPos - Position).Length;

            if (peaceFulTime > 0)
                return;

            if (dist <= 190)
            {
                Gun = shotGun;
                Gun.Update(dt);
                AttackTimer -= dt;
                if (AttackTimer <= 0)
                {
                    Vector direction = playerPos - Position;
                    direction.Normalize();
                    List<Projectile> additionalPellets;
                    var projectile = Gun.CreateProjectile(Position, direction, this, out additionalPellets);
                    if (projectile != null)
                    {
                        projectiles.Add(projectile);
                        projectiles.AddRange(additionalPellets);
                    }
                    AttackTimer = AttackCooldown;
                }
            }
            else if (dist <= 350)
            {
                Gun = circularWeapon;
                Gun.Update(dt);
                AttackTimer -= dt;
                if (AttackTimer <= 0)
                {
                    Vector direction = playerPos - Position;
                    direction.Normalize();
                    List<Projectile> additionalPellets;
                    var projectile = Gun.CreateProjectile(Position, direction, this, out additionalPellets);
                    if (projectile != null)
                    {
                        projectiles.Add(projectile);
                        projectiles.AddRange(additionalPellets);
                    }
                    AttackTimer = AttackCooldown;
                }
            }
            else
            {
                Gun = autoWeapon;
                Gun.Update(dt);
                burstCooldown -= dt;
                if (burstShotsLeft > 0 && burstCooldown <= 0)
                {
                    Vector direction = playerPos - Position;
                    direction.Normalize();
                    List<Projectile> additionalPellets;
                    var projectile = Gun.CreateProjectile(Position, direction, this, out additionalPellets);
                    if (projectile != null)
                    {
                        projectiles.Add(projectile);
                        projectiles.AddRange(additionalPellets);
                    }
                    burstShotsLeft--;
                    burstCooldown = 0.1; 
                }
                else if (burstShotsLeft == 0 && AttackTimer <= 0)
                {
                    burstShotsLeft = 3; 
                    AttackTimer = 0.2;
                }
                AttackTimer -= dt;
            }

            if (dist > 100)
            {
                Vector toPlayer = playerPos - Position;
                toPlayer.Normalize();
                Position += toPlayer * MoveSpeed * dt;
            }
        }
    }
}
