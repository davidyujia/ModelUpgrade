using System;
using System.Collections.Generic;

namespace ModelUpgrade.Extensions
{
    /// <summary>
    /// Model upgrade extension
    /// </summary>
    public static class ModelUpgradeExtension
    {
        internal static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }
    }
}
