// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace ReactiveMarbles.Plugin.Example;

/// <summary>
/// This plug-in configures the HostBuilderContext to have the hosted services from the online example.
/// </summary>
[PluginOrder(100)]
public class Plugin : PluginBase<LifetimeEventsHostedService, TimedHostedService>;
