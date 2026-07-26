// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;

namespace Extensions.Hosting.Benchmarks;

/// <summary>Runs the hosting benchmark suite.</summary>
public static class Program
{
    /// <summary>Runs all selected benchmarks.</summary>
    /// <param name="args">The BenchmarkDotNet command-line arguments.</param>
    public static void Main(string[] args) =>
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
