using UnityEngine;
using System.Collections;

public class ScriptScreenPanel : MonoBehaviour {
	public Camera cameraAlternate;
	public GameObject game;

	public void PushBackAlternate() {
		//Simply moves camera showing current piece back so that it is behind interface
		cameraAlternate.depth = 1;
	}

	public void PullFrontAlternate() {
		//Simply moves camera showing current piece forward so that it is in front of interface
		cameraAlternate.depth = 3;
	}

	public void EndPoint() {
		game.GetComponent<ScriptGame>().GameActionAnime(ScriptGame.GameFlow.LastFrame, gameObject);
	}

	public void StartPoint() {
		game.GetComponent<ScriptGame>().GameActionAnime(ScriptGame.GameFlow.FirstFrame, gameObject);
	}
	//.class
}
