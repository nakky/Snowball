using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Snowball
{
    public static class Util
    {
        public static void Log(string message)
        {
        #if UNITY_EDITOR
            Debug.Log(message);
        #else
            Console.WriteLine(message);
        #endif
        }

    }

}

