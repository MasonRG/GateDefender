using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelector : MonoBehaviour {

	private GameController gc;
	public TowerUI towerUI;

	void Start() {
		gc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
	}
		

	void Update() {

		//Do not be active if we have a ghost active
		if (gc.ghostManager.isActive) {
			Deselect();
			return;
		}

		//Deselect no matter what we are pointing at
		if (Input.GetMouseButtonDown(Common.RIGHT_MOUSE)) {
			Deselect();
			return;
		}

		//Don't need to check for selection if we are over a UI element
		if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			return;

		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit contact;
		const float max_cast_length = 100f;

		if (Physics.Raycast(mouseRay, out contact, max_cast_length, Common.Layers.all)) {

			if (Input.GetMouseButtonDown(Common.LEFT_MOUSE)) {

				if (contact.transform.tag.Equals("Tower")) { //select

					if (contact.transform.gameObject != gc.selectedObject) //Don't reselect if it is the same object
						Select(contact.transform.gameObject);

					Tower selectedTower = gc.selectedObject.GetComponent<Tower>();
					gc.DrawRange(gc.selectedObject.transform.position, selectedTower.towerType, selectedTower.upgradeTier);
				}
				else if ((contact.transform.tag.Equals("Ground") || contact.transform.tag.Equals("Path"))) { //deselect
					Deselect();
				}

			}
				
		}
	}

	void Select(GameObject obj) {
		gc.selectedObject = obj;
		towerUI.gameObject.SetActive(false);
		if (!towerUI.gameObject.activeSelf && !towerUI.isAnimating) {
			towerUI.gameObject.SetActive(true);
			towerUI.transform.position = obj.transform.position;
			towerUI.StartAnimating(true);
		}
	}

	void Deselect() {
		gc.selectedObject = null;
		if (towerUI.gameObject.activeSelf && !towerUI.isAnimating)
			towerUI.StartAnimating(false);
	}
}
