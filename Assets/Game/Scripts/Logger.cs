using System.Diagnostics;

public class Logger
{
    [Conditional("DEBUG")]
    public static void Log(object InCaller, string InFormat, params object[] InArgs)
    {
        UnityEngine.Debug.Log(string.Format("[{0}] ", InCaller.GetType() != typeof(string) ? InCaller.GetType().Name : InCaller) + string.Format(InFormat, InArgs));
    }

    [Conditional("DEBUG")]
    public static void Warning(object InCaller, string InFormat, params object[] InArgs)
    {
        UnityEngine.Debug.LogWarning(string.Format("[{0}] ", InCaller.GetType() != typeof(string) ? InCaller.GetType().Name : InCaller) + string.Format(InFormat, InArgs));
    }

    [Conditional("DEBUG")]
    public static void Error(object InCaller, string InFormat, params object[] InArgs)
    {
        UnityEngine.Debug.LogError(string.Format("[{0}] ", InCaller.GetType() != typeof(string) ? InCaller.GetType().Name : InCaller) + string.Format(InFormat, InArgs));        
    }
}