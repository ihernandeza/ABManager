using UnityEngine;

[CreateAssetMenu(fileName = "New Asset Bundle Object",menuName = "PleIQ/Asset Bundle Object")]
public class BundleObject : ScriptableObject {

    [Header("Settings")]
    public int version = 0;

    [Header("Data")]
    public string bundleName = "";    
    public string[] contentNames;
}
