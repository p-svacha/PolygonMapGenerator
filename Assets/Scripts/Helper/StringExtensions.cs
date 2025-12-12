using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StringExtensions
{
    /// <summary>
    /// Capitalizes the first letter of the string, taking into account HTML-like tags at the start.
    /// </summary>
    /// <param name="str">The string whose first character should be capitalized.</param>
    /// <returns>
    /// The input string with its first character converted to upper case, or the original
    /// string if it is null, empty, or already capitalized.
    /// </returns>
    public static string CapitalizeFirst(this string str)
    {
        // Check for null or empty string
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        // Check if first letter is already capitalized
        if (char.IsUpper(str[0]))
        {
            return str;
        }

        // Handle single character strings
        if (str.Length == 1)
        {
            return str.ToUpper();
        }

        // Get the first letter index between tags if applicable
        int num = str.FirstLetterBetweenTags();

        // If the first letter is the starting character
        if (num == 0)
        {
            return char.ToUpper(str[num]) + str.Substring(num + 1);
        }

        // Otherwise, capitalize the first letter after the specified index
        return str.Substring(0, num) + char.ToUpper(str[num]) + str.Substring(num + 1);
    }

    /// <summary>
    /// Finds the index of the first letter in the string, skipping over an opening tag if present.
    /// </summary>
    /// <param name="str">The string to examine for a leading tag.</param>
    /// <returns>
    /// The index of the first character to capitalize; zero if no leading tag is found,
    /// or the position immediately after the closing '>' of the first tag.
    /// </returns>
    public static int FirstLetterBetweenTags(this string str)
    {
        int num = 0;
        if (str[num] == '<' && str.IndexOf('>') > num && num < str.Length - 1 && str[num + 1] != '/')
        {
            num = str.IndexOf('>') + 1;
        }
        return num;
    }

    /// <summary>
    /// Capitalizes the first letter of each word in the string.
    /// </summary>
    /// <param name="str">The string whose words should be capitalized.</param>
    /// <returns>
    /// A new string with each word's first character converted to upper case.
    /// If the input is null or empty, the original string is returned.
    /// </returns>
    public static string CapitalizeEachWord(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        // Split on whitespace to identify words
        var words = str.Split(new[] { ' ' }, StringSplitOptions.None);
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = words[i].CapitalizeFirst();
        }

        // Recombine into a single string
        return string.Join(" ", words);
    }
}
