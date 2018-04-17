using UnityEngine;
using System.Collections;

// Management of the main camera
public class GameCamera : MonoBehaviour {

	public static GameCamera main;

    public static Camera cam;
    public bool playing = false;

    void Awake() {
        main = this;
        cam = GetComponent<Camera>();
        UIAssistant.onScreenResize += OnScreenResize;
        OnScreenResize();
    }

    public void OnScreenResize() {
        if (!FieldArea.main)
            return;

        FieldArea.main.UpdateParameters();

        StopAllCoroutines();
        StartCoroutine(ResizingRoutine());
    }

    IEnumerator ResizingRoutine() {
        FieldArea.main.UpdateParameters();
        
        float targetSize = GetTargetSize();

        Vector3 targetPosition = new Vector3(-2f * FieldArea.position.x / FieldArea.screen_size.x, -2f * FieldArea.position.y / FieldArea.screen_size.y, -10);
        targetPosition.x *= targetSize * Screen.width / Screen.height;
        targetPosition.y *= targetSize;

        float speed = Vector3.Distance(targetPosition, transform.position) * 3;
        speed = Mathf.Max(speed, 3);

        while (playing && (targetPosition != transform.position || cam.orthographicSize != targetSize)) {
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, targetSize, Time.unscaledDeltaTime * speed);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.unscaledDeltaTime * speed);
            yield return 0;
        }
    }

    float GetTargetSize() {
        float width = FieldAssistant.main.field.width * ProjectParameters.main.slot_offset * (FieldArea.screen_size.x / FieldArea.size.x) * 0.5f * Screen.height / Screen.width;
        float height = FieldAssistant.main.field.height * ProjectParameters.main.slot_offset * (FieldArea.screen_size.y / FieldArea.size.y) * 0.5f;

        return width > height ? width : height;
    }

	// Switching to the display of the playing field
	public void ShowField (){
		StartCoroutine (ShowFieldRoutine ());
	}

	// Switching to display the game menu
	public void HideField (){
		StartCoroutine (HideFieldRoutine ());
	}
		
	// Coroutine of displaying of field
	public IEnumerator ShowFieldRoutine ()
	{

        if (playing)
            yield break;
        playing = true;

        while (FieldArea.main == null)
            yield return 0;

        FieldArea.main.UpdateParameters();


        float t = 0;

        float targetSize = GetTargetSize();
        Vector3 targetPosition = new Vector3(-2f * FieldArea.position.x / FieldArea.screen_size.x, -2f * FieldArea.position.y / FieldArea.screen_size.y, -10);
        targetPosition.x *= targetSize * Screen.width / Screen.height;
        targetPosition.y *= targetSize;

        cam.orthographicSize = targetSize;

        Vector3 position = new Vector3(0,10, -10);
        while (t < 1) {
            t += (-Mathf.Abs(0.5f - t) + 0.5f + 0.05f) * Time.unscaledDeltaTime * 6;
            transform.position = Vector3.Lerp(position, targetPosition, t);
            yield return 0;
        }
    }

	// Coroutine of displaying of game menu
	public IEnumerator HideFieldRoutine () {
        if (!playing)
            yield break;

        playing = false;
        
        float t = 0;

        Vector3 position = transform.position;

        while (t < 1) {
            t += (-Mathf.Abs(0.5f - t) + 0.5f + 0.05f) * Time.unscaledDeltaTime * 6;
            transform.position = Vector3.Lerp(position, new Vector3(0, 10, -10), t);
            yield return 0;
        }


        yield break;
	}
}