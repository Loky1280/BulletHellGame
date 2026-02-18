using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GameWorld
{
    public class LevelGenerator
    {
        private Random random = new Random();

        public (List<Room> Rooms, List<Corridor> Corridors) GenerateRooms(int count, int maxWidth, int maxHeight)
        {
            List<Room> rooms = new List<Room>();
            List<Corridor> corridors = new List<Corridor>();

            Room mainRoom = new Room
            {
                X = -208,
                Y = -208,
                Width = 512,
                Height = 512,
                IsConnectToMainRoom = true,
            };
            rooms.Add(mainRoom);

            const double minDistanceBetweenRooms = 500;
            const int maxAttempts = 100;

            for (int i = 0; i < count; i++)
            {
                Room newRoom;
                bool isTooClose;
                int attempts = 0;

                do
                {
                    newRoom = new Room
                    {
                        X = random.Next(-3500, 3500),
                        Y = random.Next(-3500, 3500),
                        Width = random.Next(400, 800),
                        Height = random.Next(400, 800),
                        IsConnectToMainRoom = false,
                        PeaceFulTime = 0.7
                    };

                    int widthRemainder = newRoom.Width % 128;
                    if (widthRemainder != 0)
                        newRoom.Width += 128 - widthRemainder;
                    int heightRemainder = newRoom.Height % 128;
                    if (heightRemainder != 0)
                        newRoom.Height += 128 - heightRemainder;

                    isTooClose = rooms.Any(existingRoom =>
                        GenerateHelp.DistanceBetweenRoomsEdges(existingRoom, newRoom) < minDistanceBetweenRooms ||
                        existingRoom.Bounds.IntersectsWith(newRoom.Bounds));

                    attempts++;

                    if (attempts > maxAttempts)
                    {
                        isTooClose = false;
                        break;
                    }

                } while (isTooClose);

                rooms.Add(newRoom);
            }

            var edges = new List<(Room A, Room B, double Distance)>();
            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    var a = rooms[i];
                    var b = rooms[j];
                    double dist = GenerateHelp.DistanceBetweenCenters(a, b);
                    edges.Add((a, b, dist));
                }
            }

            edges.Sort((e1, e2) => e1.Distance.CompareTo(e2.Distance));

            var parent = new Dictionary<Room, Room>();
            Room Find(Room r)
            {
                if (!parent.ContainsKey(r)) parent[r] = r;
                if (parent[r] != r) parent[r] = Find(parent[r]);
                return parent[r];
            }
            void Union(Room a, Room b)
            {
                parent[Find(a)] = Find(b);
            }

            foreach (var (a, b, dist) in edges)
            {
                if (Find(a) != Find(b))
                {
                    var corridor = new Corridor(a, b);
                    if (!GenerateHelp.DoesCorridorIntersectExisting(corridor, corridors) &&
                        !GenerateHelp.DoesCorridorIntersectAnyRoom(corridor, rooms, a, b))
                    {
                        corridors.Add(corridor);
                        Union(a, b);
                        a.IsConnectToMainRoom = true;
                        b.IsConnectToMainRoom = true;
                        GenerateHelp.UpdateRoomConnectionFlags(a, b);
                    }
                }
            }

            var unconnectedRooms = rooms.Where(r => !r.IsConnectToMainRoom).ToList();
            foreach (var room in unconnectedRooms)
            {
                var nearestConnected = rooms
                    .Where(r => r.IsConnectToMainRoom)
                    .OrderBy(r => GenerateHelp.DistanceBetweenCenters(room, r))
                    .FirstOrDefault();

                if (nearestConnected != null)
                {
                    var corridor = new Corridor(room, nearestConnected);
                    corridors.Add(corridor);
                    room.IsConnectToMainRoom = true;
                    nearestConnected.IsConnectToMainRoom = true;
                    GenerateHelp.UpdateRoomConnectionFlags(room, nearestConnected);
                }
            }

            Dictionary<Room, int> component = new Dictionary<Room, int>();
            int compId = 0;
            foreach (var room in rooms)
            {
                if (!component.ContainsKey(room))
                {
                    Queue<Room> q = new Queue<Room>();
                    q.Enqueue(room);
                    component[room] = compId;
                    while (q.Count > 0)
                    {
                        var r = q.Dequeue();
                        foreach (var c in corridors.Where(cor => cor.RoomA == r || cor.RoomB == r))
                        {
                            var other = c.RoomA == r ? c.RoomB : c.RoomA;
                            if (!component.ContainsKey(other))
                            {
                                component[other] = compId;
                                q.Enqueue(other);
                            }
                        }
                    }
                    compId++;
                }
            }

            while (component.Values.Distinct().Count() > 1)
            {
                double minDist = double.MaxValue;
                Room bestA = null, bestB = null;
                foreach (var a in rooms)
                {
                    foreach (var b in rooms)
                    {
                        if (component[a] != component[b])
                        {
                            double dist = GenerateHelp.DistanceBetweenCenters(a, b);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                bestA = a;
                                bestB = b;
                            }
                        }
                    }
                }
                var corridor = new Corridor(bestA, bestB);
                corridors.Add(corridor);
                GenerateHelp.UpdateRoomConnectionFlags(bestA, bestB);

                int oldComp = component[bestB];
                int newComp = component[bestA];
                foreach (var r in rooms.Where(r => component[r] == oldComp))
                    component[r] = newComp;
            }

            var deadEnds = rooms
                .Where(r => r != mainRoom && corridors.Count(c => c.RoomA == r || c.RoomB == r) == 1)
                .ToList();

            if (deadEnds.Count > 0)
            {
                var bossRoom = deadEnds
                    .OrderByDescending(r => GenerateHelp.DistanceBetweenCenters(mainRoom, r))
                    .First();
                bossRoom.IsBossRoom = true;
                deadEnds.Remove(bossRoom); 
            }

            if (deadEnds.Count > 0)
            {
                var treasureRoom = deadEnds
                    .OrderByDescending(r => GenerateHelp.DistanceBetweenCenters(
                        rooms.FirstOrDefault(room => room.IsBossRoom) ?? mainRoom, r))
                    .First();
                treasureRoom.IsTreasureRoom = true;
                deadEnds.Remove(treasureRoom); 
            }

            if (deadEnds.Count > 0)
            {
                var shopRoom = deadEnds.First();
                shopRoom.IsShopRoom = true;
            }
            else
            {

                var boss = rooms.FirstOrDefault(r => r.IsBossRoom);
                var treasure = rooms.FirstOrDefault(r => r.IsTreasureRoom);
                var shopCandidate = rooms
                    .Where(r => r != mainRoom && r != boss && r != treasure && !r.IsShopRoom)
                    .OrderByDescending(r => GenerateHelp.DistanceBetweenCenters(boss ?? mainRoom, r))
                    .FirstOrDefault();
                if (shopCandidate != null)
                    shopCandidate.IsShopRoom = true;
            }

            return (rooms, corridors);
        }

    }
}