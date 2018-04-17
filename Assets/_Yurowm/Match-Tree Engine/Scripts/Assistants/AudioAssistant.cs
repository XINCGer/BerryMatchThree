using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

[RequireComponent (typeof (AudioListener))]
[RequireComponent (typeof (AudioSource))]
public class AudioAssistant : MonoBehaviour {

	public static AudioAssistant main;

	AudioSource music;
	AudioSource sfx;

	public float musicVolume {
        get {
            if (!PlayerPrefs.HasKey("Music Volume"))
                return 0.5f;
            return PlayerPrefs.GetFloat("Music Volume");
        }
        set {
            PlayerPrefs.SetFloat("Music Volume", value);
        }
    }

    public float sfxVolume {
        get {
            if (!PlayerPrefs.HasKey("SFX Volume"))
                return 1f;
            return PlayerPrefs.GetFloat("SFX Volume");
        }
        set {
            PlayerPrefs.SetFloat("SFX Volume", value);
        }
    }

    public List<MusicTrack> tracks = new List<MusicTrack>();
    public List<Sound> sounds = new List<Sound>();
    Sound GetSoundByName(string name) {
        return sounds.Find(x => x.name == name);
    }

	static List<string> mixBuffer = new List<string>();
	static float mixBufferClearDelay = 0.05f;

    public bool mute = false;
    public bool quiet_mode = false;
    
    internal string currentTrack;
		

    void Awake() {
        main = this;

        AudioSource[] sources = GetComponents<AudioSource>();
        music = sources[0];
        sfx = sources[1];

        // Initialize
        sfxVolume = sfxVolume;
        musicVolume = musicVolume;

        ChangeMusicVolume(musicVolume);
        ChangeSFXVolume(sfxVolume);

        StartCoroutine(MixBufferRoutine());

        mute = PlayerPrefs.GetInt("Mute") == 1;
    }

	// Coroutine responsible for limiting the frequency of playing sounds
	IEnumerator MixBufferRoutine() {
        float time = 0;

		while (true) {
            time += Time.unscaledDeltaTime;
            yield return 0;
            if (time >= mixBufferClearDelay) {
			    mixBuffer.Clear();
                time = 0;
            }
		}
	}

	// Launching a music track
    public void PlayMusic(string trackName) {
        if (trackName != "")
            currentTrack = trackName;
		AudioClip to = null;
        foreach (MusicTrack track in tracks)
            if (track.name == trackName)
                to = track.track;
        StartCoroutine(main.CrossFade(to));
	}

	// A smooth transition from one to another music
	IEnumerator CrossFade(AudioClip to) {
		float delay = 0.3f;
		if (music.clip != null) {
			while (delay > 0) {
				music.volume = delay * musicVolume * ProjectParameters.main.music_volume_max;
				delay -= Time.unscaledDeltaTime;
				yield return 0;
			}
		}
		music.clip = to;
        if (to == null || mute) {
            music.Stop();
            yield break;
        }
        delay = 0;
		if (!music.isPlaying) music.Play();
		while (delay < 0.3f) {
			music.volume = delay * musicVolume * ProjectParameters.main.music_volume_max;
			delay += Time.unscaledDeltaTime;
			yield return 0;
		}
		music.volume = musicVolume * ProjectParameters.main.music_volume_max;
	}

	// A single sound effect
	public static void Shot(string clip) {
        Sound sound = main.GetSoundByName(clip);

        if (sound != null && !mixBuffer.Contains(clip)) {
            if (sound.clips.Count == 0) return;
			mixBuffer.Add(clip);
            main.sfx.PlayOneShot(sound.clips.GetRandom());
		}
	}

    // Turn on/off music
    public void MuteButton() {
        mute = !mute;
        PlayerPrefs.SetInt("Mute", mute ? 1 : 0);
        PlayerPrefs.Save();
        PlayMusic(mute ? "" : currentTrack);
    }

    public void ChangeMusicVolume(float v) {
        musicVolume = v;
        music.volume = musicVolume * ProjectParameters.main.music_volume_max;
    }

    public void ChangeSFXVolume(float v) {
        sfxVolume = v;
        sfx.volume = sfxVolume;
    }

    [System.Serializable]
    public class MusicTrack {
        public string name;
        public AudioClip track;
    }

    [System.Serializable]
    public class Sound {
        public string name;
        public List<AudioClip> clips = new List<AudioClip>();
    }
}

