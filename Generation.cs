using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DiabloMapGen
{

    public enum Axis
    {
        North,
        South,
        East,
        West,
        Random
    }

    public class HallwayInfo
    {
        public int startX, startY;
        public int endX, endY;
        public Axis direction;
        public Room start;
        public Room end;
    }

    public class Room
    {
        public int ID = 0;
        public int Gen = 0;
        public Room Origin;
        public int x, y, w, h;
        public int Left { get {  return x; } }
        public int Right { get { return x + w; } }
        public int Top { get { return y; } }
        public int Bottom { get { return y + h; } }

        public int CenterX { get {  return x + w/2; } }

        public int CenterY { get { return y + h/2; } }

        public bool Overlaps(Room other)
        {
            System.Drawing.Point l1 = new System.Drawing.Point(x, Top);
            System.Drawing.Point r1 = new System.Drawing.Point(Right, Bottom);
            System.Drawing.Point l2 = new System.Drawing.Point(other.x, other.Top);
            System.Drawing.Point r2 = new System.Drawing.Point(other.Right, other.Bottom);

            if (l1.X == r1.X || l1.Y == r1.Y || l2.X == r2.X || l2.Y == r2.Y)
            {
                // the line cannot have positive overlap
                return false;
            }

            // If one rectangle is on left side of other
            if (l1.X >= r2.X || l2.X >= r1.X)
                return false;

            // If one rectangle is above other
            if (r1.Y >= l2.Y || r2.Y >= l1.Y)
                return false;

            return true;
        }

        public bool EntirelyInside(Room other)
        {
            if (x > other.x && y > other.y && Bottom < other.Bottom && Right < other.Right)
                return true;
            return false;
        }
    }

    public class Map : Room
    {
        public Map(int w, int h)
        {
            x = 0;
            y = 0;
            this.w = w;
            this.h = h;
            RoomTable = new int[w, h];
            TileTable = new int[w, h];

            for (int x = 0; x < w; ++x)
            { 
                for (int y = 0; y < h; ++y)
                {
                    RoomTable[x,y] = 0;
                    TileTable[x,y] = 0;
                }
            }
        }

        /// <summary>
        /// Copy (potentially partially) from another map.
        /// </summary>
        public Map(int w, int h, Map src) : this(w, h)
        {
            for (int x = 0; x < src.w; ++x)
            { 
                for (int y = 0; y < src.h; ++y)
                {
                    RoomTable[x, y] = src.RoomTable[x, y];
                    TileTable[x, y] = src.TileTable[x, y];
                }
            }
        }

        public int[,] RoomTable;
        public int[,] TileTable;

        public Dictionary<int, Room> Rooms = new Dictionary<int, Room>();

        public void AddRoom(Room room)
        {
            Rooms.Add(room.ID, room);
            for (int x = room.x; x <= room.Right; ++x)
            {
                for (int y = room.y; y <= room.Bottom; ++y)
                    RoomTable[x,y] = room.ID;
            }
        }
        public bool IsRoomSafe(Room r)
        {
            if (r.Right > Right || r.Bottom > Bottom || r.x < x || r.y < y)
                return false;

            for (int x = r.x; x <= r.Right; ++x)
            {
                for (int y = r.y; y <= r.Bottom; ++y)
                    if (RoomTable[x,y] > 0 || RoomTable[x,y] == -1)
                        return false;
            }
            return true;
        }

        /// <summary>
        /// Solidifies the outermost border
        /// </summary>
        public void SolidifyBorder()
        {
            for (int y = 0; y < h; ++y)
            {
                RoomTable[0, y] = -1;
                RoomTable[w-1,y] = -1;
                TileTable[0, y] = -1;
                TileTable[w - 1, y] = -1;
            }

            for (int x = 0; x < w; ++x)
            {
                RoomTable[x, 0] = -1;
                RoomTable[x, h-1] = -1;
                TileTable[x, 0] = -1;
                TileTable[x, h - 1] = -1;
            }
        }

        public void FlipVertical()
        {
            Map m = new Map(w, h, this);
            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    RoomTable[x, h - 1 - y] = m.RoomTable[x, y];
                    TileTable[x, h - 1 - y] = m.TileTable[x, y];
                }
            }
        }

        public void FlipHorizontal()
        {
            Map m = new Map(w, h, this);
            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    RoomTable[w - x - 1, y] = m.RoomTable[x,y];
                    TileTable[w - x - 1, y] = m.TileTable[x, y];
                }
            }
        }

        /// <summary>
        /// Dialbo 1 Hell Map style symmetry.
        /// </summary>
        public void ExpandQuarterViaMirror()
        {
            MirrorHorizontal();
            MirrorVertical();
        }

        /// <summary>
        /// Mirrors the map vertically
        /// </summary>
        public void MirrorHorizontal()
        {
            int halfW = w / 2;
            for (int x = 0; x < halfW; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    RoomTable[halfW + x, y] = RoomTable[halfW - x - 1, y];
                    TileTable[halfW + x, y] = TileTable[halfW - x - 1, y];
                }
            }
        }

        /// <summary>
        /// Mirrors the map vertically
        /// </summary>
        public void MirrorVertical()
        {
            int halfH = h / 2;
            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < halfH; ++y)
                {
                    RoomTable[x, halfH + y] = RoomTable[x, halfH - y - 1];
                    TileTable[x, halfH + y] = TileTable[x, halfH - y - 1];
                }
            }
        }

        public int CountRoomMarked()
        {
            int ct = 0;
            for (int x = 0; x < w; ++x)
                for (int y = 0; y < h; ++y)
                    ct += RoomTable[x,y] > 0 ? 1 : 0;
            return ct;
        }

        public float WalkableRatio()
        {
            return (float)CountRoomMarked() / (float)(w * h);
        }

        bool DualEquals(int a, int c1, int c2)
        {
            if (a == c1 || a == c2)
                return true;

            Room r;
            if (Rooms.TryGetValue(a, out r))
            { 
                if (r != null)
                    if (r.Origin != null && r.Origin.ID == c1)
                        return true;
            }

            return false;
        }

        public bool[,] GetNeightbors_RoomID(int x, int y)
        {
            int myID = RoomTable[x,y];
            Room room;
            Rooms.TryGetValue(myID, out room);
            int originID = room != null && room.Origin != null ? room.Origin.ID : 999999;

            bool[,] r = new bool[3,3];
            // ROW, COLUMN
            r[0,0] = DualEquals(RoomTable[Math.Max(x - 1, 0), Math.Max(y - 1, 0)], myID, originID);
            r[0,1] = DualEquals(RoomTable[x, Math.Max(y - 1, 0)], myID, originID);
            r[0,2] = DualEquals(RoomTable[Math.Min(x + 1, w-1), Math.Max(y - 1, 0)], myID, originID);

            r[1, 0] = DualEquals(RoomTable[Math.Max(x - 1, 0), y], myID, originID);
            r[1, 1] = DualEquals(RoomTable[x, y], myID, originID);
            r[1, 2] = DualEquals(RoomTable[Math.Min(x + 1, w-1), y], myID, originID);

            r[2, 0] = DualEquals(RoomTable[Math.Max(x - 1, 0), Math.Min(y + 1, h-1)], myID, originID);
            r[2, 1] = DualEquals(RoomTable[x, Math.Min(y + 1, h-1)], myID, originID);
            r[2, 2] = DualEquals(RoomTable[Math.Min(x + 1, w-1), Math.Min(y+1, h-1)], myID, originID);

            return r;
        }

        public bool[,] GetNeightbors_Marked(int x, int y)
        {
            int myID = RoomTable[x, y];

            bool[,] r = new bool[3, 3];
            // ROW, COLUMN
            r[0, 0] = RoomTable[Math.Max(x - 1, 0), Math.Max(y - 1, 0)] > 0;
            r[0, 1] = RoomTable[x, Math.Max(y - 1, 0)] > 0;
            r[0, 2] = RoomTable[Math.Min(x + 1, w - 1), Math.Max(y - 1, 0)] > 0;

            r[1, 0] = RoomTable[Math.Max(x - 1, 0), y] > 0;
            r[1, 1] = RoomTable[x, y] > 0;
            r[1, 2] = RoomTable[Math.Min(x + 1, w - 1), y] > 0;

            r[2, 0] = RoomTable[Math.Max(x - 1, 0), Math.Min(y + 1, h - 1)] > 0;
            r[2, 1] = RoomTable[x, Math.Min(y + 1, h - 1)] > 0;
            r[2, 2] = RoomTable[Math.Min(x + 1, w - 1), Math.Min(y + 1, h - 1)] > 0;

            return r;
        }
        public bool EdgeOkay(Axis axis)
        {
            if (axis == Axis.West)
            {
                for (int i = 0; i < h; ++i)
                {
                    if (RoomTable[0,i] > 0)
                        return true;
                }
            }
            else if (axis == Axis.East)
            {
                for (int i = 0; i < h; ++i)
                {
                    if (RoomTable[w - 1, i] > 0)
                        return true;
                }
            }
            else if (axis == Axis.North)
            {
                for (int i = 0; i < w; ++i)
                {
                    if (RoomTable[i, 0] > 0)
                        return true;
                }
            }
            else if (axis == Axis.South)
            {
                for (int i = 0; i < w; ++i)
                {
                    if (RoomTable[i, h - 1] > 0)
                        return true;
                }
            }

            return false;
        }

        public Room ExtremeRoom(Axis axis)
        {
            if (axis == Axis.North)
            {
                Room curBest = null;
                int y = int.MaxValue;
                foreach (var r in Rooms)
                    if (r.Value.Top < y)
                    {
                        curBest = r.Value;
                        y = r.Value.Top;
                    }

                return curBest;
            }
            else if (axis == Axis.South)
            {
                Room curBest = null;
                int y = int.MinValue;
                foreach (var r in Rooms)
                    if (r.Value.Bottom > y)
                    {
                        curBest = r.Value;
                        y = r.Value.Bottom;
                    }

                return curBest;
            }
            else if (axis == Axis.West)
            {
                Room curBest = null;
                int x = int.MaxValue;
                foreach (var r in Rooms)
                    if (r.Value.Left < x)
                    {
                        curBest = r.Value;
                        x = r.Value.Left;
                    }

                return curBest;
            }
            else if (axis == Axis.East)
            {
                Room curBest = null;
                int x = int.MinValue;
                foreach (var r in Rooms)
                    if (r.Value.Right > x)
                    {
                        curBest = r.Value;
                        x = r.Value.Right;
                    }

                return curBest;
            }

            return null;
        }

        public List<Room> RoomsBeyondAxis(Room refRoom, Axis axis)
        {
            List<Room> r = new List<Room>();
            if (axis == Axis.North)
            {
                foreach (Room rr in Rooms.Values)
                {
                    if (rr == refRoom)
                        continue;

                    if (rr.Bottom > refRoom.Top + 1)
                        r.Add(rr);
                }
            }
            else if (axis == Axis.South)
            {
                foreach (Room rr in Rooms.Values)
                {
                    if (rr == refRoom)
                        continue;

                    if (rr.Top < refRoom.Bottom - 1)
                        r.Add(rr);
                }
            }
            else if (axis == Axis.East)
            {
                foreach (Room rr in Rooms.Values)
                {
                    if (rr == refRoom)
                        continue;

                    if (rr.Left > refRoom.Right + 1)
                        r.Add(rr);
                }
            }
            else if (axis == Axis.West)
            {
                foreach (Room rr in Rooms.Values)
                {
                    if (rr == refRoom)
                        continue;

                    if (rr.Right < refRoom.Left - 1)
                        r.Add(rr);
                }
            }
            return r;
        }
    }

    public class Generation
    {
        int nextId_ = 0;
        Random rand = new Random();
        Map map;

        public Random r { get { return rand; } }

        public void Seed(int seed)
        {
            rand = new Random(seed);
        }

        public Generation(Map map)
        {
            this.map = map;
        }
        public Generation(Map map, int seed)
        {
            rand = new Random(seed);
            this.map = map;
        }

        public int GetID() { return ++nextId_; }

        public Room PlaceRoom(Map map, int minX, int maxX, int minY, int maxY, int minSizeX, int maxSizeX, int minSizeY, int maxSizeY, int id = -1)
        {
            Room r = new Room();
            r.ID = id != -1 ? id : GetID();
            r.Gen = 0;
            r.x = rand.Next(minX, maxX + 1);
            r.y = rand.Next(minY, maxY + 1);
            r.w = rand.Next(minSizeX, maxSizeX + 1);
            r.h = rand.Next(minSizeY, maxSizeY + 1);
            if (r.EntirelyInside(map) && map.IsRoomSafe(r))
            { 
                map.AddRoom(r);
                return r;
            }
            return null;
        }

        public Room ExplicitRoom(Map map, int x, int y, int w, int h)
        {
            Room r = new Room();
            r.ID = GetID();
            r.Gen = 0;
            r.x = x;
            r.y = y;
            r.w = w;
            r.h = h;
            if (r.EntirelyInside(map))// && map.IsRoomSafe(r))
            {
                map.AddRoom(r);
                return r;
            }
            return null;
        }

        public Room BudRoom(Map map, Room srcRoom, bool allowOverlap, int minW, int maxW, int minH, int maxH, int maxShift, int generation, Axis axis = Axis.Random)
        {
            if (axis == Axis.Random)
                axis = (Axis)rand.Next(0, 3);

            int w = rand.Next(minW, maxW + 1);
            int h = rand.Next(minH, maxH + 1);

            if (axis == Axis.North || axis == Axis.South)
                maxShift = Math.Min(maxShift, srcRoom.w / 2);
            if (axis == Axis.East || axis == Axis.West)
                maxShift = Math.Min(maxShift, srcRoom.h / 2);

            int shift = rand.Next(0, maxShift+1);

            Room newRoom = new Room();
            newRoom.w = w;
            newRoom.h = h;

            if (axis == Axis.North)
            {
                newRoom.x = srcRoom.x + shift;
                newRoom.y = srcRoom.Top - newRoom.h - 1;
            }
            else if (axis == Axis.South)
            {
                newRoom.x = srcRoom.x + shift;
                newRoom.y = srcRoom.Bottom + 1;
            }
            else if (axis == Axis.West)
            {
                newRoom.x = srcRoom.Left - newRoom.w - 1;
                newRoom.y = srcRoom.Top + shift;
            }
            else if (axis == Axis.East)
            {
                newRoom.x = srcRoom.Right + 1;
                newRoom.y = srcRoom.Top + shift;
            }

            if (!newRoom.EntirelyInside(map))
                return null;

            if (!map.IsRoomSafe(newRoom))
                return null;

            if (!allowOverlap)
            { 
                foreach (var kvp in map.Rooms)
                {
                    if (newRoom.Overlaps(kvp.Value))
                        return null;
                }
            }

            newRoom.ID = GetID();
            newRoom.Gen = generation;
            if (generation > 0)
                newRoom.Origin = srcRoom;
            map.AddRoom(newRoom);
            return newRoom;
        }

        public Room SpawnCentral(Map map, int w, int h)
        {
            Room r = new Room();
            r.ID = GetID();
            r.Gen = 0;

            int halfW = map.w / 2;
            int halfH = map.h / 2;
            int fract = halfW / 4;
            do { 
                r.x = rand.Next(halfW - fract, halfW + fract);
                r.y = rand.Next(halfH - fract, halfH + fract);
                r.w = w;
                r.h = h;
            } while (!map.IsRoomSafe(r));
            map.AddRoom(r);

            return r;
        }

        public void Cave(Map map)
        {
            Room r = SpawnCentral(map, 2, 2);         
            CaveBud(map, r, 1, Axis.Random);
        }

        public void CaveBud(Map map, Room srcRoom, int generation, Axis cameFrom)
        { 
            if (cameFrom != Axis.West)
            {
                for (int i = 0; i < 10; ++i)
                { 
                    int w = rand.Next(3, 5);
                    int h = rand.Next(3, 5);

                    Room r = new Room();
                    r.ID = GetID();
                    r.Origin = srcRoom;
                    r.Gen = generation;
                    r.x = srcRoom.Left - w - 1;
                    r.y = rand.Next(srcRoom.Top, srcRoom.Bottom);
                    r.w = w;
                    r.h = h;
                    if (map.IsRoomSafe(r))
                    { 
                        map.AddRoom(r);
                        CaveBud(map, r, generation + 1, Axis.East);
                        break;
                    }
                }
            }

            if (cameFrom != Axis.East)
            {
                for (int i = 0; i < 10; ++i)
                {
                    int w = rand.Next(3, 5);
                    int h = rand.Next(3, 5);

                    Room r = new Room();
                    r.ID = GetID();
                    r.Gen = generation;
                    r.x = srcRoom.Right + 1;
                    r.y = rand.Next(srcRoom.Top, srcRoom.Bottom);
                    r.w = w;
                    r.h = h;
                    if (map.IsRoomSafe(r))
                    {
                        map.AddRoom(r);
                        CaveBud(map, r, generation + 1, Axis.West);
                        break;
                    }
                }
            }

            if (cameFrom != Axis.North)
            {
                for (int i = 0; i < 10; ++i)
                {
                    int w = rand.Next(3, 5);
                    int h = rand.Next(3, 5);

                    Room r = new Room();
                    r.ID = GetID();
                    r.Gen = generation;
                    r.x = rand.Next(srcRoom.Left, srcRoom.Right);
                    r.y = srcRoom.Bottom + 1;
                    r.w = w;
                    r.h = h;
                    if (map.IsRoomSafe(r))
                    {
                        map.AddRoom(r);
                        CaveBud(map, r, generation + 1, Axis.South);
                        break;
                    }
                }
            }

            if (cameFrom != Axis.South)
            {
                for (int i = 0; i < 10; ++i)
                {
                    int w = rand.Next(3, 5);
                    int h = rand.Next(3, 5);

                    Room r = new Room();
                    r.ID = GetID();
                    r.Gen = generation;
                    r.x = rand.Next(srcRoom.Left, srcRoom.Right);
                    r.y = srcRoom.Top - h - 1;
                    r.w = w;
                    r.h = h;
                    if (map.IsRoomSafe(r))
                    {
                        map.AddRoom(r);
                        CaveBud(map, r, generation + 1, Axis.North);
                        break;
                    }
                }
            }
        }

        public void CathedralBaseBass(Map map, out Room a, out Room b, out Room c, out Room longHall)
        {
            int axis = rand.Next(0, 2);
            int ct = rand.Next(2, 4);

            int xMid = map.w / 2;
            int yMid = map.h / 2;

            int corridorSize = 6;
            int roomSize = 8;
            int halfRoomSize = 4;
            int halfCooridorSize = 3;

            if (axis == 0) // along Y
            {
                if (ct == 3)
                {
                    a = ExplicitRoom(map, xMid - halfRoomSize, map.h - 2 - roomSize, roomSize, roomSize);
                    b = ExplicitRoom(map, xMid - halfRoomSize, 2, roomSize, roomSize);
                    c = ExplicitRoom(map, map.w - 2 - roomSize, yMid - halfRoomSize, roomSize, roomSize);
            
                    Room hall = ExplicitRoom(map, xMid - halfCooridorSize, 2 + roomSize + 1, corridorSize, a.Top - b.Bottom - 2);
                    longHall = hall;
                    ExplicitRoom(map, hall.Right + 1, yMid - halfCooridorSize, c.Left - hall.Right - 2, corridorSize);
                }
                else
                {
                    a = ExplicitRoom(map, xMid - halfRoomSize, map.h - 2 - roomSize, roomSize, roomSize);
                    b = ExplicitRoom(map, xMid - halfRoomSize, 2, roomSize, roomSize);
                    c = null;
            
                    longHall = ExplicitRoom(map, xMid - halfCooridorSize, 2 + roomSize + 1, corridorSize, a.Top - b.Bottom - 2);
                }
            }
            else // along X
            {
                if (ct == 3)
                {
                    a = ExplicitRoom(map, 2, yMid - halfRoomSize, roomSize, roomSize);
                    b = ExplicitRoom(map, map.w - 2 - roomSize, yMid - halfRoomSize, roomSize, roomSize);
                    c = ExplicitRoom(map, xMid - halfRoomSize, 2, roomSize, roomSize);

                    Room hall = longHall = ExplicitRoom(map, a.Right + 1, yMid - halfCooridorSize, b.Left - a.Right - 2, corridorSize);
                    ExplicitRoom(map, xMid - halfCooridorSize, c.Bottom + 1, corridorSize, hall.Top - c.Bottom - 2);
                }
                else
                {
                    a = ExplicitRoom(map, 2, yMid - halfRoomSize, roomSize, roomSize);
                    b = ExplicitRoom(map, map.w - 2 - roomSize, yMid - halfRoomSize, roomSize, roomSize);
                    c = null;

                    longHall = ExplicitRoom(map, a.Right + 1, yMid - halfCooridorSize, b.Left - a.Right - 2, corridorSize);
                }
            }
        }

        public void QuadrantRoomGen(Map map, MapRect rect, List<HallwayInfo> halls, bool force, int fw, int fh, int maxW, int maxH, Axis hallDir = Axis.Random, HallwayInfo hall = null)
        {
            if (rect.w < 10 || rect.h < 10)
                return;

            int rW = rand.Next(4, 10);
            int rH = rand.Next(4, 10);

            if (force)
            { 
                rW = fw;
                rH = fh;
            }
            else
            {
                rW = Math.Min(rW, maxW);
                rH = Math.Min(rH, maxH);
            }

            if (rW < 4 || rH < 4)
                return;

            Room r;
            int tries = 0;
            do { 
                int pX = rand.Next(0, rect.w - rW - 1) + rect.x;
                int pY = rand.Next(0, rect.h - rH - 1) + rect.y;

                r = new Room();
                r.ID = GetID();
                r.x = pX;
                r.y = pY;
                r.w = rW;
                r.h = rH;
                if (map.IsRoomSafe(r))
                    map.AddRoom(r);
                else
                    r = null;
                
                ++tries;
                if (tries > 50)
                    return;
            } while (r == null);
            
            if (hallDir != Axis.Random && hall != null)
            {
                hall.end = r;
                if (hallDir == Axis.West)
                {
                    hall.endX = r.Right;
                    hall.endY = rand.Next(r.Top, r.Bottom);
                }
                else if (hallDir == Axis.East)
                {
                    hall.endX = r.Left;
                    hall.endY = rand.Next(r.Top, r.Bottom);
                }
                else if (hallDir == Axis.North)
                {
                    hall.endX = rand.Next(r.Left, r.Right);
                    hall.endY = r.Bottom;
                }
                else if (hallDir == Axis.South)
                {
                    hall.endX = rand.Next(r.Left, r.Right);
                    hall.endY = r.Top;
                }
                halls.Add(hall);
            }

            MapRect left, right, top, bottom;
            rect.PartitionAroundAxis(r.CenterX, r.CenterY, out left, out top, out right, out bottom);

            left = left.Shrink(2);
            right = right.Shrink(2);
            top = top.Shrink(2);
            bottom = bottom.Shrink(2);

            // taller than wide, favor X axis rooms first
            //if (rH > rW)
            { 
                QuadrantRoomGen(map, left, halls, false, 0, 0, left.w, left.h, Axis.West, 
                    new HallwayInfo { 
                        startX = r.x, 
                        startY = rand.Next(r.Top, r.Bottom), 
                        direction = Axis.West, 
                        start = r 
                    });
                QuadrantRoomGen(map, right, halls, false, 0, 0, right.w, right.h, Axis.East, 
                    new HallwayInfo { 
                        startX = r.Right, 
                        startY = rand.Next(r.Top, r.Bottom), 
                        direction = Axis.East, 
                        start = r 
                    });
                QuadrantRoomGen(map, top, halls, false, 0, 0, top.w, top.h, Axis.North, 
                    new HallwayInfo { 
                        startX = rand.Next(r.Left, r.Right), 
                        startY = r.Top, 
                        direction = Axis.North, 
                        start = r 
                    });
                QuadrantRoomGen(map, bottom, halls, false, 0, 0, bottom.w, bottom.h, Axis.South, 
                    new HallwayInfo { 
                        startX = rand.Next(r.Left, r.Right), 
                        startY = r.Bottom, 
                        direction = Axis.South, 
                        start = r 
                    });
            }
            //else // wider than tall, favor Y axis rooms first
            //{
            //    QuadrantRoomGen(map, top, halls, false, 0, 0, top.w, top.h, Axis.North, new HallwayInfo { startX = rand.Next(r.Left, r.Right), startY = r.Top, direction = Axis.North, start = r });
            //    QuadrantRoomGen(map, bottom, halls, false, 0, 0, bottom.w, bottom.h, Axis.South, new HallwayInfo { startX = rand.Next(r.Left, r.Right), startY = r.Top, direction = Axis.South, start = r });
            //    QuadrantRoomGen(map, left, halls, false, 0, 0, left.w, left.h, Axis.West, new HallwayInfo { startX = r.x, startY = rand.Next(r.Top, r.Bottom), direction = Axis.West, start = r });
            //    QuadrantRoomGen(map, right, halls, false, 0, 0, right.w, right.h, Axis.East, new HallwayInfo { startX = r.Right, startY = rand.Next(r.Top, r.Bottom), direction = Axis.East, start = r });
            //}
        }

        public void TraceHallwaysSimple(Map map, List<HallwayInfo> halls)
        {
            TraceHallwaysSimple(map, halls, new int[] { 1, 2, 3 });
        }

        public void TraceHallwaysSimple(Map map, List<HallwayInfo> halls, int[] hallSizes)
        {
            int[] xStep = new int[] { 0, 0, 1, -1 };
            int[] yStep = new int[] { -1, 1, 0, 0 };

            Action<Map,bool,int,int,int> blitHall = (Map map, bool virt, int ct, int x, int y) =>
            {
                if (ct == 1)
                    return;

                int num = (int)Math.Floor(ct / 2.0f);
                if (num == 0)
                    return;
                for (int i = 0; i < num; ++i)
                {
                    if (virt)
                    {
                        if (y - (i + 1) > 0)
                            map.RoomTable[x, y - (i + 1)] = 99;
                    }
                    else
                    {
                        if (x - (i + 1) > 0)
                            map.RoomTable[x - (i + 1), y] = 99;
                    }
                }

                if (ct % 2 == 0)
                    num -= 1;

                for (int i = 0; i < num; ++i)
                {
                    if (virt)
                    {
                        if (y + (i + 1) < map.h - 1)
                            map.RoomTable[x, y + (i + 1)] = 99;
                    }
                    else
                    {
                        if (x + (i + 1) < map.w - 1)
                            map.RoomTable[x + (i + 1), y] = 99;
                    }
                }
            };

            for (int i = 0; i < halls.Count; ++i)
            {
                HallwayInfo hall = halls[i];
                int w = Rand(hallSizes);

                int distX = Math.Abs(hall.endX - hall.startX);
                int distY = Math.Abs(hall.endY - hall.startY);
                int signX = Math.Sign(hall.endX - hall.startX);
                int signY = Math.Sign(hall.endY - hall.startY);

                int x = hall.startX;
                int y = hall.startY;
                if (rand.Next(0, 2) == 0)
                {
                    for (int xx = 0; xx < distX; ++xx)
                    {
                        x += signX;
                        map.RoomTable[x, y] = 99;
                        blitHall(map, true, w, x, y);
                    }
                    for (int yy = 0; yy < distY; ++yy)
                    {
                        y += signY;
                        map.RoomTable[x, y] = 99;
                        blitHall(map, false, w, x, y);
                    }
                }
                else
                {
                    for (int yy = 0; yy < distY; ++yy)
                    {
                        y += signY;
                        map.RoomTable[x, y] = 99;
                        blitHall(map, false, w, x, y);
                    }
                    for (int xx = 0; xx < distX; ++xx)
                    {
                        x += signX;
                        map.RoomTable[x, y] = 99;
                        blitHall(map, true, w, x, y);
                    }
                }
            }
        }

        public int Rand(int[] options)
        {
            return options[rand.Next(0, options.Length)];
        }

        public Room RandomRoom(Map map)
        {
            int idx = rand.Next(0, map.Rooms.Count);
            foreach (var val in map.Rooms)
            {
                if (idx == 0)
                    return val.Value;
                --idx;
            }
            return map.Rooms.First().Value;
        }
    }

    public static class MapExt
    { 
        public static void SortAxisGoingTowards(this List<Room> r, Axis axis)
        {
            if (axis == Axis.North)
            {
                r.Sort((a, b) => { 
                    if (a.Bottom < b.Bottom)
                        return -1;
                    if (a.Bottom > b.Bottom)
                        return 1;
                    return 0;
                });
            }
            else if (axis == Axis.South)
            {
                r.Sort((a, b) => {
                    if (a.Top > b.Top)
                        return -1;
                    if (a.Top < b.Top)
                        return 1;
                    return 0;
                });
            }
            else if (axis == Axis.East)
            {
                r.Sort((a, b) => {
                    if (a.Left < b.Left)
                        return -1;
                    if (a.Left > b.Left)
                        return 1;
                    return 0;
                });
            }
            else if (axis == Axis.West)
            {
                r.Sort((a, b) => {
                    if (a.Right > b.Right)
                        return -1;
                    if (a.Right < b.Right)
                        return 1;
                    return 0;
                });
            }
        }

        public static List<int> DistanceVertical(this List<Room> rooms, int d)
        {
            List<int> r = new List<int>();
            foreach (var room in rooms)
                r.Add(Math.Abs(room.CenterY - d));
            return r;
        }

        public static List<int> DistanceHorizontal(this List<Room> rooms, int d)
        {
            List<int> r = new List<int>();
            foreach (var room in rooms)
                r.Add(Math.Abs(room.CenterX - d));
            return r;
        }

        public static int MinIndex(this List<int> vec)
        {
            int min = int.MaxValue;
            int bestIndex = int.MaxValue;
            for (int i = 0; i < vec.Count; ++i)
            {
                if (min > vec[i])
                {
                    min = vec[i];
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        public static int MaxIndex(this List<int> vec)
        {
            int max = int.MinValue;
            int bestIndex = int.MaxValue;
            for (int i = 0; i < vec.Count; ++i)
            {
                if (max < vec[i])
                {
                    max = vec[i];
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
    }

}
