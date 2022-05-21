using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupaStuff.Net.Packet.Util
{
    public abstract class IBytifiable
    {
        public byte[] Bytify()
        {
            return Bytifier.Bytify(this);
        }
        public IBytifiable(byte[] bytes)
        {

        }
    }
}
