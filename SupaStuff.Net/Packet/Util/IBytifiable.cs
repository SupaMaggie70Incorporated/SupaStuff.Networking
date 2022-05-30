using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupaStuff.Net.Packet.Util
{
    /// <summary>
    /// The abstract class for bytifiable objects, such as packets, to ease the coding difficulty
    /// </summary>
    public abstract class IBytifiable
    {
        public virtual byte[] Bytify()
        {
            return Bytifier.Bytify(this);
        }
        public IBytifiable(byte[] bytes)
        {

        }
    }
}
