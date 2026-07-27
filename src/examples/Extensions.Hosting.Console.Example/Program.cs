// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.PluginService;

namespace ReactiveMarbles.Extensions.Hosting.Console.Example;

/// <summary>Provides the console example entry point.</summary>
internal static class Program
{
    /// <summary>Runs the plugin service host.</summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task representing the host lifetime.</returns>
    internal static Task Main(string[] args) =>
        ServiceHost.Create(typeof(Program), args);
}
