using System;
using System.IO;
using Haptic.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Haptic.Editor
{
    public static class ProjectSetup
    {
        const string ScenePath = "Assets/Scenes/Haptic.unity";
        const string ApkPath = "Builds/Android/Haptic-Android.apk";

        [MenuItem("Haptic/Configure Project")]
        public static void Configure()
        {
            Directory.CreateDirectory("Assets/Scenes");
            ConfigurePlayer();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("UI Camera", typeof(Camera), typeof(AudioListener));
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.025f, 0.045f, 1f);
            camera.orthographic = true;

            new GameObject("Haptic", typeof(AppBootstrap));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Haptic project configured.");
        }

        [MenuItem("Haptic/Build Android APK")]
        public static void BuildAndroid()
        {
            ConfigurePlayer();
            if (!File.Exists(ScenePath))
                Configure();

            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath) ?? "Builds/Android");
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"Android build failed: {report.summary.result}, {report.summary.totalErrors} errors");
            Debug.Log($"APK built: {Path.GetFullPath(ApkPath)} ({report.summary.totalSize} bytes)");
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Jomebe";
            PlayerSettings.productName = "Haptic";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.jomebe.haptic");
            PlayerSettings.bundleVersion = "1.0.1";
            PlayerSettings.Android.bundleVersionCode = 2;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard);
        }
    }
}

