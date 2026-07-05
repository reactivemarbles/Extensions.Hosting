// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.Plugins;

namespace ReactiveMarbles.Plugin.Example;

/// <summary>This plug-in configures the HostBuilderContext to have the hosted services from the online example.</summary>
[PluginOrder(100)]
public class Plugin : PluginBase<LifetimeEventsHostedService, TimedHostedService>;
