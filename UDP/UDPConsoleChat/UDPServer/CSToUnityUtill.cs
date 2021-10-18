using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE  || UNITY_ANDROID
using UnityEngine;
#endif

namespace CSToUnityUtill
{
    public static class Log
    {
        static public void WriteLine(string msg)
        {
#if UNITY_STANDALONE  || UNITY_ANDROID
            Debug.Log(msg);
#else
            Console.WriteLine(msg);
#endif
        }
        static public void WriteLine(string msg, object value)
        {
            string strLog = string.Format("{0}_{1}", msg, value);
#if UNITY_STANDALONE || UNITY_ANDROID
            Debug.Log(strLog);
#else
            Console.WriteLine(strLog);
#endif
        }
        static public void WriteLine(string msg, object value1, object value2)
        {
            string strLog = string.Format("{0}_{1},{2}", msg, value1, value2);
#if UNITY_STANDALONE || UNITY_ANDROID
            Debug.Log(strLog);
#else
            Console.WriteLine(strLog);
#endif
        }
        static public void WriteLine(string msg, object value1, object value2, object value3)
        {
            string strLog = string.Format("{0}_{1},{2},{3}", msg, value1, value2, value3);
#if UNITY_STANDALONE || UNITY_ANDROID
            Debug.Log(strLog);
#else
            Console.WriteLine(strLog);
#endif
        }
    }
}
