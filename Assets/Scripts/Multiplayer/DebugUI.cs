using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CoopGameUI : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void OnGUI()
    {
        int width = Screen.width, height = Screen.height;
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, width, height * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = height * 2 / 50;
        style.normal.textColor = Color.white;

        // FPS counter
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);



        // Buttons
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            if (GUI.Button(new Rect(10, 60, 150, 40), "Start as Host (Detective)"))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUI.Button(new Rect(10, 110, 150, 40), "Start as Client (Spirit)"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
        else
        {
            if (GUI.Button(new Rect(10, 160, 150, 40), "Shutdown"))
            {
                StartCoroutine(RestartSceneClean());
            }

        }
    }

    private IEnumerator RestartSceneClean()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
            yield return new WaitForSeconds(0.1f);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }


    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }
}
