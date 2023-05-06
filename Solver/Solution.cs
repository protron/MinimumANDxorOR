using System;
using System.Linq;

public class Solution
{
    public static long solveOne(long ai, long aj)
    {
        return (ai & aj) ^ (ai | aj);
    }

    public static void Main()
    {
        var t = int.Parse(Console.ReadLine());
        while (t-- > 0)
        {
            Console.ReadLine();
            var line = Console.ReadLine();
            var values = line.Split(new[] { ' ' }).Select(x => long.Parse(x));
            var result = long.MaxValue;
            long? ai = null;
            foreach (var aj in values)
            {
                if (ai != null)
                {
                    var current = solveOne(ai.Value, aj);
                    if (current < result)
                    {
                        result = current;
                    }
                }
                ai = aj;
            }
            Console.WriteLine(result);
        }
    }
}
