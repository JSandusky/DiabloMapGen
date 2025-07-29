using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiabloMapGen
{
    public class WaveSynth
    { 
        public int apex;
        public int freq;
        public int accum = 0;

        static Random rnd = new Random();
        public static float[] Generate(int freq, float duration, int sampleRate, float scale = 1.0f)
        {
            int elements = (int)Math.Ceiling(duration * sampleRate);
            float[] ret = new float[elements];
            float t = 0.0f;
            float td = duration / elements;

            float rand = 0.75f + ((float)rnd.NextDouble()) * 0.25f;

            for (int i = 0; i < elements; ++i)
            {
                float rand2 = 0.75f + ((float)rnd.NextDouble()) * 0.25f;
                ret[i] = (((float)Math.Cos(freq * t * 2.0f * Math.PI) * scale * (1.0f - (t / duration)) * rand) - 0.5f) * 2.0f * rand2;
                ret[i] *= (1.0f - (td/duration));
                //ret[i] = ((1.0f - () * rand) - 0.5f) * 2.0f;
                t += td;
            }
            return ret;
        }

    }


    public class MotorGen
    {
        const int RATE = 22050;

        public int Limiter = 4500;
        public int cylinderCount = 2;
        public float[] degrees;
        
        public int currentRPM = 0;

        public WaveSynth fire;
        public WaveSynth passive;

        System.IO.MemoryStream stream;
        Random rnd = new Random();

        public MotorGen(int cylinders, int maxRPM)
        {
            Limiter = maxRPM;
            cylinderCount = cylinders;
            degrees = new float[cylinderCount];
            float strokeHit = ((float)cylinderCount) / (cylinderCount + 1);
            float curStroke = 0.0f;
            for (int i = 0; i < cylinderCount; ++i)
            {
                degrees[i] = curStroke;
                curStroke += strokeHit;
            }
            stream = new System.IO.MemoryStream();
        }

        public void Generate(float seconds)
        {
            int totalRate = (int)(RATE * seconds);
            int ratePersec = 50;
            float timePerStep = (((float)ratePersec) / ((float)RATE));

            float td = 0.0f;

            float accumRevs = 0.0f;

            int skipNext = 0;
            float[] nullWrite = new float[(int)Math.Ceiling(RATE * timePerStep)];
            for (int i = 0; i < nullWrite.Length; ++i)
                nullWrite[i] = 0.0f;
            byte[] nullBytes = nullWrite.SelectMany(f => BitConverter.GetBytes(f)).ToArray();

            for (int i = 0; i < totalRate; i += ratePersec)
            {
                int strokesPerMin = currentRPM * 2;
                int strokesPerSec = strokesPerMin / 60;

                accumRevs += currentRPM * timePerStep;
                if (accumRevs > 0.25f && skipNext <= 0)
                { 
                    stream.Write(WaveSynth.Generate(16, timePerStep, RATE).SelectMany(f => BitConverter.GetBytes(f)).ToArray());
                    accumRevs = 0.0f;
                    skipNext += 5;
                }
                else
                {
                    //stream.Write(WaveSynth.Generate(2, timePerStep, RATE, 0.6f).SelectMany(f => BitConverter.GetBytes(f)).ToArray());
                    stream.Write(nullBytes);
                    --skipNext;
                }

                td += timePerStep;
                currentRPM += 100;
                if (currentRPM > Limiter)
                    currentRPM = Limiter;
            }
        }

        public void Save()
        {
            using (var f = new System.IO.FileStream("engine.raw", System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                stream.WriteTo(f);
            }
        }
    }
}
