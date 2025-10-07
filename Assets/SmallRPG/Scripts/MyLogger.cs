using System.Diagnostics;
using UnityEngine;

namespace SmallRPG
{
    public static class MyLogger
    {
        [Conditional("DEBUG")]
        // [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            
            UnityEngine.Debug.Log(message);
        }

        [Conditional("DEBUG")]
        // [Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("DEBUG")]
        // [Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}


