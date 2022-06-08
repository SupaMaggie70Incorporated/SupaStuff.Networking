using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupaStuff.Net.Packets.Util
{
    /// <summary>
    /// The abstract class for bytifiable objects, such as packets, to ease the coding difficulty
    /// </summary>
    internal abstract class IBytifiable
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
