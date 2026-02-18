using System.Windows;
using General_logic;
using Player;
using Enemies;

namespace GameWorld
{
    public static class Collision
    {
        public static bool CheckProjectileWallCollision(Projectile proj, List<Room> rooms, List<Corridor> corridors)
        {
            bool isInside = false;
            foreach (var room in rooms)
            {
                if (room.Bounds.Contains(proj.Position))
                {
                    isInside = true;
                    break;
                }
            }
            if (!isInside && corridors != null)
            {
                foreach (var corridor in corridors)
                {
                    foreach (var segment in corridor.Segments)
                    {
                        if (segment.Contains(proj.Position))
                        {
                            isInside = true;
                            break;
                        }
                    }
                    if (isInside) break;
                }
            }
            return !isInside;
        }

        public static bool CheckProjectileEnemyCollision(Projectile proj, List<Enemy> enemies, out Enemy hitEnemy)
        {
            hitEnemy = null;
            if (enemies == null) return false;

            Rect projRect = new Rect(proj.Position.X - 4, proj.Position.Y - 4, 8, 8);
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                if (projRect.IntersectsWith(enemy.Bounds))
                {
                    hitEnemy = enemy;
                    return true;
                }
            }
            return false;
        }

        public static bool CheckProjectilePlayerCollision(Projectile proj, PlayerMove player, double playerImageWidth, double playerImageHeight)
        {
            Rect projRect = new Rect(proj.Position.X - 4, proj.Position.Y - 4, 8, 8);
            Rect playerRect = new Rect(
                player.X - playerImageWidth / 2,
                player.Y - playerImageHeight / 2,
                playerImageWidth,
                playerImageHeight
            );
            return projRect.IntersectsWith(playerRect);
        }

        public static bool CheckPlayerShopItemCollision(
            PlayerMove player,
            double playerImageWidth,
            double playerImageHeight,
            List<Rect> shopItems,
            out int itemIndex)
        {
            itemIndex = -1;
            Rect playerRect = new Rect(
                player.X - playerImageWidth / 2,
                player.Y - playerImageHeight / 2,
                playerImageWidth,
                playerImageHeight
            );

            for (int i = shopItems.Count - 1; i >= 0; i--)
            {
                if (shopItems[i].IntersectsWith(playerRect))
                {
                    itemIndex = i;
                    return true;
                }
            }
            return false;
        }

        public static bool CheckPlayerTreasureCollision(
            PlayerMove player,
            double playerImageWidth,
            double playerImageHeight,
            Rect treasureRect)
        {
            Rect playerRect = new Rect(
                player.X - playerImageWidth / 2,
                player.Y - playerImageHeight / 2,
                playerImageWidth,
                playerImageHeight
            );
            return treasureRect.IntersectsWith(playerRect);
        }

        public static bool IsProjectileOutOfBounds(Projectile proj)
        {
            return proj.Position.X < -4500 || proj.Position.X > 4500 ||
                   proj.Position.Y < -4500 || proj.Position.Y > 4500;
        }
    }
}
