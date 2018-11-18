using UnityEngine;
using System.Collections;

public class ScriptClicker : MonoBehaviour {
	public GameObject prefabSpark;
	public GameObject prefabExplode;

	private GameObject mSpark = null, mExplode = null;
	private bool mLit = false;
	private static Color mUnlitColor = new Color32(255, 255, 255, 205);

	public System.Action<ScriptGame.GameFlow,Object> receiver { get; set; }
	public int row { get; set; }
	public int column { get; set; }
	public GameObject occupant { get; set; }

	void Start() {
		SetUnlit();
	}

	void OnMouseDown() {
		receiver(ScriptGame.GameFlow.Enter, this);
	}

	void OnMouseEnter() {
		receiver(ScriptGame.GameFlow.Select, this);
	}

	void OnMouseUp() {
		receiver(ScriptGame.GameFlow.Drop, this);
	}

	public bool IsLit() {
		return mLit;
	}

	public void SetLit(Color nLitColor) {
		GetComponent<Renderer>().material.color = nLitColor;
		mLit = true;
	}

	public void SetUnlit() {
		GetComponent<Renderer>().material.color = mUnlitColor;
		mLit = false;
	}

	//Interface
	public void SparkOn() {
		if(mSpark == null) {
			mSpark = (GameObject)Instantiate(prefabSpark, gameObject.transform.position, gameObject.transform.rotation);
			mSpark.transform.parent = gameObject.transform;
			mSpark.layer = this.gameObject.layer;
		}
		var ps = mSpark.GetComponent<ParticleSystem>().main;
		ps.loop = true;
		mSpark.SetActive(true);
	}
	
	public void SparkOff(bool nImmediate = false) {
		if (mSpark == null) return;
		var ps = mSpark.GetComponent<ParticleSystem>().main;
		ps.loop = true;
		if (nImmediate) mSpark.SetActive(false);
	}

	public void ExplodeOn() {
		if(mExplode == null) {
			mExplode = (GameObject)Instantiate(prefabExplode, gameObject.transform.position, gameObject.transform.rotation);
			mExplode.transform.parent = gameObject.transform;
			mExplode.layer = this.gameObject.layer;
		}
		mExplode.GetComponent<ParticleSystem>().Play();
		mExplode.SetActive(true);
	}
	
	public void ExplodeOff() {
		if (mExplode == null) return;
		mExplode.GetComponent<ParticleSystem>().Stop();
		mExplode.SetActive(false);
	}
	//.class
}






























