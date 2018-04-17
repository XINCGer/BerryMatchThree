using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Berry.Utils;

[RequireComponent (typeof (Text))]
public class DinaLabel : MonoBehaviour, INeedLocalization {

    public static Dictionary<string, Word> words = new Dictionary<string,Word>();
    public static bool initialized = false;

    Text label;

    public bool localized = false;
    public string key;
    public string text;
    public bool update = false;
    public float delay = 0.2f;
    float lastTime = 0;
    public List<Mask> masks = new List<Mask>();

	void Awake () {
        if (!initialized)
            Initialize();
        label = GetComponent<Text>();
        ItemCounter.refresh += UpdateLabel;
	}

    public static void Initialize() {
        words.Add("CurrentLevel", () => {return LevelProfile.main.level.ToString();});
        words.Add("CurrentScore", () => {return SessionAssistant.main.score.ToString();});
        words.Add("FirstStarScore", () => {return LevelProfile.main.firstStarScore.ToString();});
        words.Add("SecondStarScore", () => {return LevelProfile.main.secondStarScore.ToString();});
        words.Add("ThirdStarScore", () => {return LevelProfile.main.thirdStarScore.ToString();});
        words.Add("BestScore", () => {return ProfileAssistant.main.local_profile.GetScore(LevelProfile.main.level).ToString();});
        words.Add("BlockCount", () => {return GameObject.FindObjectsOfType<Block> ().Length.ToString();});
        words.Add("BlockCountTotal", () => {return SessionAssistant.main.blockCountTotal.ToString();});
        words.Add("JellyCount", () => {return Slot.all.Count(x => x.Value.jelly).ToString();});
        words.Add("JellyCountTotal", () => {return SessionAssistant.main.jellyCountTotal.ToString();});
        words.Add("JamCount", () => {return Slot.all.Count(x => Jam.GetType(x.Value) == "Jam A").ToString();});
        words.Add("JamCountTotal", () => {return Slot.all.Count.ToString();});
        words.Add("SugarCount", () => {return SessionAssistant.main.targetSugarDropsCount.ToString();});
        words.Add("SugarCountTotal", () => {return LevelProfile.main.targetSugarDropsCount.ToString();});
        words.Add("CurrentMoves", () => {return SessionAssistant.main.movesCount.ToString();});
        words.Add("CurrentTime", () => {return Utils.ToTimerFormat(SessionAssistant.main.timeLeft);});
        words.Add("WaitingStatus", () => {return Utils.waitingStatus;});
        words.Add("TargetModeName", () => {return LocalizationAssistant.main["targetmodename_" + LevelProfile.main.target];});
        words.Add("BoosterSelectedName", () => {return BerryStoreAssistant.main.GetItemByID(BoosterButton.boosterSelectedId).name;});
        words.Add("BoosterSelectedPackDescription", () => {return LocalizationAssistant.main[BerryStoreAssistant.main.GetItemByID(BoosterButton.boosterSelectedId).localization_description];});
        words.Add("LifesCount", () => {return ProfileAssistant.main.local_profile["life"].ToString();});
        words.Add("SpinCost", () => {return SpinWheel._spinCost.ToString();});
        words.Add("LastReward", () => {return SpinWheel.lastReward;});
        words.Add("ColorCollections", () => {
            string r = "";
            foreach (int i in SessionAssistant.main.countOfEachTargetCount)
                r += (r.Length > 0 ? "," : "") + Mathf.Max(0, i).ToString();
            return r;
        });
        words.Add("NextLifeTimer", () => {
            if (ProfileAssistant.main.local_profile["life"] >= ProjectParameters.main.lifes_limit)
                return LocalizationAssistant.main["full"];
            System.TimeSpan span = ProfileAssistant.main.local_profile.next_life_time - System.DateTime.Now;
            if (span.TotalSeconds <= 0) return "00:00";
            return string.Format("{0:00}:{1:00}", span.Minutes, span.Seconds);
        });

        initialized = true;
    }
	
	void OnEnable () {
        UpdateLabel();
	}

    void Update () {
        if (!update) return;
        if (lastTime + delay > Time.unscaledTime) return;
        lastTime = Time.unscaledTime;
        UpdateLabel();
    }

    void UpdateLabel() {
        string result = GetText();
        foreach (Mask mask in masks)
            result = result.Replace("{" + mask.key + "}", words[mask.value].Invoke());
        label.text = result;
    }


    public string GetText() {
        return localized ? LocalizationAssistant.main[key] : text;
    }

    public List<string> RequriedLocalizationKeys() {
        List<string> result = new List<string>();
        foreach (string target in Enum.GetNames(typeof(FieldTarget)))
            result.Add("targetmodename_" + target);
        return result;
    }

    public delegate string Word();

    [System.Serializable]
    public class Mask {
        public string key = "";
        public string value = "";

        public Mask(string _key) {
            key = _key;
        }
    }
}
