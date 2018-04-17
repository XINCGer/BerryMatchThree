using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class LinearAnimatior : MonoBehaviour {


    public bool rotZ = false;
    public float rotZampl = 0;
    public float rotZfreq = 0;
    float rotZoffset = 0;
    public float rotZphase = 0;
    public float rotZvelocity = 0;
    public bool sizeX = false;
    public float sizeXampl = 0;
    public float sizeXfreq = 0;
    public float sizeXphase = 0;
    float sizeXoffset = 1;
    public bool sizeY = false;
    public float sizeYampl = 0;
    public float sizeYfreq = 0;
    public float sizeYphase = 0;
    float sizeYoffset = 1;

    public bool posX = false;
    public float posXampl = 0;
    public float posXfreq = 0;
    float posXoffset = 1;
    public float posXvelocity = 0;
    public bool posY = false;
    public float posYampl = 0;
    public float posYfreq = 0;
    float posYoffset = 1;
    public float posYvelocity = 0;

    Vector3 z;

    void Awake() {
        Recalculate();
    }

    void Recalculate() {
        sizeXoffset = transform.localScale.x;
        sizeYoffset = transform.localScale.y;
        rotZoffset = transform.localEulerAngles.z;
        posXoffset = transform.localPosition.x;
        posYoffset = transform.localPosition.y;
    }

	void Update () {

        if (rotZ)
            transform.localEulerAngles = Vector3.forward * (rotZoffset + Mathf.Sin(rotZfreq * (rotZphase + Time.unscaledTime)) * rotZampl + rotZvelocity * Time.unscaledTime);

        if (sizeX || sizeY) {
            z = transform.localScale;

            if (sizeX)
                z.x = sizeXoffset + Mathf.Sin(sizeXphase + sizeXfreq * Time.unscaledTime) * sizeXampl;
            if (sizeY)
                z.y = sizeYoffset + Mathf.Sin(sizeYphase + sizeYfreq * Time.unscaledTime) * sizeYampl;

            transform.localScale = z;
        }

        if (posX || posY) {
            z = transform.localPosition;

            if (posX)
                z.x = posXoffset + Mathf.Sin(posXfreq * Time.unscaledTime) * posXampl;
            if (posY)
                z.y = posYoffset + Mathf.Sin(posYfreq * Time.unscaledTime) * posYampl;

            transform.localPosition = z;
        }

	}
}
