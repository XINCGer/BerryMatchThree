using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections.Generic;
using Berry.Utils;

[RequireComponent (typeof (Text))]
public class TargetDescriptionDisplay : MonoBehaviour, INeedLocalization {
	
	Text text;

    public List<string> RequriedLocalizationKeys() {
        List<string> result = new List<string>();
        foreach (string target in Enum.GetNames(typeof (FieldTarget)))
            result.Add("targetdescription_" + target);
        foreach (string target in Enum.GetNames(typeof(FieldTarget)))
            result.Add("targetmodename_" + target);
        foreach (string target in Enum.GetNames(typeof(Limitation)))
            result.Add("limitationdescription_" + target);
        return result;
    }

    void Awake () {
		text = GetComponent<Text> ();	
	}
	
	void OnEnable () {
        text.text = "";
		if (LevelProfile.main == null)
			return;

        string descrition = LocalizationAssistant.main["targetdescription_" + LevelProfile.main.target];

		switch (LevelProfile.main.target) {
			case FieldTarget.None: descrition = string.Format(descrition, LevelProfile.main.firstStarScore); break;
			case FieldTarget.Jelly: descrition = string.Format(descrition, LevelProfile.main.slots.Count(x => x.jelly_level > 0)); break;
			case FieldTarget.Block: descrition = string.Format(descrition, LevelProfile.main.slots.Count(x => x.block_type == "Block")); break;
			case FieldTarget.SugarDrop: descrition = string.Format(descrition, LevelProfile.main.targetSugarDropsCount); break;
        }

        text.text += descrition + " ";
        descrition = LocalizationAssistant.main["limitationdescription_" + LevelProfile.main.limitation];

        switch (LevelProfile.main.limitation) {
		    case Limitation.Moves: descrition = string.Format(descrition, LevelProfile.main.limit); break;
		    case Limitation.Time: descrition = string.Format(descrition, Utils.ToTimerFormat(LevelProfile.main.limit)); break;
		}

		text.text += descrition;
	}
}
