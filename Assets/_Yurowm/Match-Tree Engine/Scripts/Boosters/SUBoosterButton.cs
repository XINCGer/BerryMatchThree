using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Class button activation booster
[RequireComponent(typeof(Button))]
public class SUBoosterButton : MonoBehaviour {

    public string boosterItemId;
    // Mask of displaying booster depending on the limitation mode

    public GameObject selected;
    public GameObject unselected;

    public Limitation[] limitationMask;
    Button button;

    public static List<string> bag = new List<string>();
    bool _equiped = true;
    bool equiped {
        get {
            return _equiped;
        }
        set {
            if (_equiped != value) {
                _equiped = value;
                selected.SetActive(_equiped);
                unselected.SetActive(!_equiped);
                if (_equiped && !bag.Contains(boosterItemId))
                    bag.Add(boosterItemId);
                if (!_equiped && bag.Contains(boosterItemId))
                    bag.Remove(boosterItemId);
            }
        }
    }

    void Awake() {
        SendMessage("BoosterInitialize", SendMessageOptions.DontRequireReceiver);
        button = GetComponent<Button>();
        foreach (ItemCounter counter in GetComponentsInChildren<ItemCounter>(true)) {
            counter.itemID = boosterItemId;
            counter.Refresh();
        }
        foreach (ItemMask mask in GetComponentsInChildren<ItemMask>()) {
            mask.itemID = boosterItemId;
            mask.Refresh();
        }
        button.onClick.AddListener(() => {
            OnClick();
        });
        equiped = false;
    }

    void OnEnable() {
        equiped = bag.Contains(boosterItemId);
    }


    void OnClick() {
        if (ProfileAssistant.main.local_profile[boosterItemId] == 0) {
            BoosterButton.boosterSelectedId = boosterItemId;
            UIAssistant.main.ShowPage("Store");
        } else
            equiped = !equiped;
    }

    public static void Generate(Transform parent) {
        foreach (string booster in bag) {
            GameObject copy = null;
            switch (booster) {
                case "bombs": copy = ContentAssistant.main.GetItem("SUBoosterBombs"); break;
                case "rainbow": copy = ContentAssistant.main.GetItem("SUBoosterRainbow"); break;
                case "ladybird": copy = ContentAssistant.main.GetItem("SUBoosterLadybird"); break;
            }
            if (copy)
                copy.transform.parent = parent;
        }
    }
}
