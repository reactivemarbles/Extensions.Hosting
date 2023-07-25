// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;

namespace CP.Extensions.Hosting.Wpf;

/// <summary>
/// This defines a service which is called before the message loop is started.
/// </summary>
public interface IWpfService
{
    /// <summary>
    /// Do whatever you need to do to initialize WPF, this is called from the UI thread.
    /// </summary>
    /// <param name="application">Application.</param>
    void Initialize(Application application);
}
