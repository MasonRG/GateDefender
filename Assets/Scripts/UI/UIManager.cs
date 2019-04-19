using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	private GameController gc;
	private GameObject infoContainer;
	private GameObject pauseContainer;
	private GameObject skipWaveWaitButton;
	private Text selectedTypeText, selectedTierText, selectedStrengthText;
	private Text moneyText, healthText, waveText, waveTimerText;
	private Text spd0Text, spd1Text, spd2Text, spd3Text;

	private int currentMoney;
	private int currentHealth;
	private int currentWave;
	private Text currentSpdText;

	void Start () {
		gc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		currentMoney = gc.playerStartMoney;
		currentHealth = gc.playerStartHealth;

		Initialize();

		moneyText.text = "$"+currentMoney.ToString();
		healthText.text = currentHealth.ToString()+" HP";
		UpdateSpeedTexts(currentSpdText);
		infoContainer.SetActive(false);
	}


	void Initialize() {
		foreach(Transform t in GetComponentsInChildren<Transform>(true)) {
			if (t.name.Equals("info_container"))			infoContainer = t.gameObject;
			else if (t.name.Equals("pause_container"))		pauseContainer = t.gameObject;
			else if (t.name.Equals("info_type_label"))		selectedTypeText = t.GetComponent<Text>();
			else if (t.name.Equals("info_tier_label"))		selectedTierText = t.GetComponent<Text>();
			else if (t.name.Equals("info_strength_label"))	selectedStrengthText = t.GetComponent<Text>();
			else if (t.name.Equals("money_label"))			moneyText = t.GetComponent<Text>();
			else if (t.name.Equals("health_label"))			healthText = t.GetComponent<Text>();
			else if (t.name.Equals("wave_count_label"))		waveText = t.GetComponent<Text>();
			else if (t.name.Equals("wave_timer_label"))		waveTimerText = t.GetComponent<Text>();
			else if (t.name.Equals("skip_wait_button"))		skipWaveWaitButton = t.gameObject;
			else if (t.name.Equals("spd0_label"))			spd0Text = t.GetComponent<Text>();
			else if (t.name.Equals("spd1_label"))			currentSpdText = spd1Text = t.GetComponent<Text>();
			else if (t.name.Equals("spd2_label"))			spd2Text = t.GetComponent<Text>();
			else if (t.name.Equals("spd3_label"))			spd3Text = t.GetComponent<Text>();

			//For assignments to build costs (only need to set once)
			else if (t.name.Equals("cost_gun"))	
				t.GetComponent<Text>().text = "$"+Tower.Costs.GUN_TIER_0.ToString();
			else if (t.name.Equals("cost_laser"))	
				t.GetComponent<Text>().text = "$"+Tower.Costs.LASER_TIER_0.ToString();
			else if (t.name.Equals("cost_rocket"))	
				t.GetComponent<Text>().text = "$"+Tower.Costs.ROCKET_TIER_0.ToString();
		}
	}


	void Update () {

		if (PlayerStats.gameOver)
			return;

		//Activate the info window when an object is selected
		if (gc.selectedObject != null && gc.selectedObject.tag.Equals("Tower")) {
			Tower T = gc.selectedObject.GetComponent<Tower>();
			selectedTypeText.text = GetTypeText(T);
			selectedTierText.text = GetTierText(T);
			selectedStrengthText.text = GetStrengthText(T);
			infoContainer.SetActive(true);
		}
		else {
			infoContainer.SetActive(false);
		}


		//Manage the speed icons and pause/gameover window

		//Make sure the correct speed icon is indicated
		Text liveSpdText = GetLiveSpeedText();
		if (currentSpdText != liveSpdText && liveSpdText != null) {
			currentSpdText = liveSpdText;
			UpdateSpeedTexts(currentSpdText);

			//Enable the pause ui if we are paused
			pauseContainer.SetActive(currentSpdText == spd0Text);
		}

		//Check for game end conditions (wave == 21 for victory ; health <= 0 for defeat)
		if (PlayerStats.wave == 20) {
			//Enemies still alive?
			if (GameObject.FindWithTag("Enemy") == null) {
				//Victory
				EndGame(true);
			}
		}
		if (PlayerStats.health <= 0) {
			//Defeat
			EndGame(false);
		}

	
		//Keep stats texts up to date
		if (currentMoney != PlayerStats.money) {
			currentMoney = PlayerStats.money;
			moneyText.text = "$"+currentMoney.ToString();
		}

		if (currentHealth != PlayerStats.health) {
			currentHealth = PlayerStats.health;
			healthText.text = currentHealth.ToString()+" HP";
		}

		if (currentWave != PlayerStats.wave) {
			currentWave = PlayerStats.wave;
			string s = (currentWave < 10)? "0"+currentWave.ToString() : currentWave.ToString();
			waveText.text = "Wave: "+s;
		}

		if (PlayerStats.waveWaitSeconds == 0) {
			waveTimerText.text = "Next wave: ";
			skipWaveWaitButton.SetActive(false);
		}else {
			waveTimerText.text = "Next wave: "+GetWaveTimerAsString();
			skipWaveWaitButton.SetActive(true);
		}


	}

	const string win_str = "You Win!";
	const string lose_str = "Game Over";
	void EndGame(bool isWin) {
		gc.ChangeGameSpeed(0);
		UpdateSpeedTexts(spd0Text);

		foreach(Transform t in pauseContainer.GetComponentsInChildren<Transform>(true)) {
			if (t.name.Equals("Resume"))					
				t.gameObject.SetActive(false);
			else if (t.name.Equals("pause_label"))
				t.GetComponent<Text>().text = (isWin)? win_str : lose_str;
		}
		pauseContainer.SetActive(true);
		PlayerStats.gameOver = true;
		GameObject.Find("ButtonAudio").GetComponent<ButtonClickAudio>().PlayGameOverSound(isWin);
	}


	const string spdColorPrefix = "<color=#00FF88FF>";
	const string spdColorSuffix = "</color>";
	const string spd0 = "\u275A \u275A";
	const string spd1 = "\u25B6";
	const string spd2 = spd1+spd1;
	const string spd3 = spd2+spd1;
	void UpdateSpeedTexts(Text newText) {
		spd0Text.text = spd0;
		spd1Text.text = spd1;
		spd2Text.text = spd2;
		spd3Text.text = spd3;
		newText.text = spdColorPrefix + newText.text + spdColorSuffix;
	}
	Text GetLiveSpeedText() {
		int scl = Mathf.RoundToInt(Time.timeScale);
		switch(scl) {
		case 0: return spd0Text;
		case 1: return spd1Text;
		case 2: return spd2Text;
		case 3: return spd3Text;
		default: return null;
		}
	}


	string GetWaveTimerAsString() {
		int i = PlayerStats.waveWaitSeconds;
		return (i < 10)? "0"+i.ToString() : i.ToString();
	}


	const string gun_name = "Machine Gun";
	const string laser_name = "Laser Cannon";
	const string rocket_name = "Rocket Launcher";
	string GetTypeText(Tower T) {
		switch(T.towerType) {
		case Tower.TYPE_GUN:	return gun_name;
		case Tower.TYPE_LASER:	return laser_name;
		case Tower.TYPE_ROCKET:	return rocket_name;
		default:				return string.Empty;
		}
	}

	const string tier0_name = "Level 1";
	const string tier1_name = "Level 2";
	const string tier2_name = "Level 3";
	string GetTierText(Tower T) {
		switch(T.upgradeTier) {
		case Tower.TIER_0:	return tier0_name;
		case Tower.TIER_1:	return tier1_name;
		case Tower.TIER_2:	return tier2_name;
		default:			return string.Empty;
		}
	}

	const string strength_prefix = "<b> Strength:</b>\t\t";
	const string weakness_prefix = "\n<b>Weakness:</b>\t\t";
	const string box_name = "Boxes";
	const string hex_name = "Hexagons";
	const string dia_name = "Diamonds";
	string GetStrengthText(Tower T) {
		string str, wkn;
		switch(T.towerType) {
		case Tower.TYPE_GUN:	str = dia_name; wkn = box_name; break;
		case Tower.TYPE_LASER:	str = box_name; wkn = hex_name; break;
		case Tower.TYPE_ROCKET:	str = hex_name; wkn = dia_name; break;
		default:				str = string.Empty; wkn = string.Empty; break;
		}
		return strength_prefix + str + weakness_prefix + wkn;
	}
}
