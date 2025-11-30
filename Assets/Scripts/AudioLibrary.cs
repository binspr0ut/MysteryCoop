using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary Instance;

    public AudioClip[] audioClips;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public AudioClip GetClip(int index)
    {
        if (index < 0 || index >= audioClips.Length)
            return null;

        return audioClips[index];
    }
}
