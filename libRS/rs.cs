using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libRS
{
    public class ReedSolomon
    {
        private int[] genPoly;
        private int[] pBytes;
        private berlekamp ber;
        private Galois ga;

        public ReedSolomon()
        {
            int i;
            int[] tp = new int[256];
            int[] tp1 = new int[256];
            genPoly = new int[Constants.MAXDEG * 2];
            pBytes = new int[Constants.MAXDEG];
            ga = new Galois();
            ber = new berlekamp(ga);

            /* multiply (x + a^n) for n = 1 to nbytes */

            berlekamp.zero_poly(ref tp1);
            tp1[0] = 1;

            for (i = 1; i <= Constants.NPAR; i++)
            {
                berlekamp.zero_poly(ref tp);
                tp[0] = ga.gexp[i];       /* set up x+a^n */
                tp[1] = 1;

                ber.mult_polys(ref genPoly, tp, tp1);
                berlekamp.copy_poly(ref tp1, genPoly);
            }
        }
        static void zero_fill_from(ref byte[] buf, int from, int to)
        {
            int i;
            for (i = from; i < to; i++) buf[i] = 0;
        }

        public void build_codeword(byte[] msg, int nbytes,ref byte[] dst)
        {
            int i;

            for (i = 0; i < nbytes; i++) dst[i] = msg[i];

            for (i = 0; i < Constants.NPAR; i++)
            {
                dst[i + nbytes] = (byte)pBytes[Constants.NPAR - 1 - i];
            }
        }

        public void decode_data(byte[] data, int nbytes)
        {
            int i, j, sum;
            for (j = 0; j < Constants.NPAR; j++)
            {
                sum = 0;
                for (i = 0; i < nbytes; i++)
                {
                    sum = data[i] ^ ga.gmult(ga.gexp[j + 1], sum);
                }
                ber.synBytes[j] = sum;
            }
        }

        public int check_syndrome()
        {
            int i, nz = 0;
            for (i = 0; i < Constants.NPAR; i++)
            {
                if (ber.synBytes[i] != 0)
                {
                    nz = 1;
                    break;
                }
            }
            return nz;
        }

        public void encode_data(byte[] msg, int nbytes, ref byte[] dst)
        {
            int i;
            int[] LFSR = new int[Constants.NPAR + 1];
            int dbyte, j;

            for (i = 0; i < Constants.NPAR + 1; i++) LFSR[i] = 0;

            for (i = 0; i < nbytes; i++)
            {
                dbyte = msg[i] ^ LFSR[Constants.NPAR - 1];
                for (j = Constants.NPAR - 1; j > 0; j--)
                {
                    LFSR[j] = LFSR[j - 1] ^ ga.gmult(genPoly[j], dbyte);
                }
                LFSR[0] = ga.gmult(genPoly[0], dbyte);
            }

            for (i = 0; i < Constants.NPAR; i++)
                pBytes[i] = LFSR[i];

            build_codeword(msg, nbytes,ref dst);
        }

        public int correct_errors_erasures(ref byte[] codeword, int csize, int nerasures, int[] erasures)
        {
            return ber.correct_errors_erasures(ref codeword, csize, nerasures, erasures);
        }
    }
   }
