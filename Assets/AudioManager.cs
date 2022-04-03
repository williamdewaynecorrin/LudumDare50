using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static float kMinDistance3D = 10.0f;
    public static float kMaxDistance3D = 100.0f;

    private static AudioManager instance = null;
    public static float audiosettingsvolume
    {
        get 
        {
            return _audiosettingsvolume; 
        }
        set
        {
            _audiosettingsvolume = value;
        }
    }
    private static float _audiosettingsvolume = 1.0f;

    private AudioSource source;
    private int fadeframes = 180;
    private bool musicmatch = false;
    private float musictime = 0f;
    private float timescale = 1.0f;

    void Awake()
    {
        if (instance == null)
        {
            InitInstance();
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
    void InitInstance()
    {
        instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    // -- music fade
    public static void FadeIn()
    {
        instance.source.volume = 0f;
        if(!instance.musicmatch)
            instance.source.Play();

        instance.StartCoroutine(instance.FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float val = 1f / (float)fadeframes;
        for(int i = 0; i < fadeframes; ++i)
        {
            source.volume += val;
            yield return new WaitForEndOfFrame();
        }

        source.volume = 1f;
        yield return null;
    }

    public static void FadeOut()
    {
        instance.source.volume = 1f;
        instance.StartCoroutine(instance.FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float val = 1f / (float)fadeframes;
        for (int i = 0; i < fadeframes; ++i)
        {
            source.volume -= val;
            yield return new WaitForEndOfFrame();
        }

        source.volume = 0f;
        yield return null;
    }

    public static bool MusicMatch()
    {
        return instance.musicmatch;
    }

    public static void RecordMusicTime()
    {
        instance.musictime = instance.source.time;
    }

    public static void SetMusicTime()
    {
        instance.source.time = instance.musictime;
    }

    public static void ResetMusicTime()
    {
        instance.musictime = 0f;
        instance.source.time = instance.musictime;
    }

    public static void PlayStitchedAudio(AudioClip[] clips, float stitchtime)
    {
        instance.StartCoroutine(instance.StitchAudio(clips, stitchtime));
    }

    IEnumerator StitchAudio(AudioClip[] clips, float stitchtime)
    {
        for (int startidx = 0; startidx < clips.Length; ++startidx)
        {
            PlayClip2D(clips[startidx]);
            yield return new WaitForSeconds(clips[startidx].length + stitchtime);
        }

        yield return null;
    }

    public static void PlayDelayedAudio(AudioClip clip, float waittime)
    {
        instance.StartCoroutine(instance.DelayedAudio(clip, waittime));
    }

    IEnumerator DelayedAudio(AudioClip clip, float waittime)
    {
        yield return new WaitForSeconds(waittime);
        PlayClip2D(clip);

        yield return null;
    }

    // -- sfx play
    public static AudioSource PlayClip2D(AudioClip clip, float volume = 1.0f)
    {
        AudioSource audio = new GameObject("audio_" + clip.name).AddComponent<AudioSource>();
        audio.volume = audiosettingsvolume * Mathf.Clamp01(volume);
        audio.clip = clip;
        audio.spatialBlend = 0.0f;
        audio.Play();
        GameObject.Destroy(audio.gameObject, clip.length);

        return audio;
    }

    public static AudioSource PlayClip2D(AudioClip clip, float volume, float pitch)
    {
        AudioSource audio = PlayClip2D(clip, volume);
        audio.pitch = pitch;

        return audio;
    }

    public static AudioSource PlayClip3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource audio = new GameObject("audio3D_" + clip.name).AddComponent<AudioSource>();
        audio.transform.position = position;
        audio.volume = audiosettingsvolume * volume;
        audio.clip = clip;
        audio.spatialBlend = 1.0f;
        audio.minDistance = kMinDistance3D;
        audio.maxDistance = kMaxDistance3D;
        audio.Play();
        GameObject.Destroy(audio.gameObject, clip.length);

        return audio;
    }

    public static AudioSource PlayClip3D(AudioClip clip, Vector3 position, float pitch, float volume = 1f)
    {
        AudioSource source = PlayClip3D(clip, position, volume);
        source.pitch = pitch;

        return source;
    }

    public static AudioSource PlayClip3D(AudioClip clip, Vector3 position, AudioSource copyfrom)
    {
        AudioSource audio = new GameObject("audio3D_" + clip.name).AddComponent<AudioSource>();
        audio.transform.position = position;
        audio.volume = audiosettingsvolume * copyfrom.volume;
        audio.clip = clip;
        audio.rolloffMode = copyfrom.rolloffMode;
        audio.maxDistance = copyfrom.maxDistance;
        audio.reverbZoneMix = copyfrom.reverbZoneMix;
        audio.spread = copyfrom.spread;
        audio.dopplerLevel = copyfrom.dopplerLevel;
        audio.spatialBlend = copyfrom.spatialBlend;

        audio.Play();
        GameObject.Destroy(audio.gameObject, clip.length);

        return audio;
    }

    public static AudioClip GetRandomClip(AudioClip[] clips)
    {
        int i = Random.Range(0, clips.Length);
        return clips[i];
    }

    public static AudioSource PlayRandomClip2D(AudioClip[] clips)
    {
        int i = Random.Range(0, clips.Length);
        return PlayClip2D(clips[i]);
    }

    public static AudioSource PlayRandomClip3D(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        int i = Random.Range(0, clips.Length);
        return PlayClip3D(clips[i], position, volume);
    }

    public static AudioSource PlayRandomClip3D(AudioClip[] clips, Vector3 position, AudioSource copyfrom)
    {
        int i = Random.Range(0, clips.Length);
        return PlayClip3D(clips[i], position, copyfrom);
    }

    // -- sfx control play
    public static AudioSource PlayClip2D(AudioClipSoundControl clip)
    {
        return PlayClip2D(clip.clip, clip.volumemult);
    }

    public static AudioSource PlayClip2D(AudioClipSoundControl clip, float pitch)
    {
        return PlayClip2D(clip.clip, clip.volumemult, pitch);
    }

    public static AudioSource PlayClip3D(AudioClipSoundControl clip, Vector3 position)
    {
        return PlayClip3D(clip.clip, position, clip.volumemult);
    }

    public static AudioSource PlayClip3D(AudioClipSoundControl clip, Vector3 position, float pitch)
    {
        return PlayClip3D(clip.clip, position, pitch, clip.volumemult);
    }

    public static AudioClipSoundControl GetRandomClip(AudioClipSoundControl[] clips)
    {
        int i = Random.Range(0, clips.Length);
        return clips[i];
    }

    public static AudioSource PlayRandomClip2D(AudioClipSoundControl[] clips)
    {
        int i = Random.Range(0, clips.Length);
        return PlayClip2D(clips[i]);
    }

    public static AudioSource PlayRandomClip3D(AudioClipSoundControl[] clips, Vector3 position)
    {
        int i = Random.Range(0, clips.Length);
        return PlayClip3D(clips[i], position);
    }
}

[System.Serializable]
public class AudioClipCollection
{
    public AudioClip[] sounds;
}

[System.Serializable]
public class AudioClipSoundControlCollection
{
    public AudioClipSoundControl[] sounds;
}

[System.Serializable]
public class AudioClipSoundControl
{
    public AudioClip clip;
    [Range(0f, 2f)]
    public float volumemult = 1.0f;
}