using System;
using System.IO;

namespace Snowball
{
    public interface IConverter
    {
        void Serialize(BytePacker packer, object data);
        object Deserialize(BytePacker packer);
        int GetDataSize(object data);
        int GetDataSize(BytePacker packer);
    }
}
