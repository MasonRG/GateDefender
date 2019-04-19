using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerUI : MonoBehaviour {


	//Tower UI needs to face the camera at all times, and
	//scale with camera zoom level.

	private GameController gc;
	private Transform camTransform;
	public Transform canvasPivot;
	public Transform upgradeButton, sellButton;
	private Text upgradeText, sellText;

	private const float min_scale = 0.6f;
	public Vector2 heightLimits;
	public Vector2 moveLimitsZ;
	public Vector2 moveLimitsY;

	[HideInInspector]public bool isAnimating;

	private bool textUpdatedFlag = false; //used to only update the text once when a change is needed

	void Start() {
		gc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		camTransform = GameObject.FindWithTag("MainCamera").transform;
		upgradeText = upgradeButton.GetComponentInChildren<Text>();
		sellText = sellButton.GetComponentInChildren<Text>();
	}

	void Update() {

		if (gc.selectedObject != null && gc.selectedObject.tag == "Tower") {

			//Set the state of the buttons (either upgrade & sell or just sell)
			if (gc.selectedObject.GetComponent<Tower>().upgradeTier == Tower.TIER_2) {
				upgradeButton.gameObject.SetActive(false);
				Vector3 p = sellButton.localPosition;
				p.x = 0f;
				sellButton.localPosition = p;
			}
			else {
				upgradeButton.gameObject.SetActive(true);
				Vector3 p = sellButton.localPosition;
				p.x = -upgradeButton.localPosition.x;
				sellButton.localPosition = p;
			}

			if (!isAnimating)
				AdjustTransform();

			if (!textUpdatedFlag)
				AdjustText();
		}
	}
		
	void AdjustTransform() {
		//Properties to be set -> use lerping to smooth
		Quaternion newRotation;
		Vector3 newLocalPosition;
		Vector3 newScale;

		//point at camera; no z-rot or y-rot allowed
		Quaternion oldRotation = canvasPivot.rotation;
		canvasPivot.LookAt(camTransform);
		Vector3 euler = canvasPivot.eulerAngles;
		euler.z = 0f; euler.y = 0f; euler.x *= -1f;
		canvasPivot.eulerAngles = euler;
		newRotation = canvasPivot.rotation;

		canvasPivot.rotation = oldRotation;


		//scale with distance to camera

		float ratio = 1f - Common.GetRatio(camTransform.position.y, heightLimits.x, heightLimits.y); //1 when closest, 0 when farthest
		float sclFactor = Mathf.Lerp(min_scale, 1f, 1f-ratio);
		newScale = Vector3.one * sclFactor;

		//move locally with distance to camera
		newLocalPosition = new Vector3
			(
				0f,
				Mathf.Lerp(moveLimitsY.x, moveLimitsY.y, ratio),
				Mathf.Lerp(moveLimitsZ.x, moveLimitsZ.y, ratio)
			);


		//Lerp to target properties
		const float LERP = 0.2f;
		canvasPivot.rotation = Quaternion.Lerp(canvasPivot.rotation, newRotation, LERP);
		transform.localScale = Vector3.Lerp(transform.localScale, newScale, LERP);
		canvasPivot.localPosition = Vector3.Lerp(canvasPivot.localPosition, newLocalPosition, LERP);
	}

	void AdjustText() {
		Tower T = gc.selectedObject.GetComponent<Tower>();
		int upgradeCost = T.GetUpgradeCost();
		int sellValue = (int)(T.GetSellValue() * gc.sellReturnPercent);

		upgradeText.text = "<b>Upgrade</b>\n$"+upgradeCost.ToString();
		sellText.text = "<b>Sell</b>\n$"+sellValue.ToString();

		textUpdatedFlag = true;
	}


	#region Button Press
	public void Upgrade() {
		bool success = gc.UpgradeTower();
		if (success)
			AdjustText();
	}
	public void Sell() {
		bool success = gc.SellTower();
		if (success)
			StartAnimating(false);
	}
	#endregion

	#region Activation/deactivation animations
	const int animate_frames_in = 8;
	const int animate_frames_out = 5;
	private IEnumerator currentRoutine;
	public AnimationCurve animationCurve;

	public void StartAnimating(bool toActive) {
		if (isAnimating)
			StopCoroutine(currentRoutine);

		currentRoutine = (toActive)? AnimateIn() : AnimateOut();
		StartCoroutine(currentRoutine);
	}

	//Animate activation by moving out from 0 position and scaling up from 0
	IEnumerator AnimateIn() {

		isAnimating = true;
		Vector3 minPos = new Vector3(0f, moveLimitsY.x, moveLimitsZ.x);
		int i = 0;
		float r;
		while (i < animate_frames_in) {
			r = animationCurve.Evaluate((float)i/(float)(animate_frames_in-1));

			transform.localScale = Vector3.Lerp(Vector3.one*0.2f, Vector3.one*min_scale, r);
			canvasPivot.localPosition = Vector3.Lerp(minPos*0.5f, minPos, r);

			i++;
			yield return null;
		}

		isAnimating = false;
		textUpdatedFlag = false;
	}

	IEnumerator AnimateOut() {

		isAnimating = true;
		Vector3 startPos = canvasPivot.localPosition;
		Vector3 startScl = transform.localScale;
		int i = 0;
		float r;
		while (i < animate_frames_out) {
			r = animationCurve.Evaluate((float)i/(float)(animate_frames_out-1));

			transform.localScale = Vector3.Lerp(startScl, Vector3.one*0.2f, r);
			canvasPivot.localPosition = Vector3.Lerp(startPos, startPos*0.5f, r);

			i++;
			yield return null;
		}
			
		isAnimating = false;
		gameObject.SetActive(false);
	}
	#endregion
}
