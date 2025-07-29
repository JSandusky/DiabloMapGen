using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiabloMapGen
{
    public struct MapRect
    {
        public int x;
        public int y;
        public int w;
        public int h;

        public int Left { get { return x;} }
        public int Right { get { return x + w; } }
        public int Top { get { return y;} }
        public int Bottom {  get { return y + h;} }

        public MapRect(Map map)
        {
            x = map.x;
            y = map.y;
            w = map.w;
            h = map.h;
        }

        public MapRect(Room r)
        {
            x = r.x;
            y = r.y;
            w = r.w;
            h = r.h;
        }

        public MapRect(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        public bool Overlaps(MapRect other)
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

        public void Partition(out MapRect topLeft, out MapRect topRight, out MapRect bottomleft, out MapRect bottomRight)
        {
            int halfW = w / 2;
            int halfH = h / 2;
            topLeft = new MapRect(x, y, halfW, halfH);
            topRight = new MapRect(x + halfW, y, halfW - 1, halfH);
            bottomleft = new MapRect(x, y + halfH, halfW, halfH - 1);
            bottomRight = new MapRect(x + halfW, y + halfH, halfW - 1, halfH - 1);
        }

        public void PartitionAround(int cx, int cy, out MapRect topLeft, out MapRect topRight, out MapRect bottomleft, out MapRect bottomRight)
        {
            int leftSize = cx - x;
            int topSize = cy - y;
            int rightSize = w - leftSize;
            int bottomSize = h - topSize;

            topLeft = new MapRect(x, y, leftSize, topSize);
            topRight = new MapRect(x + leftSize, y, rightSize, topSize);
            bottomleft = new MapRect(x, y + topSize, leftSize, bottomSize);
            bottomRight = new MapRect(x + leftSize, y + topSize, rightSize, bottomSize);
        }

        public void PartitionAroundAxis(int cx, int cy, out MapRect left, out MapRect top, out MapRect right, out MapRect bottom)
        {
            int leftSize = cx - x;
            int topSize = cy - y;
            int rightSize = w - leftSize;
            int bottomSize = h - topSize;

            left = new MapRect(x, y, leftSize, h);
            right = new MapRect(x + leftSize, y, rightSize, h);
            top = new MapRect(x, y, w, topSize);
            bottom = new MapRect(x, y + topSize, w, bottomSize);
        }

        public MapRect Shrink(int dim)
        {
            return new MapRect(x + dim, y + dim, w - dim - dim, h - dim - dim);
        }
    }
}
