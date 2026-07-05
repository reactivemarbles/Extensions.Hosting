// Copyright (c) 2016-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveMarbles.Extensions.Hosting.WinForms;

/// <summary>Represents a shell interface for hosting and managing Windows Forms-based user interfaces within an application.</summary>
/// <remarks>Implement this interface to provide integration points for Windows Forms UI components, such as main
/// windows, dialogs, or tool windows, within a host application. The specific responsibilities and capabilities of the
/// shell are defined by the implementing class.</remarks>
public interface IWinFormsShell : IComponent;
