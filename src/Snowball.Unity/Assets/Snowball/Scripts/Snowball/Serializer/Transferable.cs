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
        AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = true)]
    public class DataAttribute : Attribute
    {
        public DataAttribute(int index)
        {
            this.Index = index;
            this.ConverterType = null;
        }

        public DataAttribute(int index, Type converterType)
        {
            this.Index = index;
            this.ConverterType = converterType;
        }

        public int Index { get; private set; }
        public Type ConverterType { get; private set; }
    }
}
