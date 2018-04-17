using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SpriteRenderer))]
public class Jam : MonoBehaviour {
    
    public string type = "";
    public Sprite jamAsprite;


    public string crush_effect;

    void Start () {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (type == "Jam A")
            sr.sprite = jamAsprite;
        AudioAssistant.Shot("WeedCreate");
    }

    public static void JamIt(Slot slot, string jamType) {
        if (!slot || jamType == "")
            return;

        if (slot.jam) {
            if (slot.jam.type == jamType)
                return;
            Destroy(slot.jam.gameObject);
        }

        AddNew(slot, jamType);
        slot.jam.type = jamType;        
    }

    public static string GetType(Slot slot) {
        if (!slot || !slot.jam)
            return "";
        return slot.jam.type;
    }


    static void AddNew(Slot slot, string type) {
        Jam jam = ContentAssistant.main.GetItem<Jam>("Jam");
        jam.type = type;
        jam.transform.SetParent(slot.transform);
        jam.transform.localPosition = Vector3.zero;
        jam.name = "Jam_" + slot.coord.x + "x" + slot.coord.y;
        slot.jam = jam;
    }
}
