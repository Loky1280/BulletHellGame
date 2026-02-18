using System;
using System.Collections.Generic;
using System.Windows;
using General_logic;
using Enemies;

namespace GameWorld
{
    public class FillWorld
    {
        public List<Room> rooms;
        public List<Corridor> corridors;
        private Random rnd;

        public FillWorld()
        {
            rooms = new List<Room>();
            corridors = new List<Corridor>();
            rnd = new Random();
        }

        public static void SpawnEnemiesToRooms(List<Room> rooms,List<Corridor> corridors, Room mainRoom, Room bossRoom)
        {
            Random rnd = new Random();
            foreach (var room in rooms)
            {
                if (room == mainRoom || room == bossRoom || room.IsShopRoom || room.IsTreasureRoom)
                    continue;

                int enemyCount = rnd.Next(2, 6);
                room.enemies = new List<Enemy>();

                int maxAttempts = 1000;
                int minDistanceToCorridor = 50;
                int minDistanceToOtherEnemy = 40;

                for (int i = 0; i < enemyCount; i++)
                {
                    Point position = new Point();
                    bool validPosition = false;
                    int attempts = 0;

                    while (!validPosition && attempts < maxAttempts)
                    {
                        attempts++;
                        double x = rnd.Next((int)room.X + 50, (int)(room.X + room.Width - 50));
                        double y = rnd.Next((int)room.Y + 50, (int)(room.Y + room.Height - 50));
                        position = new Point(x, y);

                        foreach (var other in room.enemies)
                        {
                            if ((other.Position - position).Length < minDistanceToOtherEnemy)
                            {
                                validPosition = false;
                                break;
                            }
                        }
                        if (!validPosition) continue;
                        
                        foreach (var corridor in corridors) 
                        {
                            foreach (var seg in corridor.Segments)
                            {
                                double dx = Math.Max(seg.Left - position.X, 0);
                                dx = Math.Max(dx, position.X - seg.Right);
                                double dy = Math.Max(seg.Top - position.Y, 0);
                                dy = Math.Max(dy, position.Y - seg.Bottom);
                                double dist = Math.Sqrt(dx * dx + dy * dy);
                                if (dist < minDistanceToCorridor)
                                {
                                    validPosition = false;
                                    break;
                                }
                            }
                            if (!validPosition) break;
                        }
                    }

                    Enemy enemy;

                    switch (rnd.Next(3))
                    {
                        case 0:
                            enemy = new CommonEnemy(position, new Pistol() { IsEnemyWeapon = true});
                            break;
                        case 1:
                            enemy = new CircularEnemy(position, new CircularWeapon() { IsEnemyWeapon = true});
                            break;
                        case 2:
                            enemy = new MultiAttackEnemy(position, new ShotGun() { IsEnemyWeapon = true });
                            break;
                        default:
                            enemy = new CommonEnemy(position, new Pistol() { IsEnemyWeapon = true });
                            break;
                    }

                    room.enemies.Add(enemy);
                }
            }
        }
        public static void HandleRoomEntry(Room currentRoom)
        {
            if (currentRoom == null) return;

            if (!currentRoom.IsVisited)
            {
                currentRoom.IsVisited = true;
                currentRoom.IsBattleBegin = currentRoom.enemies != null && currentRoom.enemies.Any(e => e.IsAlive);

            }
            else
            {
                if (currentRoom.enemies == null || !currentRoom.enemies.Any(e => e.IsAlive))
                    currentRoom.IsBattleBegin = false;
            }
        }
    }
}
