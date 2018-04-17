using UnityEngine;
using System.Collections;

public abstract class IBoosterLogic : MonoBehaviour {
    BoosterButton booster;
    public bool turnOffController = true;

    void Awake() {
        UIAssistant.onShowPage += Disable;
    }

    public void Disable(string page) {
        Disable();
        UIAssistant.onShowPage -= Disable;
        Destroy(gameObject);
    }

    public virtual void Disable() {

    }

    void OnEnable() {
        if (turnOffController)
            TurnController(false);
        StartCoroutine(Logic());
    }

    public abstract IEnumerator Logic();

    void OnDisable() {
        if (turnOffController)
            TurnController(true);
    }

    // Enable/Disable ControlAssistant
    void TurnController(bool b) {
        if (ControlAssistant.main == null)
            return;
        ControlAssistant.main.enabled = b;
    }

}