using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libRS
{
    internal class berlekamp
    {
        private int NErasures;
        private int NErrors;
        private int[] ErasureLocs;
        private int[] ErrorLocs;
        private int[] Lambda;
        internal int[] synBytes;
        private int[] Omega;
        private Galois ga;

        public berlekamp(Galois ga)
        {
            NErasures = 0;
            NErrors = 0;
            ErasureLocs = new int[256];
            Lambda = new int[Constants.MAXDEG];
            synBytes = new int[Constants.MAXDEG];
            Omega = new int[Constants.MAXDEG];
            ErrorLocs = new int[256];
            this.ga = ga;
        }

        internal void Modified_Berlekamp_Massey()
        {
            int n, L, L2, k, d, i;
            int[] psi = new int[Constants.MAXDEG];
            int[] psi2 = new int[Constants.MAXDEG];
            int[] D = new int[Constants.MAXDEG];
            int[] gamma = new int[Constants.MAXDEG];

            /* initialize Gamma, the erasure locator polynomial */
            init_gamma(ref gamma);

            /* initialize to z */
            copy_poly(ref D, gamma);
            mul_z_poly(ref D);

            copy_poly(ref psi, gamma);
            k = -1; L = NErasures;

            for (n = NErasures; n < Constants.NPAR; n++)
            {

                d = compute_discrepancy(psi, synBytes, L, n);

                if (d != 0)
                {

                    /* psi2 = psi - d*D */
                    for (i = 0; i < Constants.MAXDEG; i++) psi2[i] = psi[i] ^ ga.gmult(d, D[i]);


                    if (L < (n - k))
                    {
                        L2 = n - k;
                        k = n - L;
                        /* D = scale_poly(ginv(d), psi); */
                        for (i = 0; i < Constants.MAXDEG; i++) D[i] = ga.gmult(psi[i], ga.ginv(d));
                        L = L2;
                    }

                    /* psi = psi2 */
                    for (i = 0; i < Constants.MAXDEG; i++) psi[i] = psi2[i];
                }

                mul_z_poly(ref D);
            }

            for (i = 0; i < Constants.MAXDEG; i++) Lambda[i] = psi[i];
            compute_modified_omega();
        }

        internal void compute_modified_omega()
        {
            int i;
            int[] product = new int[Constants.MAXDEG * 2];

            mult_polys(ref product, Lambda, synBytes);
            zero_poly(ref Omega);
            for (i = 0; i < Constants.NPAR; i++)
                Omega[i] = product[i];

        }

        internal void mult_polys(ref int[] dst, int[] p1, int[] p2)
        {
            int i, j;
            int[] tmp1 = new int[Constants.MAXDEG * 2];

            for (i = 0; i < (Constants.MAXDEG * 2); i++) dst[i] = 0;

            for (i = 0; i < Constants.MAXDEG; i++)
            {
                for (j = Constants.MAXDEG; j < (Constants.MAXDEG * 2); j++) tmp1[j] = 0;

                /* scale tmp1 by p1[i] */
                for (j = 0; j < Constants.MAXDEG; j++) tmp1[j] = ga.gmult(p2[j], p1[i]);
                /* and mult (shift) tmp1 right by i */
                for (j = (Constants.MAXDEG * 2) - 1; j >= i; j--) tmp1[j] = tmp1[j - i];
                for (j = 0; j < i; j++) tmp1[j] = 0;

                /* add into partial product */
                for (j = 0; j < (Constants.MAXDEG * 2); j++) dst[j] ^= tmp1[j];
            }
        }

        internal void compute_next_omega(int d, int[] A, ref int[] dst, int[] src)
        {
            int i;
            for (i = 0; i < Constants.MAXDEG; i++)
            {
                dst[i] = src[i] ^ ga.gmult(d, A[i]);
            }
        }

        private int compute_discrepancy(int[] lambda, int[] S, int L, int n)
        {
            int i, sum = 0;

            for (i = 0; i <= L; i++)
                sum ^= ga.gmult(lambda[i], S[n - i]);
            return (sum);
        }

        private void init_gamma(ref int[] gamma)
        {
            int[] tmp = new int[Constants.MAXDEG];
            int e;

            zero_poly(ref gamma);
            zero_poly(ref tmp);
            gamma[0] = 1;

            for (e = 0; e < NErasures; e++)
            {
                copy_poly(ref tmp, gamma);
                scale_poly(ga.gexp[ErasureLocs[e]], ref tmp);
                mul_z_poly(ref tmp);
                add_polys(ref gamma, tmp);
            }
        }

        static internal void mul_z_poly(ref int[] src)
        {
            int i;
            for (i = Constants.MAXDEG - 1; i > 0; i--)
                src[i] = src[i - 1];
            src[0] = 0;
        }

        static internal void add_polys(ref int[] dst, int[] src)
        {
            int i;
            for (i = 0; i < Constants.MAXDEG; i++) dst[i] ^= src[i];
        }

        static internal void copy_poly(ref int[] dst, int[] src)
        {
            int i;
            for (i = 0; i < Constants.MAXDEG; i++) dst[i] = src[i];
        }

        internal void scale_poly(int k, ref int[] poly)
        {
            int i;
            for (i = 0; i < Constants.MAXDEG; i++) poly[i] = ga.gmult(k, poly[i]);
        }

        static internal void zero_poly(ref int[] poly)
        {
            int i;
            for (i = 0; i < Constants.MAXDEG; i++) poly[i] = 0;
        }

        private void Find_Roots()
        {
            int sum, r, k;
            NErrors = 0;

            for (r = 1; r < 256; r++)
            {
                sum = 0;
                /* evaluate lambda at r */
                for (k = 0; k < Constants.NPAR + 1; k++)
                {
                    sum ^= ga.gmult(ga.gexp[(k * r) % 255], Lambda[k]);
                }
                if (sum == 0)
                {
                    ErrorLocs[NErrors] = (255 - r); NErrors++;
                }
            }
        }

        public int correct_errors_erasures(ref byte[] codeword, int csize, int nerasures, int[] erasures)
        {
            int r, i, j, err;

            /* If you want to take advantage of erasure correction, be sure to
               set NErasures and ErasureLocs[] with the locations of erasures.
               */
            NErasures = nerasures;
            for (i = 0; i < NErasures; i++) ErasureLocs[i] = erasures[i];

            Modified_Berlekamp_Massey();
            Find_Roots();


            if ((NErrors <= Constants.NPAR) && NErrors > 0)
            {

                /* first check for illegal error locs */
                for (r = 0; r < NErrors; r++)
                {
                    if (ErrorLocs[r] >= csize)
                    {
                        return (0);
                    }
                }

                for (r = 0; r < NErrors; r++)
                {
                    int num, denom;
                    i = ErrorLocs[r];
                    /* evaluate Omega at alpha^(-i) */

                    num = 0;
                    for (j = 0; j < Constants.MAXDEG; j++)
                        num ^= ga.gmult(Omega[j], ga.gexp[((255 - i) * j) % 255]);

                    /* evaluate Lambda' (derivative) at alpha^(-i) ; all odd powers disappear */
                    denom = 0;
                    for (j = 1; j < Constants.MAXDEG; j += 2)
                    {
                        denom ^= ga.gmult(Lambda[j], ga.gexp[((255 - i) * (j - 1)) % 255]);
                    }

                    err = ga.gmult(num, ga.ginv(denom));

                    codeword[csize - i - 1] = (byte)(codeword[csize - i - 1] ^ err);
                }
                return (1);
            }
            else
            {
                return (0);
            }
        }
    }
}
