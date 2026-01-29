using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildScript
{
    public static void PerformWebGLBuild()
    {
        var buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        buildPlayerOptions.locationPathName = "Build";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize} bytes");
        }
        else if (summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            Debug.LogError("Build failed");
            // Exit with error code if running in batch mode so CI/Scripts know it failed
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
