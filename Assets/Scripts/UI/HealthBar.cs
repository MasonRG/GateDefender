using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

	private Enemy host;
	private Transform canvas;
	private Image bar;

	private int maxHealth, currentHealth;

	void Start () {

		foreach(Transform t in GetComponentsInChildren<Transform>()) {
			if (t.name.Equals("Canvas"))	canvas = t;
			else if (t.name.Equals("Bar"))	bar = t.GetComponent<Image>();
		}

		bar.fillAmount = 1f;
		bar.color = Color.green;

		host = GetComponentInParent<Enemy>();
		maxHealth = host.Health;
		currentHealth = maxHealth;
	}

	void Update() {

		//Only update the healthbar if the health changes
		if (currentHealth != host.Health) {
			currentHealth = host.Health;
			AdjustHealthBar();
		}

		//Override rotation
		OrientHealthBar();
	}

	void OrientHealthBar() {
		Vector3 euler = canvas.eulerAngles;
		euler.z = 0f; euler.y = 0f; euler.x = 30f;
		canvas.eulerAngles = euler;
	}

	void AdjustHealthBar() {

		//take current health and max health and figure out what percent remains
		float percent = (float)currentHealth / (float)maxHealth;

		//set the fill bar to this percentage
		bar.fillAmount = percent;


		// decide the colour based on percent using color lerping
		Color startColor, endColor;
		if (percent >= 0.5f) {
			startColor = Color.green;
			endColor = Color.yellow;
			percent = Common.GetRatio(percent, 0.5f, 1f);
		}else {
			startColor = Color.yellow;
			endColor = Color.red;
			percent = Common.GetRatio(percent, 0f, 0.5f);
		}

		bar.color = Color.Lerp(endColor, startColor, percent);
	}

}
