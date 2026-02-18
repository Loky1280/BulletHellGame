using System.Windows;
using Player;

namespace GameWorld
{
    public static class GenerateHelp
    {
        public static double DistanceBetweenRoomsEdges(Room a, Room b)
        {
            double dx = 0;
            if (a.X + a.Width < b.X)
                dx = b.X - (a.X + a.Width);
            else if (b.X + b.Width < a.X)
                dx = a.X - (b.X + b.Width);

            double dy = 0;
            if (a.Y + a.Height < b.Y)
                dy = b.Y - (a.Y + a.Height);
            else if (b.Y + b.Height < a.Y)
                dy = a.Y - (b.Y + b.Height);

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double DistanceBetweenCenters(Room a, Room b)
        {
            Point centerA = a.GetCenter();
            Point centerB = b.GetCenter();
            double dx = centerA.X - centerB.X;
            double dy = centerA.Y - centerB.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static bool DoesCorridorIntersectExisting(Corridor newCorridor, List<Corridor> existingCorridors)
        {
            foreach (var existing in existingCorridors)
            {
                foreach (var seg1 in newCorridor.Segments)
                {
                    foreach (var seg2 in existing.Segments)
                    {
                        if (seg1.IntersectsWith(seg2))
                            return true;
                    }
                }
            }
            return false;
        }

        public static void UpdateRoomConnectionFlags(Room from, Room to)
        {
            Vector direction = new Vector(to.X - from.X, to.Y - from.Y);

            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                if (direction.X > 0)
                {
                    if (!from.PositiveXConnect && !to.NegativeXConnect)
                    {
                        from.PositiveXConnect = true;
                        to.NegativeXConnect = true;
                    }
                }
                else
                {
                    if (!from.NegativeXConnect && !to.PositiveXConnect)
                    {
                        from.NegativeXConnect = true;
                        to.PositiveXConnect = true;
                    }
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    if (!from.PositiveYConnect && !to.NegativeYConnect)
                    {
                        from.PositiveYConnect = true;
                        to.NegativeYConnect = true;
                    }
                }
                else
                {
                    if (!from.NegativeYConnect && !to.PositiveYConnect)
                    {
                        from.NegativeYConnect = true;
                        to.PositiveYConnect = true;
                    }
                }
            }
        }
        public static bool DoesCorridorIntersectAnyRoom(Corridor corridor, List<Room> rooms, Room exceptA, Room exceptB)
        {
            foreach (var room in rooms)
            {
                if (room == exceptA || room == exceptB)
                    continue;
                foreach (var seg in corridor.Segments)
                {
                    if (room.Bounds.IntersectsWith(seg))
                        return true;
                }
            }
            return false;
        }
        public static Point GetSpawnPoint(Room mainRoom)
        {
            return mainRoom.GetCenter();
        }
        public static Point GetRandomPointInRoom(Room room, Random random)
        {
            double x = random.Next((int)room.X + 20, (int)(room.X + room.Width - 20));
            double y = random.Next((int)room.Y + 20, (int)(room.Y + room.Height - 20));
            return new Point(x, y);
        }

        public static List<Point> GetRandomEnemyPositionsInRoom(
            Room room,
            List<Corridor> corridors,
            Random random,
            int enemyCount,
            double minDistanceBetweenEnemies = 40,
            double minDistanceToCorridor = 40)
        {
            var positions = new List<Point>();
            int maxAttempts = 1000;

            for (int i = 0; i < enemyCount; i++)
            {
                bool placed = false;
                int attempts = 0;
                while (!placed && attempts < maxAttempts)
                {
                    attempts++;
                    Point candidate = GetRandomPointInRoom(room, random);

                    bool tooCloseToOther = positions.Any(p => (p - candidate).Length < minDistanceBetweenEnemies);
                    if (tooCloseToOther)
                        continue;

                    bool tooCloseToCorridor = false;
                    foreach (var corridor in corridors)
                    {
                        foreach (var seg in corridor.Segments)
                        {
                            double dx = Math.Max(seg.Left - candidate.X, 0);
                            dx = Math.Max(dx, candidate.X - seg.Right);
                            double dy = Math.Max(seg.Top - candidate.Y, 0);
                            dy = Math.Max(dy, candidate.Y - seg.Bottom);
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (dist < minDistanceToCorridor)
                            {
                                tooCloseToCorridor = true;
                                break;
                            }
                        }
                        if (tooCloseToCorridor) break;
                    }
                    if (tooCloseToCorridor)
                        continue;

                    positions.Add(candidate);
                    placed = true;
                }
            }
            return positions;
        }
        public static Room GetPlayerCurrentRoom(List<Room> rooms, PlayerMove player)
        {
            foreach (var room in rooms)
            {
                Rect playerRect = new Rect(player.X, player.Y, 30, 30);
                if (room.Bounds.IntersectsWith(playerRect))
                    return room;
            }
            return null;
        }

        public static void HandleShopInteraction(
            Room shopRoom,
            Room currentRoom,
            PlayerMove player,
            double playerImageWidth,
            double playerImageHeight,
            ref int playerMoney,
            ref List<Rect> shopHpRects,
            ref List<Rect> shopArmorRects)
        {
            if (shopRoom != null && shopRoom == currentRoom)
            {
                Rect playerRect = new Rect(player.X - playerImageWidth / 2, player.Y - playerImageHeight / 2, playerImageWidth, playerImageHeight);
                for (int i = shopHpRects.Count - 1; i >= 0; i--)
                {
                    if (shopHpRects[i].IntersectsWith(playerRect))
                    {
                        if (playerMoney >= 2 && player.Character.Health < 4)
                        {
                            playerMoney -= 2;
                            player.Character.Health++;
                            shopHpRects.RemoveAt(i);
                        }
                    }
                }
                for (int i = shopArmorRects.Count - 1; i >= 0; i--)
                {
                    if (shopArmorRects[i].IntersectsWith(playerRect))
                    {
                        if (playerMoney >= 3)
                        {
                            playerMoney -= 3;
                            player.Character.Armor++;
                            shopArmorRects.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
