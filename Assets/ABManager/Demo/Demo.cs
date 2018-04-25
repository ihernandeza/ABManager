using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Demo : MonoBehaviour {

    public Text txt;

	IEnumerator Start () {

        Debug.Log("Iniciando Escena... " + Time.time);

        //Progreso de descarga...
        bool download = true;
        while (download)
        {
            int progress = ABManager.GetInstance().GetDownloadProgress();
            txt.text = string.Format("{0}{1}", progress, "%");

            if (progress == 100)
                download = false;

            yield return new WaitForEndOfFrame();
        }

        //Instanciar elementos obtenidos.
        List<Object> assets = ABManager.GetInstance().GetAssets("geo_shapes");
        for (int i = 0; i < assets.Count; i++)
        {
            GameObject temp = Instantiate(assets[i], new Vector3(i, i, i), Quaternion.identity) as GameObject;
        }

        //Simular tiempo de la actividad
        yield return new WaitForSeconds(10);

        //Destruye el GameObject que contiene a ABManager, los elementos cargados permanecerán en la escena.
        ABManager.GetInstance().UnloadManager();

        //Simular un cambio de escena....
        SceneManager.LoadScene("Demo");
    }
}
