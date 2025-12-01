// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Android.App;
using Android.Runtime;

namespace Extensions.Hosting.Maui.Example;

/// <summary>
/// MainApplication.
/// </summary>
/// <seealso cref="Microsoft.Maui.MauiApplication" />
[Application]
public class MainApplication : MauiApplication
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainApplication"/> class.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <param name="ownership">The ownership.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership, IServiceProvider serviceProvider)
        : base(handle, ownership) => _serviceProvider = serviceProvider;

    protected override MauiApp CreateMauiApp() => _serviceProvider.GetRequiredService<MauiApp>();
}
