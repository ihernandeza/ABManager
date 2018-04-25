#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{

    public static void BuildAllAssetBundle()
    {
        BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

        string outputPath = Path.Combine("AssetBundles", GetPlatformForAssetBundles(buildTarget));
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        //BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets

        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, buildTarget);
    }

    private static string GetPlatformForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            default:
                return null;
        }
    }
}
#endif