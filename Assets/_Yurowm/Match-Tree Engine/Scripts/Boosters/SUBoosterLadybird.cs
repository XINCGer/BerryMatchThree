using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

public class SUBoosterLadybird : MonoBehaviour {
    void Start() {
        string id = "ladybird";
        ProfileAssistant.main.local_profile[id]--;
        SUBoosterButton.bag.Remove(id);
        StartCoroutine(BoosterRountine());
    }

    IEnumerator BoosterRountine() {
        int total = 10;
        int count = total;
        while (count > 0) {
            yield return StartCoroutine(Utils.WaitFor(SessionAssistant.main.CanIWait, 0.3f));
            GameObject ladybird = ContentAssistant.main.GetItem("Ladybird" + Chip.chipTypes.GetRandom());
            Vector3 position = new Vector3();
            position.x = Random.Range(-0.5f, 0.5f) * LevelProfile.main.width * ProjectParameters.main.slot_offset;
            position.y = (0.5f * LevelProfile.main.height + 2) * ProjectParameters.main.slot_offset;

            ladybird.transform.position = position;
            SessionAssistant.main.EventCounter();
            ladybird.GetComponent<Chip>().DestroyChip();
            count--;
            
            while (SessionAssistant.main.GetResource() * total >= count)
                yield return 0;
        }

        Destroy(gameObject);
    }
}
