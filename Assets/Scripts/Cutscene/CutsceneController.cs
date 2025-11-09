using UnityEngine;
using UnityEngine.Playables;
using System;
using UnityEngine.Video;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    private Action onEnd;
    private VideoPlayer video;

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        if (!video) video = GetComponent<VideoPlayer>();
        if (video) video.playOnAwake = false;
    }

    public void Play(Action end)
    {
        onEnd = end;
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (!director) director = GetComponent<PlayableDirector>();
        if (!video) video = GetComponent<VideoPlayer>();

        // Tunggu 1 frame agar VideoPlayer siap
        yield return null;

        if (video)
        {
            video.Stop();
            video.Prepare();                     // prepare dulu
            while (!video.isPrepared) yield return null; // tunggu sampai siap
            video.Play();
            Debug.Log($"[CutsceneController] Playing video: {video.clip?.name}");
        }

        // Mainkan timeline
        director.stopped -= OnStopped;
        director.stopped += OnStopped;
        director.time = 0;
        director.Play();
    }

    public void SkipToEnd()
    {
        if (!director) return;
        if (director.state == PlayState.Playing)
        {
            double eps = 0.05;
            director.time = Math.Max(0, director.duration - eps);
            director.Evaluate();
        }
    }

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
