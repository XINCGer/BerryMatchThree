using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Berry.Utils;

public class ControlAssistant : MonoBehaviour {
	
	
	public static ControlAssistant main;
	RaycastHit2D hit;
	public Camera controlCamera;
	
	Slot pressedSlot;
	Vector2 pressPoint;
	
	bool isMobilePlatform;

    public static System.Action<Chip, Side> swap = delegate {};
	
	void  Awake (){
		main = this;
		isMobilePlatform = Application.isMobilePlatform;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        swap += Chip.Swap;
	}

	void  Update (){
		if (Time.timeScale == 0) return;
		if (isMobilePlatform)
			MobileUpdate();
		else 
			DecktopUpdate();
	}
	
	// control function for mobile devices
	void  MobileUpdate (){
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            if (!EventSystem.current) return;
            if (EventSystem.current.IsPointerOverGameObject(-1)) return;
            if (EventSystem.current.IsPointerOverGameObject(0)) return;
			Vector2 point = controlCamera.ScreenPointToRay(Input.GetTouch(0).position).origin;
			hit = Physics2D.Raycast(point, Vector2.zero);
			if (!hit.transform) return;
			pressedSlot = hit.transform.GetComponent<Slot>();
			pressPoint = Input.GetTouch(0).position;
		}
		if (Input.touchCount > 0 && pressedSlot) {
			Vector2 move = Input.GetTouch(0).position - pressPoint;
			if (move.magnitude > Screen.height * 0.05f) {
                foreach (Side side in Utils.straightSides)
                    if (Vector2.Angle(move, Utils.SideOffsetX(side) * Vector2.right + Utils.SideOffsetY(side) * Vector2.up) <= 45)
                        if (pressedSlot.chip)
                            swap.Invoke(pressedSlot.chip, side);
				pressedSlot = null;
			}
		}
	}
	
	// Control function for stationary platforms
	void  DecktopUpdate (){
		if (Input.GetMouseButtonDown(0)) {
            if (EventSystem.current.IsPointerOverGameObject(-1)) return;
            if (EventSystem.current.IsPointerOverGameObject(0)) return;
			Vector2 point = controlCamera.ScreenPointToRay(Input.mousePosition).origin;
			hit = Physics2D.Raycast(point, Vector2.zero);
			if (!hit.transform) return;
			pressedSlot = hit.transform.GetComponent<Slot>();
			pressPoint = Input.mousePosition; 
		}
		if (Input.GetMouseButton(0) && pressedSlot != null) {
			Vector2 move = Input.mousePosition;
			move -= pressPoint;
			if (move.magnitude > Screen.height * 0.05f) {
                foreach (Side side in Utils.straightSides)
                    if (Vector2.Angle(move, Utils.SideOffsetX(side) * Vector2.right + Utils.SideOffsetY(side) * Vector2.up) <= 45)
                        if (pressedSlot.chip) 
                            swap.Invoke(pressedSlot.chip, side);

				pressedSlot = null;
			}
		}
	}
	
	public Slot GetSlotFromTouch() {
		Vector2 point;
		if (isMobilePlatform) {
			if (Input.touchCount == 0) return null;
			point = controlCamera.ScreenPointToRay(Input.GetTouch(0).position).origin;
		} else 
			point = controlCamera.ScreenPointToRay(Input.mousePosition).origin;
		
		hit = Physics2D.Raycast(point, Vector2.zero);
		if (!hit.transform) return null;
		return hit.transform.GetComponent<Slot>();
	}
}