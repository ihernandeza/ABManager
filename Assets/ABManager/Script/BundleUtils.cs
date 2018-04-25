using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleUtils {

    public static string GetRuntimePlatformForAssetBundles()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            case RuntimePlatform.WebGLPlayer:
                return "WebGL";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            case RuntimePlatform.WindowsEditor:
                return "Editor";
            case RuntimePlatform.OSXEditor:
                return "Editor";
            default:
                return null;
        }
    }
}
