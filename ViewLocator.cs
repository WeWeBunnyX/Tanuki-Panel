using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TanukiPanel.ViewModels;

namespace TanukiPanel;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        var vmType = param.GetType();

        // Try common name patterns to locate the corresponding View type.
        // 1) Replace the .ViewModels namespace segment with .Views and the suffix ViewModel -> View
        // 2) Fallback: just replace ViewModel -> View in the full name
        string[] candidates = new[]
        {
            vmType.FullName!.Replace(".ViewModels.", ".Views.", StringComparison.Ordinal).Replace("ViewModel", "View", StringComparison.Ordinal),
            vmType.FullName!.Replace("ViewModel", "View", StringComparison.Ordinal)
        };

        Type? type = null;
        foreach (var candidate in candidates)
        {
            type = Type.GetType(candidate);
            if (type != null) break;

            // If Type.GetType fails (type in another assembly), scan loaded assemblies for the type full name.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(candidate);
                if (type != null) break;
            }

            if (type != null) break;
        }

        if (type != null)
        {
            var ctrl = (Control)Activator.CreateInstance(type)!;
            // Ensure the created view receives the view model as its DataContext
            ctrl.DataContext = param;
            return ctrl;
        }

        return new TextBlock { Text = "Not Found: " + vmType.FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
