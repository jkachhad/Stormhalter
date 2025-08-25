using System;
using System.IO;
using System.Linq;

namespace Kesmai.Prototype.RoslynWorkspace;

internal static class Program
{
    private static void Main()
    {
        var folderPath = @"C:\Example";
        var workspace = RoslynWorkspaceFactory.CreateFromFolder(folderPath);

        var project = workspace.CurrentSolution.Projects.First();
        Console.WriteLine($"Loaded {project.DocumentIds.Count} documents from {folderPath}.");
    }
}
