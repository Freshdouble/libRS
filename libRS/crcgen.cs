using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libRS
{
    /*
    class crcgen
    {
        public crcgen()
        {

        }

        public ushort crc_ccitt(byte[] msg, int len)
        {
            int i;
            ushort acc = 0;

            for (i = 0; i < len; i++)
            {
                acc = crchware(msg[i], 0x1021, acc);
            }

            return (acc);
        }

        public ushort crchware(ushort data, ushort genpoly, ushort accum)
        {
            ushort i;
            data <<= 8;
            for (i = 8; i > 0; i--)
            {
                if (((data ^ accum) & 0x8000) != 0)
                    accum = (ushort)(((accum << 1) ^ genpoly) & 0xFFFF);
                else
                    accum = (ushort)((accum << 1) & 0xFFFF);
                data = (ushort)((data << 1) & 0xFFFF);
            }
            return (accum);
        }
    }
    */
}
