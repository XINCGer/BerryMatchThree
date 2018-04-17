using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapLocation : MonoBehaviour {
    public Transform nextLocationConnector;
    public Transform previousLocationConnector;
    [HideInInspector]
    public int number = 0;
    MapLocation nextLocation = null;
    MapLocation previousLocation = null;

    LevelMap map;
    Rect mapRect;

    public Sprite background;

    void Start() {
        LevelMap.main.AddLocation(this);
        OnPositionChanged();
        CreateButtons();
    }

    void CreateButtons() {
        Transform locators_folder = transform.Find("Buttons");
        if (!locators_folder) {
            Debug.LogError("I can't find Buttons folder");
            return;
        }
        Transform connector;
        LevelButton level_button;
        int level;
        int firstLevel = LevelMap.main.locationLevelNumber[number];
        int lastCount = GetLevelCount();
        for (int l = 0; l < lastCount; l++) {
            level = firstLevel + 1 + l;
            if (locators_folder.childCount <= l) return;
            connector = locators_folder.GetChild(l);
            if (!connector || !Level.all.ContainsKey(level))
                return;

            level_button = ContentAssistant.main.GetItem<LevelButton>("LevelButton");
            level_button.transform.parent = connector;
            level_button.transform.localPosition = Vector3.zero;
            level_button.level = level;
            //level_button.GetComponent<Canvas>().worldCamera = LevelMap2.main.mapCamera;
            level_button.Initialize();
        }
        
    }

    public void OnPositionChanged() {
        if (LevelMap.main == null) return;
        int p = LevelMap.main.IsVisible(previousLocationConnector);
        int n = LevelMap.main.IsVisible(nextLocationConnector);
        if (n != 0 && p != 0 && p == n) {
            LevelMap.main.RemoveLocation(this);
            Destroy(gameObject);
            return;
        }
        if (nextLocation == null) {
            if (n == 0) {
                nextLocation = LevelMap.main.ShowNextLocation(this);
            }
        }
        if (previousLocation == null) {
            if (p == 0) {
                previousLocation = LevelMap.main.ShowPreviuosLocation(this);
            }
        }
    }

    public int GetLevelCount() {
        return transform.Find("Buttons").childCount;    
    }

    public void ApplyBackground() {
        FieldBackground.background = background;
        UIAssistant.onScreenResize.Invoke();
    }
}
