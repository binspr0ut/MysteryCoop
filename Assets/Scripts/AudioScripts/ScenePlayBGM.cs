using UnityEngine;

public class ScenePlayBGM : MonoBehaviour
{
    public AudioClip bgm;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(bgm);
        }
    }
}