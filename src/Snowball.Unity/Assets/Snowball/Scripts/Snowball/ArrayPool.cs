using System;
using System.Collections.Generic;

namespace Snowball
{
    public abstract class ArrayPool<T>
    {
        public static ArrayPool<T> Create() { return new SimpleArrayPool<T>(); }

        public static ArrayPool<T> Create(int maxArrayLength, int maxArraysPerBucket)
        {
            return new SimpleArrayPool<T>(maxArrayLength, maxArraysPerBucket);
        }
           

        public abstract T[] Rent(int minimumLength);
        public abstract void Return(T[] array, bool clearArray = false);
    }

    internal sealed class SimpleArrayPool<T> : ArrayPool<T>
    {
        public int maxArrayLength;
        public int maxArraysPerBucket;

        public Queue<T[]>[] buckets;

        public SimpleArrayPool(int maxArrayLength = 4098, int maxArraysPerBucket = 256)
        {
            this.maxArrayLength = maxArrayLength;
            this.maxArraysPerBucket = maxArraysPerBucket;

            int numBuckets = calculateIndexFromSize(maxArrayLength) + 1;
            buckets = new Queue<T[]>[numBuckets];

            for(int i = 0; i < numBuckets; i++)
            {
                buckets[i] = new Queue<T[]>();
            }
        }

        int calculateIndexFromSize(int size)
        {
            return (size + 7) >> 3;
        }

        int calculateSizeFromIndex(int index)
        {
            return (index) << 3;
        }


        public override T[] Rent(int minimumLength)
        {
            if (minimumLength > maxArrayLength) return null;

            int index = calculateIndexFromSize(minimumLength);

            lock (this)
            {
                if (buckets[index].Count > 0)
                {
                    T[] stored = buckets[index].Peek();
                    buckets[index].Dequeue();
                    return stored;
                }

                if (buckets[index].Count > maxArraysPerBucket) return null;
            }


            int size = calculateSizeFromIndex(index);
            T[] ret = new T[size];
            return ret;
        }

        public override void Return(T[] array, bool clearArray = false)
        {
            if (clearArray)
            {
                Array.Clear(array, 0, array.Length);
            }
            int index = calculateIndexFromSize(array.Length);

            lock (this)
            {
                buckets[index].Enqueue(array);
            }
        }
    }
}
