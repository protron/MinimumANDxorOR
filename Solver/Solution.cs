using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main()
    {
        var t = int.Parse(Console.ReadLine());
        while (t-- > 0)
        {
            Console.ReadLine();
            var line = Console.ReadLine();
            var numberInputs = line
                .Split(new[] { ' ' })
                .Select(x => long.Parse(x))
                .OrderByDescending(x => x);
            var result = long.MaxValue;
            foreach (var pair in GetPairsWithPrevious(numberInputs))
            {
                var hypotesis = pair.Previous ^ pair.Current;
                if (hypotesis < result)
                {
                    result = hypotesis;
                }
            }
            Console.WriteLine(result);
        }
    }

    public static IEnumerable<(T Previous, T Current)> GetPairsWithPrevious<T>(IEnumerable<T> values)
    {
        using var e = values.GetEnumerator();
        if (!e.MoveNext()) yield break;
        var previous = e.Current;
        while (e.MoveNext())
        {
            yield return (previous, e.Current);
            previous = e.Current;
        }
    }

    /*
    public static IEnumerable<(T Previous, T Current)> GetPairsWithPrevious<T>(IEnumerable<T> values)
        where T : struct
    {
        T? previous = null;
        foreach (var current in values)
        {
            if (previous.HasValue)
            {
                yield return (previous.Value, current);
            }
            previous = current;
        }
    }
    */
}
