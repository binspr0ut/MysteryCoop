// using UnityEngine;
// using UnityEngine.Playables;
// using System;
// using UnityEngine.Video;

// public class CutsceneController : MonoBehaviour
// {
//     public PlayableDirector director;
//     private Action onEnd;
//     private VideoPlayer video; 
//     public void Play(Action end)
//     {
//         // print("Sapiman Play");
//         // onEnd = end;
//         // if (!director) director = GetComponent<PlayableDirector>();
//         // director.stopped += OnStopped;
//         // director.Play();
//         print("Sapiman Rigel");
//         Rigel();
//     }

//     public void Rigel()
//     {
//         if (!director) director = GetComponent<PlayableDirector>();
//         if (!video) video = GetComponent<VideoPlayer>();
//         print("Sapiman Call Rigel");
//         director.Play();
//         video.Play();
//     }

//     public void Skip()
//     {
//         if (director && director.state == PlayState.Playing)
//             director.Stop();
//     }

//     void OnStopped(PlayableDirector d)
//     {
//         d.stopped -= OnStopped;
//         onEnd?.Invoke();
//     }
// }

using UnityEngine;
using UnityEngine.Playables;
using System;
using UnityEngine.Video;

public class CutsceneController : MonoBehaviour
{
    public PlayableDirector director;
    private Action onEnd;
    private VideoPlayer video;

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        if (!video)    video    = GetComponent<VideoPlayer>();
        if (video)     video.playOnAwake = false;
    }

    public void Play(Action end)
    {
        onEnd = end;
        if (!director) director = GetComponent<PlayableDirector>();
        if (!video)    video    = GetComponent<VideoPlayer>();

        // restart state bersih
        if (video)    { video.Stop(); video.Play(); }
        director.stopped -= OnStopped;
        director.stopped += OnStopped;
        director.time = 0;
        director.Play();
    }

    // TAP #1: loncat ke akhir, tapi jangan akhiri sequence (biar user lihat frame terakhir + fade)
    public void SkipToEnd()
    {
        if (!director) return;
        if (director.state == PlayState.Playing)
        {
            // loncat persis ke near-end; biarkan event OnStopped terpicu natural
            double eps = 0.05;
            director.time = Math.Max(0, director.duration - eps);
            director.Evaluate(); // tampilkan frame akhir segera
        }
    }

    // (opsional) kalau ingin langsung matikan
    public void StopNow()
    {
        if (director && director.state == PlayState.Playing)
            director.Stop();
    }

    private void OnStopped(PlayableDirector d)
    {
        d.stopped -= OnStopped;
        onEnd?.Invoke();
    }
}