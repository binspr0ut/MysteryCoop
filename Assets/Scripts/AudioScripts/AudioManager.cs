using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;

    private void Awake()
    {
        // simple singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return; // kalau sudah jalan, jangan restart

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        bgmSource.PlayOneShot(clip); // boleh pakai bgmSource dulu karena masih 2D
    }

    public void StopBGM(bool clearClip = false)
    {
        if (bgmSource == null) return;

        bgmSource.Stop();

        if (clearClip)
            bgmSource.clip = null;
    }

}