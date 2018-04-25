using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Clase que realiza descarga de Assets Bundles.
/// </summary>
public class ABManager : MonoBehaviour
{
    /// <summary>
    /// Intancia
    /// </summary>
    private static ABManager instance = null;

    [Header("Download Settings")]
    [Tooltip("Elimina todos los assets descargados al cache, forzando una descarga nueva.")]
    public bool cleanCache = false;

    [Tooltip("Listado de BundleOjects para descargar, en cada BundleObject se especifica el nombre, objetos y version.")]
    public List<BundleObject> bundlesToLoad = new List<BundleObject>();


    /// <summary>
    /// Proceso de descarga
    /// 
    /// 
    /// 
    /// Listado de porcentajes por cada bundle. Si hay 5 bundles a descargar, entonces existiran 5 porcentajes cada uno del 0% al 100%
    /// </summary>
    private List<int> downloadPercents = new List<int>();

    /// <summary>
    /// Indice del bundle actual con el cual se está interactuando (bundle activo), con esto podemos asignar el valor al porcentaje de la lista superior.
    /// </summary>
    private int currentBundleIteration = 0;

    /// <summary>
    /// El objeto WWW, se requiere en la corutina de carga y en el state Update, para obtener el valor y mostrarlo.
    /// </summary>
    private WWW www;

    /// <summary>
    /// Porcentaje total de descarga
    /// </summary>
    private int downloadProgress = 0;

    /// <summary>
    /// Listado de Assets listos para su uso por otros Scripts.
    /// </summary>
    private Dictionary<string, List<Object>> readyAssets = new Dictionary<string, List<Object>>();

    /// <summary>
    /// Cuando el proceso de descarga termine, ese será TRUE.
    /// No es posible confiar solamente del porcentaje ya que es un proceso asincrono y tiene un retraso de respuesta, para ello usamos este flag.
    /// </summary>
    private bool processComplete = false;


    #region CLASS INSTANCE

    public static ABManager GetInstance()
    {
        return instance;
    }

    public void UnloadManager()
    {
        instance = null;
        Destroy(this.gameObject);
    }

    #endregion

    #region _STATES

    private void Awake()
    {
        if (instance != null)
            DestroyObject(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(this);
    }

    void Start()
    {
        if (bundlesToLoad.Count == 0)
            return;

        for (int i = 0; i < bundlesToLoad.Count; i++)
            downloadPercents.Add(0);

        StartCoroutine(DownloadAndCache());
    }

    private void Update()
    {
        //Si termino de descargar todos los bundles
        if (currentBundleIteration == downloadPercents.Count)
        {
            downloadProgress = 100;
            return;
        }

        if (www != null)
        {
            //Asignar a la lista el porcentaje de descarga del bundle activo.
            downloadPercents[currentBundleIteration] = Mathf.RoundToInt(www.progress * 100);

            //Proceso de suma y cálculo de promedio simple
            int downloadingProgressGlobal = 0;
            foreach (int objectProgress in downloadPercents)
                downloadingProgressGlobal += objectProgress;
            downloadingProgressGlobal = Mathf.RoundToInt(downloadingProgressGlobal / downloadPercents.Count);

            //Asignar valores para mostrar.
            downloadProgress = downloadingProgressGlobal;
        }
    }

    #endregion
    
    #region DOWNLOAD PROCESS

    IEnumerator DownloadAndCache()
    {
        //Si se solicita, se eliminará el cache.
        if (cleanCache)
            Caching.ClearCache();

        //Esperar que el sistema de Cache haya sido cargado, antes de continuar.
        while (!Caching.ready)
            yield return null;

        //Obtener plataforma de EJECUCIÓN
        string platform = BundleUtils.GetRuntimePlatformForAssetBundles();

        if (platform == null)
        {
            Debug.LogError("No Platform for this Runtime App.");
            yield break;
        }


        foreach (BundleObject bundleObject in bundlesToLoad)
        {
            string currentPath = BundleConstants.AssetsBundleServerURL + platform + "/" + bundleObject.bundleName;
            www = WWW.LoadFromCacheOrDownload(currentPath, bundleObject.version);
            yield return www;

            if (www.error != null)
            {
                Debug.LogError("Url: " + currentPath);
                Debug.LogError("WWW download had an error:" + www.error);
                yield break;
            }

            AssetBundle bundle = www.assetBundle;

            //Si no se especificaron assets, se cargaran todos
            string[] assetsInBundle = bundleObject.contentNames.Length == 0 ? bundle.GetAllAssetNames() : bundleObject.contentNames;

            //Lista de assets que vienen en este bundle
            List<Object> bundleObjectsTemp = new List<Object>();

            foreach (string content in assetsInBundle)
            {
                AssetBundleRequest currentAsset = bundle.LoadAssetAsync(content);
                yield return currentAsset;

                bundleObjectsTemp.Add(currentAsset.asset);
                //Instantiate(currentAsset.asset);
                //Instantiate(bundle.mainAsset);
                //Debug.Log("Downloading " + content);
            }

            Debug.Log("Bundle OK " + bundleObject.name);

            //Asignación a assets completos
            readyAssets.Add(bundleObject.bundleName, bundleObjectsTemp);

            currentBundleIteration++;

            //Liberar los contenidos comprimidos de los AssetBundles para conservar memoria.
            bundle.Unload(false);
        }

        processComplete = true;

    }

    #endregion

    #region GETTERS / SETTERS

    /// <summary>
    /// Devuelve el progreso de descarga en formato int de 0-100
    /// </summary>
    /// <returns></returns>
    public int GetDownloadProgress()
    {
        //Se descargó TODO pero aun no se asignan los valores localmente de LoadAssetAsync.
        //Este proceso puede variar dependiendo de la cantidad de assets descargados.
        if (downloadProgress == 100)
        {            
            if (processComplete)
            {
                return 100;
            }
            else
            {
                //Esperar a que el proceso de asignación termine.
                return 99;
            }
        }
        else
        {
            return downloadProgress;
        }
    }

    /// <summary>
    /// Retorna Objeto solicitado desde los assets bundles ya cargados.
    /// El usuario deberá realizar la conversión al tipo que corresponda.
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public Object GetAsset(string bundle, string assetName)
    {
        if (downloadProgress != 100)
            return null;

        foreach (KeyValuePair<string, List<Object>> kv in readyAssets)
        {
            if (kv.Key == bundle)
            {
                foreach (Object asset in kv.Value)
                {
                    if (asset.name == assetName)
                    {
                        return asset;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Retorna todos los asses de un bundle como Object
    /// </summary>
    /// <param name="bundle"></param>
    /// <returns></returns>
    public List<Object> GetAssets(string bundle)
    {
        if (!readyAssets.ContainsKey(bundle))
            return null;

        return readyAssets[bundle];
    }

    /// <summary>
    /// Retorna todos los asses de un bundle como Sprite
    /// </summary>
    /// <param name="bundle"></param>
    /// <returns></returns>
    public List<Sprite> GetAssetsAsSprite(string bundle)
    {
        if (!BundleExists(bundle))
            return null;

        List<Sprite> returnList = new List<Sprite>();
        List<Object> objs = readyAssets[bundle];

        for (int i = 0; i < objs.Count; i++)
        {
            Texture2D tx = (Texture2D)objs[i]; //Conversión de tipo
            Sprite spr = Sprite.Create(tx, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            spr.name = tx.name;
            returnList.Add(spr);
        }

        return returnList;
    }

    #endregion

    #region CALLBACKS
    /// <summary>
    /// Verifica que el bundle solicitado por el usuario exista.
    /// </summary>
    /// <param name="bundle">Nombre del Bundle</param>
    /// <returns>True / False</returns>
    private bool BundleExists(string bundle)
    {
        if (!readyAssets.ContainsKey(bundle))
        {
            Debug.LogWarning("El bundle " + bundle + " no existe.");
            foreach (KeyValuePair<string, List<Object>> kv in readyAssets)
                Debug.LogWarning("- " + kv.Key);
            return false;
        }
        return true;
    }
    #endregion
}