using UnityEngine;

public static class IntegerExtensions
{
    public static string ToOrdinal(this int number)
    {
        int abs = Mathf.Abs(number);

        // Special cases: 11th, 12th, 13th
        if (abs % 100 >= 11 && abs % 100 <= 13)
            return number + "th";

        return (abs % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th"
        };
    }
}
