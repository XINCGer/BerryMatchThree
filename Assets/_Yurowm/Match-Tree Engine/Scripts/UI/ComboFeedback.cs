using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Berry.Utils;

public class ComboFeedback : MonoBehaviour {

    public static ComboFeedback main;

    Text text;

    Animation anim;

    void Awake() {
        main = this;
        anim = GetComponent<Animation>();
        text = transform.GetComponentInChildren<Text>();
        text.gameObject.SetActive(false);
    }

    void OnEnable() {
        StartCoroutine(PlayRoutine());
    }
    
    IEnumerator PlayRoutine() {
        int last_swap = SessionAssistant.main.swapEvent;
        int last_match_count = SessionAssistant.main.matchCount;


        while (true) {
            if (SessionAssistant.main.matchCount - last_match_count >= 3) {
                Feedback feedback = ProjectParameters.main.feedbacks.GetRandom();
                text.gameObject.SetActive(true);
                text.text = feedback.text;
                anim.Play();
                AudioAssistant.Shot(feedback.audioClipName);
                while (anim.isPlaying)
                    yield return 0;
            }
            text.gameObject.SetActive(false);
            
            while (SessionAssistant.main.swapEvent <= last_swap)
                yield return 0;
            
            last_match_count = SessionAssistant.main.matchCount;
            last_swap = SessionAssistant.main.swapEvent;

            yield return 0;

            yield return StartCoroutine(Utils.WaitFor(SessionAssistant.main.CanIWait, 0.5f));
        }
    }

    [System.Serializable]
    public class Feedback {
        public string text = "";
        public string audioClipName = "";
    }
}
