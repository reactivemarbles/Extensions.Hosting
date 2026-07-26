// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Extensions.Hosting.Tests;

/// <summary>Provides allocation-conscious enumerable helpers for test assertions.</summary>
internal static class TestEnumerable
{
    /// <summary>Determines whether a sequence contains at least one element.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The sequence to inspect.</param>
    /// <returns>true when the sequence contains an element; otherwise, false.</returns>
    internal static bool ContainsAny<T>(IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext();
    }

    /// <summary>Determines whether a sequence contains an element matching a predicate.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The sequence to inspect.</param>
    /// <param name="predicate">The predicate used to identify a matching element.</param>
    /// <returns>true when a matching element is found; otherwise, false.</returns>
    internal static bool Contains<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Materializes a sequence into a list through explicit iteration.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The sequence to materialize.</param>
    /// <returns>A list containing the sequence elements.</returns>
    internal static List<T> Materialize<T>(IEnumerable<T> source) =>
        new(source);
}
