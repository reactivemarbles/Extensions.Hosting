// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Android.App;
using Android.Runtime;

namespace Extensions.Hosting.Maui.Example;

/// <summary>Represents the Android application entry point for the sample MAUI application.</summary>
/// <seealso cref="Microsoft.Maui.MauiApplication" />
[Application]
public class MainApplication : MauiApplication
{
    /// <summary>Initializes a new instance of the <see cref="MainApplication"/> class.</summary>
    /// <param name="handle">The handle.</param>
    /// <param name="ownership">The ownership.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <summary>Creates the configured MAUI application.</summary>
    /// <returns>The configured MAUI application.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.GetMauiApp();
}
