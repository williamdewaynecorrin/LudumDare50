using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static float audiosettingsvolume
    {
        get 
        {
            return _audiosettingsvolume; 
        }
        set
        {
            _audiosettingsvolume = value;

            if (instance != null)
            {
                if(instance.audio != null)
                    instance.audio = instance.GetComponent<AudioSource>();

                if(instance.DiscernAudio() != null)
                    instance.DiscernAudio().volume = _audiosettingsvolume;
            }
        }
    }
    private static float _audiosettingsvolume = 1.0f;

    [SerializeField]
    private AudioClip levelmusic;
    [SerializeField]
    private float musicvolumedelta = 0.01f;
    [SerializeField]
    private bool debugmusicoff = false;

    private new AudioSource audio;
    private static MusicManager instance = null;
    private ESmartAudioState state = ESmartAudioState.Stopped;
    private float timescale = 1.0f;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;

            if (debugmusicoff)
                _audiosettingsvolume = 0f;

            audio = GetComponent<AudioSource>();
            audio.loop = true;
            audio.spatialBlend = 0f;
            audio.clip = levelmusic;
            audio.volume = 0f;

            FadeIn();
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            bool newsong = this.levelmusic != instance.DiscernAudio().clip;

            if (!instance.DiscernAudio().isPlaying || newsong)
            {
                instance.DiscernAudio().clip = this.levelmusic;
                instance.DiscernAudio().volume = 0f;
                FadeIn();
            }
            else
            {
                // -- nothing needed for now
            }

            GameObject.Destroy(this.gameObject);
        }
    }

    void FixedUpdate()
    {
        if (state == ESmartAudioState.Starting)
        {
            DiscernAudio().volume += musicvolumedelta;
            if (DiscernAudio().volume >= audiosettingsvolume)
            {
                DiscernAudio().volume = audiosettingsvolume;
                state = ESmartAudioState.Started;
            }
        }
        else if (state == ESmartAudioState.Stopping)
        {
            DiscernAudio().volume -= musicvolumedelta;
            if (DiscernAudio().volume <= 0f)
            {
                DiscernAudio().volume = 0f;
                state = ESmartAudioState.Stopped;
                DiscernAudio().Stop();
            }
        }
    }

    private AudioSource DiscernAudio()
    {
        return audio;
    }

    public static void FadeIn()
    {
        instance.audio.Play();
        instance.state = ESmartAudioState.Starting;
    }

    public static void FadeOut()
    {
        instance.state = ESmartAudioState.Stopping;
    }

    public static ESmartAudioState GetState()
    {
        return instance.state;
    }

    public static void SetTimeScale(float timescale)
    {
        instance.timescale = timescale;
    }
}

