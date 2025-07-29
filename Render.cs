using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;

namespace DiabloMapGen
{
    public class Render
    {
        const int TOP = 0;
        const int CENTER = 1;
        const int BOT = 2;

        const int LEFT = 0;
        const int RIGHT = 2;

        static List<Color> colors = new List<Color>();

        static Color Darken(Color inCol)
        {
            return Color.FromArgb(
                inCol.R / 2,
                inCol.G / 2,
                inCol.B / 2
                );
        }

        static void InitColors()
        {
            if (colors.Count > 0)
                return;

            var colorFields = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (var f in colorFields)
            {
                if (f.Name == "Black")
                    continue;
                colors.Add((Color)f.GetValue(null));
            }
            Shuffle(colors);
        }

        public static void Draw(Map map, string outPath)
        {
            InitColors();
            System.Drawing.Bitmap img = new Bitmap(map.w * 2, map.h * 2, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // clear to black
            for (int x = 0; x < map.w * 2; ++x)
                for (int y = 0; y < map.h * 2; ++y)
                    img.SetPixel(x,y,Color.Black);

            for (int mx = 0; mx < map.w; ++mx)
            {
                for (int my = 0; my < map.h; ++my)
                {
                    if (map.RoomTable[mx,my] <= 0)
                        continue;

                    bool[,] solidity = map.GetNeightbors_RoomID(mx, my);

                    // is everyone around us good?
                    int solidCt = 0;
                    for (int x = 0; x < solidity.GetLength(0); ++x)
                        for (int y = 0; y < solidity.GetLength(1); ++y)
                            solidCt += solidity[x,y] == false ? 1 : 0;
                    if (solidCt == 8)
                        break;
                    
                    // TOP-LEFT
                    if (solidity[TOP,LEFT] == false || solidity[TOP,CENTER] == false)
                        img.SetPixel(mx*2,my*2,Color.Yellow);

                    // TOP-RIGHT
                    if (solidity[TOP, RIGHT] == false || solidity[TOP,CENTER] == false)
                        img.SetPixel(mx*2+1, my*2, Color.Yellow);

                    // BOTTOM-LEFT
                    if (solidity[BOT,LEFT] == false || solidity[BOT,CENTER] == false)
                        img.SetPixel(mx*2, my*2 + 1, Color.Yellow);

                    // BOTTOM-RIGHT
                    if (solidity[BOT, RIGHT] == false || solidity[BOT, CENTER] == false)
                        img.SetPixel(mx * 2 + 1, my * 2 + 1, Color.Yellow);

                    if (solidity[CENTER,LEFT] == false)
                    {
                        img.SetPixel(mx * 2, my * 2, Color.Yellow);
                        img.SetPixel(mx * 2, my * 2 + 1, Color.Yellow);
                    }
                    if (solidity[CENTER, RIGHT] == false)
                    {
                        img.SetPixel(mx * 2 + 1, my * 2, Color.Yellow);
                        img.SetPixel(mx * 2 + 1, my * 2 + 1, Color.Yellow);
                    }
                }
            }

            for (int mx = 0; mx < map.w; ++mx)
            {
                for (int my = 0; my < map.h; ++my)
                {
                    for (int x = 0; x < 2; ++x)
                    {
                        for (int y = 0; y < 2; ++y)
                        {
                            var pix = img.GetPixel(mx*2 + x, my*2 + y);
                            if (pix.R == Color.Yellow.R && pix.G == Color.Yellow.G && pix.B == Color.Yellow.B)
                                continue;
                            {
                                if (map.RoomTable[mx,my] > 0)
                                {
                                    img.SetPixel(mx*2 + x, my*2 + y, Darken(colors[map.RoomTable[mx, my] % colors.Count]));
                                }
                            }
                        }
                    }
                }
            }

            img.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void Shuffle(List<Color> list)
        {
            Random rng = new Random(3254);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Color value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void DrawRoomID(Map map, string outPath)
        {
            InitColors();

            System.Drawing.Bitmap img = new Bitmap(map.w*2, map.h*2, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // clear to black
            for (int x = 0; x < map.w; ++x)
                for (int y = 0; y < map.h; ++y)
                    img.SetPixel(x, y, Color.Black);

            for (int mx = 0; mx < map.w; ++mx)
            {
                for (int my = 0; my < map.h; ++my)
                {
                    if (map.RoomTable[mx,my] > 0)
                    {
                        img.SetPixel(mx*2,my*2, colors[map.RoomTable[mx,my] % colors.Count]);
                        img.SetPixel(mx * 2+1, my * 2, colors[map.RoomTable[mx, my] % colors.Count]);
                        img.SetPixel(mx * 2, my * 2+1, colors[map.RoomTable[mx, my] % colors.Count]);
                        img.SetPixel(mx * 2+1, my * 2+1, colors[map.RoomTable[mx, my] % colors.Count]);
                    }
                }
            }

            img.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void DrawRoomGeneration(Map map, string outPath)
        {
            InitColors();

            System.Drawing.Bitmap img = new Bitmap(map.w * 2, map.h * 2, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int maxGen = 0;
            foreach (var r in map.Rooms)
                maxGen = Math.Max(maxGen, r.Value.Gen);

            // clear to black
            for (int x = 0; x < map.w; ++x)
                for (int y = 0; y < map.h; ++y)
                    img.SetPixel(x, y, Color.Black);

            for (int mx = 0; mx < map.w; ++mx)
            {
                for (int my = 0; my < map.h; ++my)
                {
                    if (map.RoomTable[mx, my] > 0)
                    {
                        Room r = map.Rooms[map.RoomTable[mx,my]];
                        float fraction = 0.25f + ((float)r.Gen / (float)maxGen) * 0.75f;
                        int c = (int)(255 * fraction);
                        if (c < 0)
                            c = 0;
                        Color col = Color.FromArgb(c,c,c);

                        img.SetPixel(mx * 2, my * 2, col);
                        img.SetPixel(mx * 2 + 1, my * 2, col);
                        img.SetPixel(mx * 2, my * 2 + 1, col);
                        img.SetPixel(mx * 2 + 1, my * 2 + 1, col);
                    }
                }
            }

            img.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
