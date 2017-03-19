using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animation : MonoBehaviour {
	public float speed = 1.0f;
	// 対象のAniamtor
	[SerializeField] Animator animator;
	// 再生するAnimationClip
	[SerializeField] AnimationClip clip;

	void Start ()
	{
		// PlayableAPIのノードを作ってAnimatorで再生
		var playableClip = UnityEngine.Experimental.Director.AnimationClipPlayable.Create(clip);
		animator.Play(playableClip);
	}

	// Update is called once per frame
	void Update () {
		TouchInfo info = AppUtil.GetTouch();
		switch(info){
		case TouchInfo.Began:
			break;
		case TouchInfo.Moved:
			//Vector3 delta = AppUtil.GetDeltaPosition();
			//float xMove = delta.x * speed *10;
			//float yMove = delta.y * speed *10;
			//
			//float xAngle = yMove;
			//float yAngle = -xMove;

			//ターゲットのGameObjectを回転させたりするつもりのコード
			//target.Rotate(xAngle, yAngle, 0.0f, Space.World);
			break;
		case TouchInfo.Ended:
			var playableClip = UnityEngine.Experimental.Director.AnimationClipPlayable.Create(clip);
			animator.Play(playableClip);
			break;
		}
	}
}
