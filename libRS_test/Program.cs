using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libRS;

namespace libRS_test
{
    class Program
    {
        static void Main(string[] args)
        {
            ReedSolomon coder = new ReedSolomon();
            ReedSolomon decoder = new ReedSolomon();
            byte[] message = new byte[Constants.Blockdata];
            message[0] = 250;
            message[1] = 1;
            message[2] = 3;
            byte[] encoded = new byte[Constants.Blocksize];
            coder.encode_data(message, message.Length,ref encoded);
            encoded[2] = 1;
            encoded[1] = 14;

            decoder.decode_data(encoded, encoded.Length);
            if(decoder.check_syndrome() != 0)
            {
                decoder.correct_errors_erasures(ref encoded, encoded.Length, 0, null);
            }

        }
    }
}
