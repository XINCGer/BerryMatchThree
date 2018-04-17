using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// An important element of UI. It combines elements of the interface closest to the destination.
public class CPanel : MonoBehaviour {

    public static int uiAnimation = 0;
    public bool freez = false;
    bool _isPlaying = false;
    public bool isPlaying {
        get {
            return _isPlaying;
        }
        set {
            if (_isPlaying != value) {
                _isPlaying = value;
                uiAnimation += _isPlaying ? 1 : -1;
            }
        }
    }

	public string hide; // Name of showing animation
	public string show; // Name of hiding animation

	private string currentClip = "";

    Animation anim;
    void Awake() {
        anim = GetComponent<Animation>();
    }

    void OnEnable() {
        freez = false;
    }

    public void SetVisible(bool visible, bool immediate = false) {
        if (gameObject.activeSelf == visible)
            return;
        currentClip = "";
        if (!visible) {
            if (hide != "")
                currentClip = hide;
            else {
                gameObject.SetActive(false);
                return;
            }
        }
        if (visible) {
            gameObject.SetActive(true);
            if (show != "")
                currentClip = show;
            else
                return;
        }
        if (currentClip == "")
            return;
        if (immediate)
            anim[currentClip].time = anim[currentClip].length;
        else 
            Play(currentClip);
    }

    void Play(string clip) {
        StartCoroutine(PlayClipRoutine(clip));
    }

    public void PlayClip(string clip) {
        if (!isPlaying)
            Play(clip);
    }

    IEnumerator PlayClipRoutine(string clip) {
        isPlaying = true;
        
        anim.Play(clip);
        anim[clip].time = 0;

        while (anim[clip].time < anim[clip].length) {

            anim[clip].enabled = true;
            anim[clip].time += Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);
            anim.Sample();
            anim[clip].enabled = false;

            yield return 0;
        }

        isPlaying = false;
        if (clip == hide)
            gameObject.SetActive(false);
    }
}
	
