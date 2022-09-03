using System;

namespace uav.logic.Extensions;

public static class ValidationExceptions
{
    public static T ValidateNotNull<T>(this T obj, string name) where T: class
    {
        if (obj is null)
        {
            throw new ArgumentNullException($"{name} is null");
        }
        return obj;
    }
}