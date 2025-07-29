using System;
using System.Collections.Generic;
using System.Linq;

namespace DiabloMapGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            MotorGen m = new MotorGen(4, 4500);
            m.Generate(8.0f);
            m.Save();

            CathedralGen();
            CatacombsGen();
            CaveGen();
            HellGen();
        }

        static void CatacombsGen()
        {
            for (int i = 0; i < 32; ++i)
            {

                Map map;

                do
                {
                    map = new Map(40, 40);
                    map.SolidifyBorder();

                    Generation gen = new Generation(map);
                    List<HallwayInfo> halls = new List<HallwayInfo>();
                    gen.QuadrantRoomGen(map, new MapRect(map), halls, false, 0, 0, 40, 40);
                    gen.TraceHallwaysSimple(map, halls);

                    var roomList = map.Rooms.Values.ToList();
                    foreach (var room in roomList)
                    {
                        gen.BudRoom(map, room, false, 6, 12, 6, 12, 3, 0, Axis.North);
                        gen.BudRoom(map, room, false, 6, 12, 6, 12, 3, 0, Axis.South);
                        gen.BudRoom(map, room, false, 6, 12, 6, 12, 3, 0, Axis.East);
                        gen.BudRoom(map, room, false, 6, 12, 6, 12, 3, 0, Axis.West);
                    }

                    roomList = map.Rooms.Values.ToList();
                    foreach (var room in roomList)
                    {
                        gen.BudRoom(map, room, false, 3, 5, 3, 5, 3, 0, Axis.North);
                        gen.BudRoom(map, room, false, 3, 5, 3, 5, 3, 0, Axis.South);
                        gen.BudRoom(map, room, false, 3, 5, 3, 5, 3, 0, Axis.East);
                        gen.BudRoom(map, room, false, 3, 5, 3, 5, 3, 0, Axis.West);
                    }

                    roomList = map.Rooms.Values.ToList();
                    foreach (var room in roomList)
                    {
                        gen.BudRoom(map, room, false, 2, 5, 2, 5, 3, 0, Axis.North);
                        gen.BudRoom(map, room, false, 2, 5, 2, 5, 3, 0, Axis.South);
                        gen.BudRoom(map, room, false, 2, 5, 2, 5, 3, 0, Axis.East);
                        gen.BudRoom(map, room, false, 2, 5, 2, 5, 3, 0, Axis.West);
                    }

                } while (map.Rooms.Count < 10);

                Render.Draw(map, $"gen/combs_line{i + 1}.png");
                Render.DrawRoomID(map, $"gen/combs{i + 1}.png");
            }
        }

        static void CaveGen()
        {
            for (int i = 0; i < 32; ++i)
            {

                Map map;
                do
                {
                    map = new Map(40, 40);
                    map.SolidifyBorder();

                    Generation gen = new Generation(map);
                    gen.Cave(map);
                } while (map.Rooms.Count < 10);

                Render.Draw(map, $"gen/gen_cave_lines{i + 1}.png");
                Render.DrawRoomGeneration(map, $"gen/gen_cave{i + 1}.png");
                //Render.DrawRoomID(map, $"gen/rooms{i + 1}.png");
            }
        }

        static void CathedralGen()
        {
            for (int i = 0; i < 32; ++i)
            {

                Map map;

                do
                {
                    map = new Map(40, 40);
                    map.SolidifyBorder();

                    Generation gen = new Generation(map);

                    Room a, b, c, hall;
                    gen.CathedralBaseBass(map, out a, out b, out c, out hall);
                    RecurseBud(gen, map, a, 0, gen.r.Next(4, 8), Axis.East);
                    RecurseBud(gen, map, a, 0, gen.r.Next(4, 8), Axis.West);
                    RecurseBud(gen, map, b, 0, gen.r.Next(4, 8), Axis.East);
                    RecurseBud(gen, map, b, 0, gen.r.Next(4, 8), Axis.West);
                    if (c != null)
                    {
                        RecurseBud(gen, map, c, 0, 5, Axis.North);
                        RecurseBud(gen, map, c, 0, 5, Axis.South);
                        if (hall != null)
                        {
                            HallBud(gen, map, hall, 0, 5, Axis.West);
                            HallBud(gen, map, hall, 0, 5, Axis.West);
                        }
                    }
                    else
                    {
                        if (hall != null)
                        {
                            HallBud(gen, map, hall, 0, 5, Axis.West);
                            HallBud(gen, map, hall, 0, 5, Axis.East);
                        }
                    }
                } while (map.WalkableRatio() < 0.4f);

                Render.Draw(map, $"gen/cath{i + 1}.png");
                //Render.DrawRoomID(map, $"gen/rooms{i + 1}.png");
                Render.DrawRoomGeneration(map, $"gen/cath_gen{i + 1}.png");
            }
        }
        static void RecurseBud(Generation gen, Map map, Room r, int depth, int maxDepth, Axis firstAxis)
        {
            if (depth == maxDepth)
                return;

            Axis newAxis = Axis.Random;
            if (gen.r.Next(0, 3) == 0) // 1 in 4 chance of changing direction
            {
                do
                {
                    newAxis = (Axis)gen.r.Next(0, 4);
                } while (newAxis == firstAxis);
            }
            else
                newAxis = firstAxis;

            Room nr;
            int tries = 0;
            do {
                if (tries > 0)
                    newAxis = (Axis)gen.r.Next(0, 4);

                tries++;
                nr = gen.BudRoom(map, r, depth > 3, 2, 6, 2, 6, 3, depth, depth == 0 ? firstAxis : newAxis);
            } while (nr == null && tries < 5);

            if (nr == null)
                return;

            RecurseBud(gen, map, nr != null ? nr : r, depth + 1, maxDepth, newAxis);
            RecurseBud(gen, map, nr != null ? nr : r, depth + 1, maxDepth, newAxis);
        }

        static void HallBud(Generation gen, Map map, Room r, int depth, int maxDepth, Axis firstAxis)
        {
            if (depth == maxDepth)
                return;

            int shift = depth == 0 ? Math.Max(r.h, r.w) / 2 - 3 : 0;

            Axis newAxis = Axis.Random;
            if (gen.r.Next(0, 3) == 0) // 1 in 4 chance of changing direction
            {
                do { 
                    newAxis = (Axis)gen.r.Next(0, 4);
                } while (newAxis == firstAxis);
            }
            else
                newAxis = firstAxis;

            Room nr;
            int tries = 0;
            do {
                if (tries > 0)
                    newAxis = (Axis)gen.r.Next(0, 4);
                nr = gen.BudRoom(map, r, depth > 3, 2, 6, 2, 6, shift, depth, depth == 0 ? firstAxis : newAxis); 
                ++tries;
            } while (nr == null && tries <= 5);

            if (nr == null)
                return;

            HallBud(gen, map, nr != null ? nr : r, depth + 1, maxDepth, newAxis);
            HallBud(gen, map, nr != null ? nr : r, depth + 1, maxDepth, newAxis);
        }

        static void HellGen()
        {
            for (int i = 0; i < 32; ++i)
            {

                Map map;
                do
                {
                    map = new Map(20, 20);

                    Generation gen = new Generation(map);                    
                    Room r = gen.SpawnCentral(map, gen.r.Next(5, 7), gen.r.Next(5, 7));
                    RecurseBud(gen, map, r, 0, 8, Axis.Random);

                    if (!map.EdgeOkay(Axis.South))
                    {
                        Room picked = map.ExtremeRoom(Axis.South);//gen.RandomRoom(map);
                        int x = picked.CenterX;
                        int y = picked.Bottom;

                        for (; y < map.h; ++y)
                        {
                            map.RoomTable[x, y] = 99;
                            if (x - 1 > 0)
                                map.RoomTable[x-1, y] = 99;
                            else
                                map.RoomTable[x+1,y] = 99;
                        }
                    }

                    if (!map.EdgeOkay(Axis.East))
                    {
                        Room picked = map.ExtremeRoom(Axis.East);//gen.RandomRoom(map);
                        int x = picked.Right;
                        int y = picked.CenterY;
                        for (; x < map.w; ++x)
                        {
                            map.RoomTable[x, y] = 99;
                            if (y - 1 > 0)
                                map.RoomTable[x, y - 1] = 99;
                            else
                                map.RoomTable[x, y + 1] = 99;
                        }
                    }

                } while (map.Rooms.Count < 8);

                Map clone = new Map(40, 40, map);
                clone.MirrorHorizontal();
                clone.MirrorVertical();

                //Render.DrawRoomGeneration(map, $"gen/gen_hell{i + 1}.png");
                Render.Draw(clone, $"gen/hell_lines{i + 1}.png");
                Render.DrawRoomID(clone, $"gen/id_hell{i + 1}.png");
            }
        }
    }
}
