using UnityEngine;
using System.Collections;

public class ScriptWorld : MonoBehaviour {
	public Material material;
	public Material reflect;
	public Camera viewer;
	private Color mTargetColor, mCurrentColor;
	private Vector3 mTargetRotation, mOriginalPosition;
	private float mTargetZ = 0;
	private const float mTargetMaxZ = 10, mColorAlpha = 100.0f, mSkyColorFactor = 1.0f, mBoxColorFactor = 0.5f;
	private const float mPerSecRotate = 0.6f, mPerSecDiscolor = 30, mPerSecMove = 0.3f;

	void Start() {
		mTargetColor = ScriptCommon.ColorRandom(mColorAlpha);
		mCurrentColor = ScriptCommon.ColorRandom(mColorAlpha);
		mTargetRotation = ScriptCommon.RotationRandom();
		mOriginalPosition = new Vector3(viewer.transform.position.x, viewer.transform.position.y, viewer.transform.position.z);
		mTargetZ = ScriptCommon.RandomSignedFloat(mOriginalPosition.z, mTargetMaxZ);
		UpdateMaterialColor();
	}

	void Update() {
		if(UpdateMaterialColor())
			mTargetColor = ScriptCommon.ColorRandom(mColorAlpha);
		if(UpdateCameraRotation())
			mTargetRotation = ScriptCommon.RotationRandom();
		if(UpdateCameraPosition())
			mTargetZ = ScriptCommon.RandomSignedFloat(mOriginalPosition.z, mTargetMaxZ);
	}
	
	private bool UpdateCameraPosition() {
		bool nComplete = false;
		float nZ = ScriptCommon.NumberTarget(viewer.transform.position.z, mTargetZ, ScriptCommon.FrameValue(mPerSecMove), out nComplete);
		viewer.transform.position = new Vector3(mOriginalPosition.x, mOriginalPosition.y, nZ);
		return nComplete;
	}

	private bool UpdateCameraRotation() {
		bool nComplete = false;
		Vector3 nCurrent = viewer.transform.rotation.eulerAngles;
		Vector3 nRotation = ScriptCommon.RotationTarget(nCurrent, mTargetRotation, ScriptCommon.FrameValue(mPerSecRotate), out nComplete);
		viewer.transform.rotation = Quaternion.Euler(nRotation.x, nRotation.y, nRotation.z);
		return nComplete;
	}

	private bool UpdateMaterialColor() {
		bool nComplete = false;
		mCurrentColor = ScriptCommon.ColorTarget(mCurrentColor, mTargetColor, ScriptCommon.FrameValue(mPerSecDiscolor), out nComplete);
		reflect.color = new Color(mCurrentColor.r*mBoxColorFactor, mCurrentColor.g*mBoxColorFactor, mCurrentColor.b*mBoxColorFactor, reflect.color.a); 
		material.SetColor("_Tint", new Color(mCurrentColor.r*mSkyColorFactor, mCurrentColor.g*mSkyColorFactor, mCurrentColor.b*mSkyColorFactor, mCurrentColor.a));
		return nComplete;
	}
	//.class
}





