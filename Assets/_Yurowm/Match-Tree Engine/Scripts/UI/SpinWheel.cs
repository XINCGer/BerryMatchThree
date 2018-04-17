using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpinWheel : MonoBehaviour {

    public Button spinButton;
    public Button stopButton;

    public GameObject freeCounter;
    public GameObject spinCounter;
    public GameObject seedCounter;

    public GameObject locker;

    public Image rewardIcon;


    public Transform wheel;
    public Animation cursor;
    public float speed = 45;
    public int spinCost = 30;
    public static int _spinCost = 0;
    public string cursorClipName;
    
    int target = 0;
    float target_angle;
    float current_angle;
    float intrigue;
    public static string lastReward = "";

    void Awake() {
        spinButton.onClick.AddListener(Spin);
        cursor[cursorClipName].speed = 0;
        _spinCost = spinCost;

        for (int i = 0; i < 8; i++)
            wheel.Find("Icon" + i).GetComponent<Image>().sprite = ProjectParameters.main.spinWheelRewards[i].icon;
    }

    void UpdateCounters() {
        freeCounter.SetActive(false);
        spinCounter.SetActive(false);
        seedCounter.SetActive(false);
        spinButton.gameObject.SetActive(true);

        if (ProfileAssistant.main.local_profile.daily_raward < System.DateTime.Now)
            freeCounter.SetActive(true);
        else if (ProfileAssistant.main.local_profile["spin"] >= 1)
            spinCounter.SetActive(true);
        else 
            seedCounter.SetActive(true);
    }

    void OnEnable () {
        StopAllCoroutines();

        wheel.transform.localRotation = Quaternion.Euler(0, 0, 45 * Random.Range(0, 8));
        spinButton.gameObject.SetActive(true);
        spinButton.transform.localScale = Vector3.one;
        stopButton.gameObject.SetActive(false);
        stopButton.transform.localScale = Vector3.one;
        locker.SetActive(false);

        UpdateCounters();
    }
	
	// Update is called once per frame
	IEnumerator WheelRoutine () {
        if (ProfileAssistant.main.local_profile.daily_raward < System.DateTime.Now) {
            System.DateTime next_reward = System.DateTime.Now;
            if (next_reward.Hour > ProfileAssistant.main.local_profile.daily_raward.Hour)
                next_reward = next_reward.AddDays(1);

            ProfileAssistant.main.local_profile.daily_raward = new System.DateTime(
                next_reward.Year, next_reward.Month, next_reward.Day,
                ProjectParameters.main.dailyreward_hour, 0, 0);
        } else if (ProfileAssistant.main.local_profile["spin"] >= 1) {
            ProfileAssistant.main.local_profile["spin"]--;
            ItemCounter.RefreshAll();
        } else if (ProfileAssistant.main.local_profile["seed"] >= spinCost) {
            ProfileAssistant.main.local_profile["seed"] -= spinCost;
            ItemCounter.RefreshAll();
        } else {
            UIAssistant.main.ShowPage("Store");
            yield break;
        }

        ProfileAssistant.main.SaveUserInventory();

        UpdateCounters();

        Animation anim;
        locker.SetActive(true);

        anim = spinButton.GetComponent<Animation>();
        anim.Play("SWButtonHide");
        while (anim.isPlaying)
            yield return 0;
        spinButton.gameObject.SetActive(false);

        speed = 0;
        while (speed < 200) {
            speed = Mathf.MoveTowards(speed, 200, Time.deltaTime * 90);
            wheel.Rotate(0, 0, -speed * Time.deltaTime);
            yield return 0;
        }

        stopButton.gameObject.SetActive(true);
        anim = stopButton.GetComponent<Animation>();
        anim.Play("SWButtonShow");

        stopButton.onClick.RemoveAllListeners();

        bool stoped = false;
        UnityAction stop = () => {
            stoped = true;
        };

        stopButton.onClick.AddListener(stop);

        while (!stoped) {
            wheel.Rotate(0, 0, -speed * Time.deltaTime);
            yield return 0;
        }

        stopButton.onClick.RemoveAllListeners();
        anim.Play("SWButtonHide");

        int total_probability = 0;
        foreach (SpinWheelReward reward in ProjectParameters.main.spinWheelRewards)
            total_probability += reward.probability;
        target = Random.Range(0, total_probability);
        for (int i = 0; i < 8; i++) {
            target -= ProjectParameters.main.spinWheelRewards[i].probability;
            if (target <= 0) {
                target = i;
                break;
            }
        }

        target_angle = target * 45 - 360;
        intrigue = -10f + (Random.value > 0.5f ? 22.5f : -22.5f);
        target_angle -= 360 * 3;

        current_angle = wheel.eulerAngles.z;

        while (intrigue != 0 || current_angle != target_angle + intrigue) {
            speed = Mathf.MoveTowards(speed, Mathf.Min(200, Mathf.Abs(current_angle - intrigue - target_angle) / 3 + 2), Time.deltaTime * 2f * (Mathf.Abs(speed) + 5f));
            current_angle = Mathf.MoveTowards(current_angle, target_angle + intrigue, speed * Time.deltaTime);
            wheel.transform.eulerAngles = Vector3.forward * current_angle;
            if (current_angle == target_angle + intrigue)
                intrigue = 0;
            yield return 0;
        }

        stopButton.gameObject.SetActive(false);
        locker.SetActive(false);

        rewardIcon.sprite = ProjectParameters.main.spinWheelRewards[target].icon;

        List<string> report = new List<string>();

        foreach (string reward in ProjectParameters.main.spinWheelRewards[target].items) {
            string[] args = reward.Split(':');
            int count = int.Parse(args[1]);
            ProfileAssistant.main.local_profile[args[0]] += count;
            report.Add(LocalizationAssistant.main[BerryStoreAssistant.main.items.Find(x => x.id == args[0]).localization_name] + (count > 1 ? "(x" + count.ToString() + ")" : ""));
        }

        lastReward = string.Join(", ", report.ToArray());

        ProfileAssistant.main.SaveUserInventory();
        
        UpdateCounters();

        ItemCounter.RefreshAll();

        UIAssistant.main.ShowPage("SpinWheelReward");
		
    }

    void Update() {
        cursor[cursorClipName].time = 1f - (wheel.eulerAngles.z % 45) / 45;
        cursor.Sample();
    }

    void Spin() {
        StartCoroutine(WheelRoutine());
    }
}

[System.Serializable]
public class SpinWheelReward {
    public Sprite icon;
    public List<string> items = new List<string>();
    public int probability = 1;
}
