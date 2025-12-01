// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.Extensions.Hosting.Maui;

namespace Extensions.Hosting.Maui.Example;

/// <summary>
/// Example MAUI service.
/// </summary>
public class ExampleMauiService : IMauiService
{
    /// <summary>
    /// Initializes the specified application.
    /// </summary>
    /// <param name="application">The application.</param>
    public void Initialize(Microsoft.Maui.Controls.Application application)
    {
        // Example initialization
        Console.WriteLine("MAUI application initialized via IMauiService");
    }
}
