using System;
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
            var values = line.Split(new[] { ' ' }).Select(x => long.Parse(x)).ToList();
            values.Sort();
            values.Reverse();
            var result = long.MaxValue;
            var enumerator = values.GetEnumerator();
            long ai = enumerator.Current;
            long aj, current;
            while (enumerator.MoveNext())
            {
                aj = enumerator.Current;
                current = ai ^ aj;
                if (current < result)
                {
                    result = current;
                }
                ai = aj;
            }
            Console.WriteLine(result);
        }
    }
}
