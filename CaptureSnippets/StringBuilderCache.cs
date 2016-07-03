using System;
using System.Text;

internal static class StringBuilderCache
{
    const int MAX_BUILDER_SIZE = 360;

    [ThreadStatic]
    static StringBuilder CachedInstance;

    public static StringBuilder Acquire(int capacity = 16)
    {
        if (capacity <= MAX_BUILDER_SIZE)
        {
            var builder = CachedInstance;
            if (builder != null)
            {
                // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                // when the requested size is larger than the current capacity
                if (capacity <= builder.Capacity)
                {
                    CachedInstance = null;
                    builder.Clear();
                    return builder;
                }
            }
        }
        return new StringBuilder(capacity);
    }

    public static void Release(StringBuilder sb)
    {
        if (sb.Capacity <= MAX_BUILDER_SIZE)
        {
            CachedInstance = sb;
        }
    }

    public static string GetStringAndRelease(StringBuilder builder)
    {
        var result = builder.ToString();
        Release(builder);
        return result;
    }
}