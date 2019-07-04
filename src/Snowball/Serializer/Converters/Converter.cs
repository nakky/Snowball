using System;
using System.IO;

namespace Snowball
{
    public abstract class Converter
    {
        public abstract void Serialize(Stream stream, object data);
        public abstract object Deserialize(Stream stream);
    }
}
