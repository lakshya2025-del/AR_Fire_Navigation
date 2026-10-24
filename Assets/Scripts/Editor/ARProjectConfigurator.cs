using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ARFireNavigation.Editor
{
    [InitializeOnLoad]
    public static class ARProjectConfigurator
    {
        static ARProjectConfigurator()
        {
            EditorApplication.delayCall += VerifyProjectConfiguration;
        }

        [MenuItem("AR Fire/Verify Project Configuration")]
        public static void VerifyProjectConfiguration()
        {
            Debug.Log("[ARProjectConfigurator] Verifying AR Fire Navigation project configuration...");

            // 1. Verify PlayerSettings
            PlayerSettings.productName = "AR Fire Navigation";
            PlayerSettings.companyName = "UniversityAR";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.university.arfirenavigation");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            // 2. Verify Build Scenes
            string scenePath = "Assets/Scenes/MainARScene.unity";
            var currentScenes = EditorBuildSettings.scenes;
            bool sceneRegistered = false;
            foreach (var s in currentScenes)
            {
                if (s.path == scenePath)
                {
                    sceneRegistered = true;
                    break;
                }
            }

            if (!sceneRegistered && File.Exists(scenePath))
            {
                var newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
                Array.Copy(currentScenes, newScenes, currentScenes.Length);
                newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;
                Debug.Log($"[ARProjectConfigurator] Registered {scenePath} in EditorBuildSettings.");
            }

            Debug.Log("[ARProjectConfigurator] Project verification completed successfully.");
        }

        public static void BuildAndroidAPK()
        {
            Debug.Log("[ARProjectConfigurator] Starting command-line Android build process...");
            VerifyProjectConfiguration();

            string[] scenes = new string[] { "Assets/Scenes/MainARScene.unity" };
            string buildDir = "Builds/Android";
            Directory.CreateDirectory(buildDir);
            string locationPath = Path.Combine(buildDir, "AR_Fire_Navigation.apk");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = locationPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[ARProjectConfigurator] Android build succeeded! Size: {summary.totalSize} bytes, Output: {locationPath}");
            }
            else
            {
                Debug.LogError($"[ARProjectConfigurator] Android build failed with result: {summary.result}. Errors: {summary.totalErrors}");
            }
        }
    }
}
