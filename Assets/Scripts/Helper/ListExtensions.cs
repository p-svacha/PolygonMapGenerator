using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ListExtensions
{
    /// <summary>
    /// Returns a random element from the list using UnityEngine.Random. Optionally removes the selected element from the list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to select a random element from.</param>
    /// <param name="removeElement">If true, the selected element is removed from the list.</param>
    /// <returns>A random element from the list.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the list is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the list is empty.</exception>
    public static T RandomElement<T>(this List<T> list, bool removeElement = false)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list), "The list cannot be null.");

        if (list.Count == 0)
            throw new InvalidOperationException("Cannot select a random element from an empty list.");

        int index = UnityEngine.Random.Range(0, list.Count);
        T element = list[index];

        if (removeElement)
        {
            list.RemoveAt(index);
        }

        return element;
    }

    /// <summary>
    /// Returns a new list containing the specified number of elements randomly selected from the original list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to pick elements from.</param>
    /// <param name="amount">The number of random elements to select.</param>
    /// <returns>A list of <paramref name="amount"/> elements chosen at random.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="amount"/> is negative.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="amount"/> is greater than the number of elements in <paramref name="list"/>.</exception>
    public static List<T> RandomElements<T>(this List<T> list, int amount)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list), "The list cannot be null.");

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

        if (amount > list.Count)
            throw new ArgumentException("Amount cannot exceed the number of elements in the list.", nameof(amount));

        // Shuffle a copy and take the first 'amount' elements
        var shuffled = list.GetShuffledList();
        return shuffled.GetRange(0, amount);
    }

    /// <summary>
    /// Returns a new list with all elements shuffled randomly without modifying the original list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    /// <returns>A new list with all elements shuffled randomly.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the list is null.</exception>
    public static List<T> GetShuffledList<T>(this List<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list), "The list cannot be null.");

        List<T> shuffledList = new List<T>(list);
        for (int i = 0; i < shuffledList.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, shuffledList.Count);
            T temp = shuffledList[i];
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        return shuffledList;
    }

    /// <summary>
    /// Builds and returns a string representation of the list's contents.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list whose contents will be represented.</param>
    /// <param name="prefix">An optional prefix to prepend to each entry.</param>
    /// <param name="includeIndex">If true, includes the element's index in the output.</param>
    /// <returns>
    /// A single string with each element on its own line, prefixed and/or indexed as specified.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the list is null.</exception>
    public static string DebugList<T>(this List<T> list, string prefix = "", bool includeIndex = false)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list), "The list cannot be null.");

        var sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            if (includeIndex)
                sb.AppendLine($"{prefix}[{i}]: {list[i]}");
            else
                sb.AppendLine($"{prefix}{list[i]}");
        }

        return sb.ToString();
    }
}
