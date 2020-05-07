using System;
using System.IO;

namespace Snowball
{
    public abstract class Converter
    {
        public abstract void Serialize(BytePacker packer, object data);
        public abstract object Deserialize(BytePacker packer);
        public abstract int GetDataSize(object data);
        public abstract int GetDataSize(BytePacker packer);
    }
}
