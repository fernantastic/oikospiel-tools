using System;

namespace AudioSynthesis.Util
{
    public static class Tables
    {
        public static readonly double[] SemitoneTable;
        public static readonly double[] CentTable;
         
        /*Creates tables in static constructor*/
        static Tables()
        {
            //const int EnvelopeSize = 128;
            CentTable = CreateCentTable();
            SemitoneTable = CreateSemitoneTable();
        }

        /*Cent table contains 2^12 ratio for pitches in the range of (-1 to +1) semitone.
          Accuracy between semitones is 1/100th of a note or 1 cent. */
        public static double[] CreateCentTable()
        {//-100 to 100 cents
            double[] cents = new double[201];
            for(int x = 0; x < cents.Length; x++)
            {
                cents[x] = Math.Pow(2.0, (x - 100.0) / 1200.0);
            }
            return cents;
        }
             
        /*Semitone table contains pitches for notes in range of -127 to 127 semitones.
          Used to calculate base pitch when voice is started. ex. (basepitch = semiTable[midinote - rootkey]) */
        public static double[] CreateSemitoneTable()
        {//-127 to 127 semitones
            double[] table = new double[255];
            for (int x = 0; x < table.Length; x++)
            {
                table[x] = Math.Pow(2.0, (x - 127.0) / 12.0);
            }
            return table;
        }
        
        /*Envelope Equations*/
        public static float[] CreateSustainTable(int size)
        {
            float[] graph = new float[size];
            for (int x = 0; x < graph.Length; x++)
            {
                graph[x] = 1;
            }
            return graph;
        }
        public static float[] CreateLinearTable(int size)
        {
            float[] graph = new float[size];
            for (int x = 0; x < graph.Length; x++)
            {
                graph[x] = x / (float)(size - 1);
            }
            return graph;
        }
        public static float[] CreateConcaveTable(int size)
        {//follows sf2 spec
            float[] graph = new float[size];
            const double c = -(20.0 / 96.0);
            int max = (size - 1) * (size - 1);
            for (int x = 0; x < graph.Length; x++)
            {
                int i = (size - 1) - x;
                graph[x] = (float)(c * Math.Log10((i * i) / (double)max));
            }
            graph[size - 1] = 1f;
            return graph;
        }
        public static float[] CreateConvexTable(int size)
        {//follows sf2 spec
            float[] graph = new float[size];
            const double c = (20.0 / 96.0);
            int max = (size - 1) * (size - 1);
            for (int x = 0; x < graph.Length; x++)
            {
                graph[x] = (float)(1 + c * Math.Log10((x * x) / (double)max));
            }
            graph[0] = 0f;
            return graph;
        }
    }
}
