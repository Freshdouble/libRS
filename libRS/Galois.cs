using System;

namespace libRS
{
    internal class Galois
    {
        public readonly int[] gexp;
        public readonly int[] glog;

        public Galois()
        {
            gexp = new int[512];
            glog = new int[256];
            init_exp_table();
        }

        private void init_exp_table()
        {
            int i, z;
            int pinit, p1, p2, p3, p4, p5, p6, p7, p8;

            pinit = p2 = p3 = p4 = p5 = p6 = p7 = p8 = 0;
            p1 = 1;

            gexp[0] = 1;
            gexp[255] = gexp[0];
            glog[0] = 0;           /* shouldn't log[0] be an error? */

            for (i = 1; i < 256; i++)
            {
                pinit = p8;
                p8 = p7;
                p7 = p6;
                p6 = p5;
                p5 = p4 ^ pinit;
                p4 = p3 ^ pinit;
                p3 = p2 ^ pinit;
                p2 = p1;
                p1 = pinit;
                gexp[i] = p1 + p2 * 2 + p3 * 4 + p4 * 8 + p5 * 16 + p6 * 32 + p7 * 64 + p8 * 128;
                gexp[i + 255] = gexp[i];
            }

            for (i = 1; i < 256; i++)
            {
                for (z = 0; z < 256; z++)
                {
                    if (gexp[z] == i)
                    {
                        glog[i] = z;
                        break;
                    }
                }
            }
        }

        public int gmult(int a, int b)
        {
            int i, j;
            if (a == 0 || b == 0) return (0);
            i = glog[a];
            j = glog[b];
            return (gexp[i + j]);
        }

        public int ginv(int elt)
        {
            return (gexp[255 - glog[elt]]);
        }
    }
}