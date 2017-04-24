using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libRS
{
    public static class Constants
    {
        public static int Blocksize = 16;

  /****************************************************************

  Below is NPAR, the only compile-time parameter you should have to
  modify.

  It is the number of parity bytes which will be appended to
  your data to create a codeword.

  Note that the maximum codeword size is 255, so the
  sum of your message length plus parity should be less than
  or equal to this maximum limit.

  In practice, you will get slooow error correction and decoding
  if you use more than a reasonably small number of parity bytes.
  (say, 10 or 20)

  ****************************************************************/

        public static int NPAR = 4;

        public static int Blockdata = Blocksize - NPAR;
        public static int MAXDEG = NPAR * 2;
    }
}
