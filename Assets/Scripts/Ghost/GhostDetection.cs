using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostDetection : MonoBehaviour {

	private GameController gc;
	public Renderer baseRenderer;
	public Renderer turretRenderer;

	public void Initialize(GameController gc, float checkRadius) {
		this.gc = gc;
		GetComponent<SphereCollider>().radius = checkRadius;
	}


	void OnEnable() {
		//Reset to defaults when enabling, bury the ghost under the map so it is not visible on first frame
		transform.position = new Vector3(0f,-10f, 0f);
		if (gc != null) {
			gc.rangeTracer.transform.position = transform.position;
			gc.ghostManager.isColliding = false;
		}
	}


	void Update() {

		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit contact;
		const float max_cast_length = 100f;

		bool mouseWithinBounds = Physics.Raycast(mouseRay, out contact, max_cast_length, Common.Layers.ground|Common.Layers.path);


		gc.ghostManager.isValid =
			(
				!gc.ghostManager.isColliding &&
				!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() &&
				mouseWithinBounds &&
				contact.transform.tag.Equals("Ground") &&
				gc.HasEnoughMoneyToBuild()
			);

		//Update position
		if (mouseWithinBounds)
			transform.position = contact.point;


		//Check for cancel
		if (Input.GetMouseButtonDown(Common.RIGHT_MOUSE))
			gc.DisableTowerGhost();


		//Set green shader if valid and permit placement
		if (gc.ghostManager.isValid) {
			ChangeColor(true);
			if (Input.GetMouseButtonDown(Common.LEFT_MOUSE)) {
				gc.SpawnTower(transform.position);
			}
				
		}
		//Set red shader if invalid
		else
			ChangeColor(false);


		//Draw the range
		gc.DrawRange(transform.position, gc.ghostManager.GetActiveType(), Tower.TIER_0);

	}

	void ChangeColor(bool valid) {
		Color c1 = (valid)? gc.ghostManager.validBaseColor : gc.ghostManager.invalidBaseColor;
		Color c2 = (valid)? gc.ghostManager.validTurretColor : gc.ghostManager.invalidTurretColor;
		baseRenderer.material.SetColor("_Tint", c1);
		turretRenderer.material.SetColor("_Tint", c2);
	}

	void OnTriggerStay(Collider other) {
		if (other.tag.Equals("Tower")) {
			gc.ghostManager.isColliding = true;
		}
	}
	void OnTriggerExit(Collider other) {
		if (other.tag.Equals("Tower")) {
			gc.ghostManager.isColliding = false;
		}
	}
}
