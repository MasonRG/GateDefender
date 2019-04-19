using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TowerAnimation {

	public static readonly int STATE_IDLE = 0;
	public static readonly int STATE_FIRE = 1;
	public static readonly int STATE_FOLD = 2;
	public static readonly int STATE_UNFOLD = 3;

	static readonly string param_state = "State";
	static readonly string tag_fold = "Fold";
	static readonly string tag_unfold = "Unfold";
	static readonly string tag_fire = "Fire";

	public static void ChangeState(Animator anim, int newState) {
		anim.SetInteger(param_state, newState);
	}

	public static bool IsFolding(Animator anim) {
		return anim.GetCurrentAnimatorStateInfo(0).IsTag(tag_fold) || anim.GetNextAnimatorStateInfo(0).IsTag(tag_fold);
	}
	public static bool IsUnfolding(Animator anim) {
		return anim.GetCurrentAnimatorStateInfo(0).IsTag(tag_unfold) || anim.GetNextAnimatorStateInfo(0).IsTag(tag_unfold);
	}
	public static bool IsFiring(Animator anim) {
		return anim.GetCurrentAnimatorStateInfo(0).IsTag(tag_fire) || anim.GetNextAnimatorStateInfo(0).IsTag(tag_fire);
	}
}
