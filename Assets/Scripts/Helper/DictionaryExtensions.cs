using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DictionaryExtensions
{
    /// <summary>
    /// Selects a random key from the dictionary based on integer weights.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="weightDictionary">Dictionary mapping keys to integer weights.</param>
    /// <returns>A randomly selected key, where probability is proportional to its weight.</returns>
    /// <exception cref="Exception">Thrown if the dictionary is empty or all weights are zero.</exception>
    public static TKey GetWeightedRandomElement<TKey>(this Dictionary<TKey, int> weightDictionary)
    {
        int probabilitySum = weightDictionary.Sum(x => x.Value);
        int rng = UnityEngine.Random.Range(0, probabilitySum);
        int tmpSum = 0;
        foreach (var kvp in weightDictionary)
        {
            tmpSum += kvp.Value;
            if (rng < tmpSum)
                return kvp.Key;
        }
        throw new Exception("No element selected. Check the dictionary for valid weights.");
    }

    /// <summary>
    /// Selects a random key from the dictionary based on float weights.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="weightDictionary">Dictionary mapping keys to float weights.</param>
    /// <returns>A randomly selected key, where probability is proportional to its weight.</returns>
    /// <exception cref="Exception">Thrown if the dictionary is empty or all weights are zero.</exception>
    public static TKey GetWeightedRandomElement<TKey>(this Dictionary<TKey, float> weightDictionary)
    {
        float probabilitySum = weightDictionary.Sum(x => x.Value);
        float rng = UnityEngine.Random.Range(0, probabilitySum);
        float tmpSum = 0;
        foreach (var kvp in weightDictionary)
        {
            tmpSum += kvp.Value;
            if (rng < tmpSum)
                return kvp.Key;
        }
        throw new Exception("No element selected. Check the dictionary for valid weights.");
    }

    /// <summary>
    /// Increments the integer value associated with the specified key by a given amount.
    /// If the key does not exist, it is added with the increment amount as its initial value.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to operate on.</param>
    /// <param name="key">The key whose value to increment.</param>
    /// <param name="amount">The amount by which to increment the value (defaults to 1).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="dictionary"/> or <paramref name="key"/> is null.
    /// </exception>
    public static void Increment<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int amount = 1)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (dictionary.TryGetValue(key, out int current))
        {
            dictionary[key] = current + amount;
        }
        else
        {
            dictionary[key] = amount;
        }
    }

    /// <summary>
    /// Increments the integer values in this dictionary by the corresponding values in another dictionary.
    /// Entries in <paramref name="other"/> that do not exist in this dictionary are added.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionaries.</typeparam>
    /// <param name="dictionary">The dictionary whose values will be updated.</param>
    /// <param name="other">The dictionary providing increment amounts by key.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="dictionary"/> or <paramref name="other"/> is null.
    /// </exception>
    public static void IncrementMultiple<TKey>(this IDictionary<TKey, int> dictionary, IDictionary<TKey, int> other)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        foreach (var kvp in other)
        {
            dictionary.Increment(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Decrements the integer value associated with the specified key by a given amount.
    /// If the key does not exist or its value is less than the decrement amount when negative values are disallowed, an exception is thrown.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to operate on.</param>
    /// <param name="key">The key whose value to decrement.</param>
    /// <param name="amount">The amount by which to decrement the value (defaults to 1).</param>
    /// <param name="allowNegativeValues">If true, allows values to go below zero; otherwise values are not permitted to be negative.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="dictionary"/> or <paramref name="key"/> is null.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if the key does not exist, or if the key's current value is less than <paramref name="amount"/> when <paramref name="allowNegativeValues"/> is false.
    /// </exception>
    public static void Decrement<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int amount = 1, bool allowNegativeValues = false)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (dictionary.ContainsKey(key))
        {
            if (!allowNegativeValues && dictionary[key] < amount)
                throw new Exception($"Key {key} is not allowed to be <{amount} when trying to decrement {amount}, but is {dictionary[key]}");
            dictionary[key] -= amount;
        }
        else
        {
            throw new Exception($"Key {key} doesn't exist.");
        }
    }

    /// <summary>
    /// Decrements the integer values in this dictionary by the corresponding values in another dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionaries.</typeparam>
    /// <param name="dictionary">The dictionary whose values will be updated.</param>
    /// <param name="other">The dictionary providing decrement amounts by key.</param>
    /// <param name="allowNegativeValues">If true, allows values to go below zero; otherwise values are not permitted to be negative.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="dictionary"/> or <paramref name="other"/> is null.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if any key in <paramref name="other"/> does not exist in <paramref name="dictionary"/>, or if a decremented value would go below zero when <paramref name="allowNegativeValues"/> is false.
    /// </exception>
    public static void DecrementMultiple<TKey>(this Dictionary<TKey, int> dictionary, IDictionary<TKey, int> other, bool allowNegativeValues = false)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        foreach (var kvp in other)
        {
            dictionary.Decrement(kvp.Key, kvp.Value, allowNegativeValues);
        }
    }

    /// <summary>
    /// Modifies the integer values in this dictionary based on the values in another dictionary.
    /// Positive values in <paramref name="other"/> will increment, and negative values will decrement.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionaries.</typeparam>
    /// <param name="dictionary">The dictionary whose values will be modified.</param>
    /// <param name="other">The dictionary providing modification amounts by key (positive to increment, negative to decrement).</param>
    /// <param name="allowNegativeValues">If true, allows values to go below zero when decrementing; otherwise negative results are disallowed.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="dictionary"/> or <paramref name="other"/> is null.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if a decrement would result in a negative value when <paramref name="allowNegativeValues"/> is false,
    /// or if a key in <paramref name="other"/> does not exist in <paramref name="dictionary"/> during a decrement.
    /// </exception>
    public static void ModifyMultiple<TKey>(this Dictionary<TKey, int> dictionary, IDictionary<TKey, int> other, bool allowNegativeValues = false)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        foreach (var kvp in other)
        {
            if (kvp.Value > 0)
                dictionary.Increment(kvp.Key, kvp.Value);
            else if (kvp.Value < 0)
                dictionary.Decrement(kvp.Key, -kvp.Value, allowNegativeValues);
        }
    }

    /// <summary>
    /// Adds a value to a list stored in the dictionary under the specified key.
    /// If the key does not exist, a new list is created and the value is added.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="dictionary">The dictionary containing lists of values.</param>
    /// <param name="key">The key under which to add the value.</param>
    /// <param name="value">The value to add to the list.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="dictionary"/> or <paramref name="key"/> is null.
    /// </exception>
    public static void AddToValueList<TKey, T>(this Dictionary<TKey, List<T>> dictionary, TKey key, T value)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (dictionary.ContainsKey(key))
        {
            dictionary[key].Add(value);
        }
        else
        {
            dictionary.Add(key, new List<T> { value });
        }
    }

    /// <summary>
    /// Selects multiple random keys from the dictionary based on float weights.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="weightDictionary">Dictionary mapping keys to float weights.</param>
    /// <param name="amount">The number of keys to select.</param>
    /// <param name="allowRepeating">If true, the same key can be selected multiple times; otherwise selections are unique.</param>
    /// <returns>A list of randomly selected keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="weightDictionary"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="amount"/> is less than 1, or if <paramref name="allowRepeating"/> is false and <paramref name="amount"/> exceeds the number of available keys.
    /// </exception>
    public static List<TKey> GetWeightedRandomElements<TKey>(this Dictionary<TKey, float> weightDictionary, int amount, bool allowRepeating = false)
    {
        if (weightDictionary == null)
            throw new ArgumentNullException(nameof(weightDictionary));
        if (amount < 1)
            throw new ArgumentException("Amount must be at least 1.", nameof(amount));
        if (!allowRepeating && amount > weightDictionary.Count)
            throw new ArgumentException("Amount cannot be greater than the number of unique elements when repetition is disallowed.", nameof(amount));

        var results = new List<TKey>(amount);

        if (allowRepeating)
        {
            for (int i = 0; i < amount; i++)
            {
                results.Add(weightDictionary.GetWeightedRandomElement());
            }
        }
        else
        {
            // Create a temporary copy to remove selected items
            var tempDict = new Dictionary<TKey, float>(weightDictionary);
            for (int i = 0; i < amount; i++)
            {
                TKey selected = tempDict.GetWeightedRandomElement();
                results.Add(selected);
                tempDict.Remove(selected);
            }
        }

        return results;
    }
}
