using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnalyticsLogger
{
    void LogLevelStart(int level);
    void LogLevelComplete(int level);
}

public class AnalyticsController : MonoBehaviour, IAnalyticsLogger
{
    public void LogLevelStart(int level)
    {
        
    }

    public void LogLevelComplete(int level)
    {
        
    }
}