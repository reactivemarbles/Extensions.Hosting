// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Dapplo.Hosting.Sample.AvaloniaDemo;

/// <summary>
/// A simple ViewLocator to locate views based on ViewModel type names.
/// This is optional and can be used if you follow MVVM pattern.
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Creates a new instance of a Control that corresponds to the type of the provided data object.
    /// </summary>
    /// <remarks>This method infers the Control type by replacing 'ViewModel' with 'View' in the data object's
    /// type name. If the type cannot be resolved or instantiation fails, a TextBlock with an error message is returned
    /// instead.</remarks>
    /// <param name="data">The data object used to determine which Control type to instantiate. Cannot be null.</param>
    /// <returns>A Control instance that matches the type of the data object, or null if the data is null. If the corresponding
    /// Control type cannot be found or instantiated, returns a TextBlock indicating the error.</returns>
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            try
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            catch
            {
                return new TextBlock { Text = "Failed to create: " + name };
            }
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Determines whether the specified data object is not null.
    /// </summary>
    /// <param name="data">The data object to evaluate for nullity. This parameter can be null.</param>
    /// <returns>true if the data object is not null; otherwise, false.</returns>
    public bool Match(object? data) => data is { };
}
