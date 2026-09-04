// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Extensions.Hosting.LegacyNetFramework.Tests;

/// <summary>Verifies package behavior compiled for .NET Framework through a child worker process.</summary>
public sealed class LegacyNetFrameworkWorkerTests
{
    /// <summary>Stores the result line payload offset after the prefix and separating space.</summary>
    private const int ResultPayloadOffset = 7;

    /// <summary>Stores the maximum time allowed for the worker process to complete.</summary>
    private static readonly TimeSpan WorkerTimeout = TimeSpan.FromSeconds(30);

    /// <summary>Runs the .NET Framework worker and validates all legacy package probes.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LegacyNetFrameworkWorker_CoversPackageCompatibilityPaths()
    {
        var workerDirectory = Path.Combine(AppContext.BaseDirectory, "LegacyNetFrameworkWorker");
        var workerPath = Path.Combine(workerDirectory, "Extensions.Hosting.LegacyNetFramework.Worker.exe");
        var normalPluginDirectory = Path.Combine(workerDirectory, "ExternalPluginFixtures", "normal");
        var reactivePluginDirectory = Path.Combine(workerDirectory, "ExternalPluginFixtures", "reactive");
        var resultPath = Path.Combine(workerDirectory, $"legacy-results-{Guid.NewGuid():N}.txt");

        await Assert.That(File.Exists(workerPath)).IsTrue();
        await Assert.That(Directory.Exists(normalPluginDirectory)).IsTrue();
        await Assert.That(Directory.Exists(reactivePluginDirectory)).IsTrue();

        using var process = new Process
        {
            StartInfo = new(workerPath)
            {
                ArgumentList = { normalPluginDirectory, reactivePluginDirectory, resultPath },
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workerDirectory,
            },
        };

        _ = process.Start();
        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();
        var exited = await WaitForExitAsync(process, WorkerTimeout);
        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        await Assert.That(exited).IsTrue();
        await Assert.That(process.ExitCode).IsEqualTo(0);
        await Assert.That(standardError).IsEmpty();

        await Assert.That(standardOutput).IsEmpty();
        await Assert.That(File.Exists(resultPath)).IsTrue();

        var results = ParseResults(await File.ReadAllTextAsync(resultPath));
        await Assert.That(results["resource_mutex"]).IsEqualTo("PASS");
        await Assert.That(results["base_ui_thread"]).IsEqualTo("PASS");
        await Assert.That(results["normal_plugin_load"]).IsEqualTo("PASS");
        await Assert.That(results["reactive_plugin_load"]).IsEqualTo("PASS");
        await Assert.That(results["missing_plugin_load"]).IsEqualTo("PASS");
        await Assert.That(results["log4net_finalizer"]).IsEqualTo("PASS");
    }

    /// <summary>Waits for the worker process and terminates it when it exceeds the timeout.</summary>
    /// <param name="process">The worker process.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <returns>A task whose result is true when the worker exits before the timeout.</returns>
    private static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout)
    {
        var delayTask = Task.Delay(timeout);
        var exitTask = Task.Run(process.WaitForExit);
        var completedTask = await Task.WhenAny(exitTask, delayTask);

        if (completedTask == exitTask)
        {
            await exitTask;
            return true;
        }

        try
        {
            process.Kill();
        }
        catch (InvalidOperationException)
        {
        }

        return false;
    }

    /// <summary>Parses worker result lines from the worker result file.</summary>
    /// <param name="standardOutput">The result file content.</param>
    /// <returns>The probe names and pass/fail markers reported by the worker.</returns>
    private static Dictionary<string, string> ParseResults(string standardOutput)
    {
        var results = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in standardOutput.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("RESULT ", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line[ResultPayloadOffset..].Split(['='], count: 2);
            if (parts.Length == 2)
            {
                results[parts[0]] = parts[1];
            }
        }

        return results;
    }
}
