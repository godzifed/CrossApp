using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class LinqExtensions
{
    // Додаткове завдання 2: Власний метод-розширення TopBy
    public static IEnumerable<T> TopBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, int count)
    {
        return source.OrderByDescending(keySelector).Take(count);
    }
}