using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Berry.Utils;
public class ServiceAssistant : MonoBehaviour {
    public static ServiceAssistant main;

    bool rate_it_showed = false;
    bool daily_reward_showed = false;
    
    void Awake() {
        if (Application.isEditor)
            Application.runInBackground = true;
        main = this;
        main.enabled = false;
        UIAssistant.onShowPage += LevelMapPopup;
        rate_it_showed = PlayerPrefs.GetInt("Rated") == 1;
    }


    void LevelMapPopup(string page) {
        if (!main.enabled) return;
        StartCoroutine(LevelMapPopupRoutine(page));
    }

    IEnumerator LevelMapPopupRoutine(string page) {
        if (page != "LevelList")
            yield break;

        yield return 0;

        while (CPanel.uiAnimation > 0)
            yield return 0;
        if (UIAssistant.main.GetCurrentPage() != page)
            yield break;

        yield return 0;

        // Daily Reward
        if (!daily_reward_showed && ProfileAssistant.main.local_profile.daily_raward < System.DateTime.Now) {
            daily_reward_showed = true;
            UIAssistant.main.ShowPage("SpinWheel");
            yield break;
        }

        // Rate It
        if (!rate_it_showed) {
            if (ProfileAssistant.main.local_profile.current_level < 10)
                yield break;
            if (UnityEngine.Random.value > 0.3f)
                yield break;
            UIAssistant.main.ShowPage("RateIt");
            yield break;
        }
    }

    public void RateIt() {
        string link = GetAppLink(Application.platform);
        if (link != "")
            Application.OpenURL(link);
        rate_it_showed = true;
        PlayerPrefs.SetInt("Rated", 1);
        UIAssistant.main.ShowPage("LevelList");
    }

    public void DownloadUpdate() {
        string link = GetAppLink();
        if (link != "")
            Application.OpenURL(link);
        UIAssistant.main.SetPanelVisible("NewVersion", false);
    }

    public void Later() {
        rate_it_showed = true;
        UIAssistant.main.ShowPage("LevelList");
    }

    public void NoThanks() {
        rate_it_showed = true;
        PlayerPrefs.SetInt("Rated", 1);
        UIAssistant.main.ShowPage("LevelList");
    }

    public static string GetAppLink(bool native = true) {
        return GetAppLink(Application.platform, native);
    }
    public static string GetAppLink(RuntimePlatform platform, bool native = true)
    {
        return "";
    }

    public void Quit() {
        Application.Quit();
    }
}
