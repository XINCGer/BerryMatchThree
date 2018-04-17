using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

[RequireComponent (typeof (Dropdown))]
public class LanguagesDropdownMenu : MonoBehaviour {


    Dropdown menu;

    void Awake () {
        menu = GetComponent<Dropdown>();
        menu.options = LocalizationAssistant.main.languages.Select(x => new Dropdown.OptionData(x.ToString())).ToList();
        menu.value = LocalizationAssistant.main.languages.IndexOf(LocalizationAssistant.main.current_language);
        menu.onValueChanged.AddListener(OnValueChanged);
    }
	
	// Update is called once per frame
	void OnValueChanged (int value) {
        LocalizationAssistant.main.LearnLanguage(LocalizationAssistant.main.languages[value]);
        ItemCounter.RefreshAll();
    }
}
