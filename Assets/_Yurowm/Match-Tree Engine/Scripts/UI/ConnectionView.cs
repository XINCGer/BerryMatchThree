using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (CPanel))]
public class ConnectionView : MonoBehaviour {
    public static ConnectionView main;

    CPanel cpanel;

    public string minimizingClipName = "";
    public string maximizingClipName = "";

    public Text messageLabel;
    public GameObject connectionView;
    public GameObject messageView;

    bool isMinimized = false;

	void Awake () {
        main = this;
        cpanel = GetComponent<CPanel>();
	}

    public void Minimize() {
        if (isMinimized || cpanel.isPlaying)
            return;
        isMinimized = true;
        cpanel.PlayClip(minimizingClipName);
    }

    public void Maximize() {
        if (!isMinimized || cpanel.isPlaying)
            return;
        isMinimized = false;
        cpanel.PlayClip(maximizingClipName);
    }

    public void ShowMessage(string message) {
        if (!gameObject.activeInHierarchy)
            return;
        if (isMinimized)
            Maximize();
        connectionView.SetActive(false);
        messageView.SetActive(true);
        messageLabel.text = message;
    }

    public static void Show() {
        main = UIAssistant.main.panels.Find(x => x.name == "Connection").GetComponent<ConnectionView>();

        Animation anim = main.GetComponent<Animation>();
        anim.Play(main.maximizingClipName);
        anim.enabled = true;
        anim[main.maximizingClipName].time = anim[main.maximizingClipName].length;
        anim.Sample();
        anim.enabled = false;

        UIAssistant.main.SetPanelVisible(main.name, true);
        UIAssistant.main.FreezPanel(main.name);
        main.connectionView.SetActive(true);
        main.messageView.SetActive(false);
        main.isMinimized = false;
    }

    public static void Hide() {
        UIAssistant.main.SetPanelVisible(main.name, false);
        main.connectionView.SetActive(true);
        main.messageView.SetActive(false);
        main.isMinimized = false;
    }

    public void Close() {
        Hide();
    }
}
