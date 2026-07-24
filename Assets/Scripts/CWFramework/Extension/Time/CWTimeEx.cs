using System;

public static class CWTimeEx
{
    public static TimeSpan GetMaxTimeUnit(TimeSpan timeSpan)
    {
        switch (timeSpan)
        {
            case { Days: > 0 }:
                return TimeSpan.FromDays(1);
            case { Hours: > 0 }:
                return TimeSpan.FromHours(1);    
            case { Minutes: > 0 }:
                return TimeSpan.FromMinutes(1);
            default:
                return TimeSpan.FromSeconds(1);    
        }
    }
}
