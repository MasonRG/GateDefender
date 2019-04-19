using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

public class MainMenuController : MonoBehaviour {

	public int LevelSelectCurr = 0;
	public GameObject Lv_E_img, Lv_M_img, Lv_H_img;

	public void LevelSelectRight(){
		LevelSelectCurr = (LevelSelectCurr + 1) % 3;
		if (LevelSelectCurr == 0) {
			Lv_E_img.SetActive (true);
			Lv_M_img.SetActive (false);
			Lv_H_img.SetActive (false);
		}
		if (LevelSelectCurr == 1) {
			Lv_E_img.SetActive (false);
			Lv_M_img.SetActive (true);
			Lv_H_img.SetActive (false);
		}
		if (LevelSelectCurr == 2) {
			Lv_E_img.SetActive (false);
			Lv_M_img.SetActive (false);
			Lv_H_img.SetActive (true);
		}

	}
	public void LevelSelectLeft(){
		LevelSelectCurr = (LevelSelectCurr - 1) % 3;
		if (LevelSelectCurr == 0) {
			Lv_E_img.SetActive (true);
			Lv_M_img.SetActive (false);
			Lv_H_img.SetActive (false);
		}
		if (LevelSelectCurr == 1) {
			Lv_E_img.SetActive (false);
			Lv_M_img.SetActive (true);
			Lv_H_img.SetActive (false);
		}
		if (LevelSelectCurr == 2) {
			Lv_E_img.SetActive (false);
			Lv_M_img.SetActive (false);
			Lv_H_img.SetActive (true);
		}
	}
	//Bad name I know. I'm sorry, I have a cold.
	public void LevelSelectSelect(){
		string[] levels = {"level_easy", "level_medium", "level_hard"}; 
		EditorSceneManager.LoadScene (levels [LevelSelectCurr]);
	}

	public void ButtonHome(){
		LevelSelectCurr = 0;
	}

	public void QuitGame(){
		UnityEditor.EditorApplication.isPlaying = false;
	}
}
