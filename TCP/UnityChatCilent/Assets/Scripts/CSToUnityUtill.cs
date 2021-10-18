using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE
using UnityEngine;
#endif


namespace CSToUnityUtill
{
    public static class Log
    {
        static public void WriteLine(string msg)
        {
#if UNITY_STANDALONE
            Debug.Log(msg);
#else
            Console.WriteLine(msg);
#endif
        }
        static public void WriteLine(string msg, object value)
        {
#if UNITY_STANDALONE
            Debug.Log(string.Format("{0}{1}",msg,value));
#else
            Console.WriteLine(msg,value);
#endif
        }
        static public void WriteLine(string msg, object value1, object value2)
        {
#if UNITY_STANDALONE
            Debug.Log(string.Format("{0}{1}{2}", msg, value1, value2));
#else
            Console.WriteLine(msg,value1,value2);
#endif
        }
        static public void WriteLine(string msg, object value1, object value2, object value3)
        {
#if UNITY_STANDALONE
            Debug.Log(string.Format("{0}{1}{2}{3}", msg, value1, value2, value3));
#else
            Console.WriteLine(msg,value1,value1,value3);
#endif
        }
    }
}
