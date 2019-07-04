using System;

namespace Snowball
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = true)]
    public class TransferableAttribute : Attribute
    {
        public TransferableAttribute()
        {
        }
    }

    [AttributeUsage(
        AttributeTargets.Field,
        AllowMultiple = false,
        Inherited = true)]
    public class DataAttribute : Attribute
    {
        public DataAttribute(int index)
        {
            this.Index = index;
        }

        public int Index { get; private set; }
    }
}
