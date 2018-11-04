using UnityEngine;
using System;
using System.Collections;

public static class ScriptCommon {
	//Frame
	public static float FrameValue(float nPerSecValue) {
		return nPerSecValue * Time.deltaTime;
	}

	public static int FrameValue(int nPerSecValue) {
		return Mathf.RoundToInt((float)nPerSecValue * Time.deltaTime);
	}

	//Rotation
	public static Vector3 RotationRandom() {
		return new Vector3(RandomAngle(), RandomAngle(), RandomAngle());
	}

	public static Vector3 RotationTarget(Vector3 nCurrent, Vector3 nTarget, float nChange, out bool nComplete) {
		bool c1 = false, c2 = false, c3 = false;
		float x = NumberTarget(nCurrent.x, nTarget.x, nChange, out c1);
		float y = NumberTarget(nCurrent.y, nTarget.y, nChange, out c2);
		float z = NumberTarget(nCurrent.z, nTarget.z, nChange, out c3);
		nComplete = c1 && c2 && c3;
		return new Vector3(x, y, z);
	}

	//Color
	public static Color ColorRandom() {
		byte r = RandomByte(), g = RandomByte(), b = RandomByte(), a = RandomByte();
		return (Color)(new Color32(r, g, b, a)); //Color is 0.0-1.0, Color32 is 0-255 even for alpha
	}

	public static Color ColorRandom(float nAlpha) {
		if(nAlpha > 100 || nAlpha < 0) nAlpha = 100;
		byte r = RandomByte(), g = RandomByte(), b = RandomByte(), a = (byte)Mathf.FloorToInt(nAlpha/100.0f*255.0f);
		return (Color)(new Color32(r, g, b, a)); //Color is 0.0-1.0, Color32 is 0-255 even for alpha, nAlpha is 0.0-100.0
	}

	public static Color ColorTarget(Color nCurrent, Color nTarget, float nChange, out bool nComplete) {
		nChange = nChange / 255.0f; //Change factor
		if(nChange > 1 || nChange < 0) nChange = 1;
		bool c1 = false, c2 = false, c3 = false, c4 = false;
		float r = NumberTarget(nCurrent.r, nTarget.r, nChange, out c1);
		float g = NumberTarget(nCurrent.g, nTarget.g, nChange, out c2);
		float b = NumberTarget(nCurrent.b, nTarget.b, nChange, out c3);
		float a = NumberTarget(nCurrent.a, nTarget.a, nChange, out c4);
		nComplete = c1 && c2 && c3 && c4;
		return new Color(r, g, b, a);
	}

	//Number
	public static float NumberTarget(float nCurrent, float nTarget, float nChange, out bool nComplete) {
		nComplete = false;
		if(nCurrent >= nTarget) { nCurrent -= nChange; if(nCurrent <= nTarget) { nCurrent = nTarget; nComplete = true; } }
		else if(nCurrent < nTarget) { nCurrent += nChange; if(nCurrent >= nTarget) { nCurrent = nTarget; nComplete = true; } }
		return nCurrent;
	}

	public static bool NumberLazyEquals(float nNumber1, float nNumber2) {
		return Mathf.FloorToInt(nNumber1) == Mathf.FloorToInt(nNumber2) || Mathf.RoundToInt(nNumber1) == Mathf.RoundToInt(nNumber2);
	}

	//Random
	public static int RandomSign() {
		return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
	}
	
	public static int RandomSignOrZero() {
		int i = UnityEngine.Random.Range(0, 3);
		if(i > 1) i = -1;
		return i;
	}

	public static bool RandomBoolean() {
		return UnityEngine.Random.Range(0, 2) == 0;
	}

	public static byte RandomByte() {
		return (byte)UnityEngine.Random.Range(0, 256);
	}

	public static float RandomAngle() {
		return UnityEngine.Random.Range(0.0f, 360.0f);
	}

	public static float RandomFactor() {
		return UnityEngine.Random.Range(0.0f, 1.0f);
	}

	public static int RandomNumber(int nMin, int nMax) {
		return UnityEngine.Random.Range(nMin, nMax);
	}

	public static int RandomEnum(Type nEnum) {
		return UnityEngine.Random.Range(0, Enum.GetNames(nEnum).Length);
	}

	public static float RandomSignedFloat(float nMin, float nMax) {
		if(RandomBoolean()) 
			return -UnityEngine.Random.Range(nMin, nMax);
		return UnityEngine.Random.Range(nMin, nMax);
	}

	public static int RandomSignedInteger(int nMin, int nMax) {
		if(RandomBoolean()) 
			return -UnityEngine.Random.Range(nMin, nMax);
		return UnityEngine.Random.Range(nMin, nMax);
	}

	//Screen	
	public static float ScreenDistanceLeft(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bounds of game object
		Vector3 nLeft = nBounds.center - nBounds.extents; //Left of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nLeft); //In pixels
		return nScreenPos.x; //Space from left of screen to left of game object, bottom left of screen is 0,0
	}

	public static float ScreenDistanceRight(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bounds of game object
		Vector3 nRight = nBounds.center + nBounds.extents; //Right of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nRight); //In pixels
		return Screen.width - nScreenPos.x; //Space from right of screen to right of game object, bottom left of screen is 0,0
	}

	public static float ScreenDistanceBottom(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bound of game object
		Vector3 nBot = nBounds.center - nBounds.extents; //Bottom of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nBot); //In pixels
		return nScreenPos.y; //Space from bottom of screen to bottom of game object, bottom left of screen is 0,0
	}

	public static float ScreenDistanceTop(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bounds of game object
		Vector3 nTop = nBounds.center + nBounds.extents; //Top of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nTop); //In pixels
		return Screen.height - nScreenPos.y; //Space from top of screen to top of game object, bottom left of screen is 0,0
	}
	

	public static float ScreenDistanceLeftFromRight(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bounds of game object
		Vector3 nRight = nBounds.center + nBounds.extents; //Right of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nRight); //In pixels
		return nScreenPos.x; //Space from left of screen to right of game object, bottom left of screen is 0,0
	}
	
	public static float ScreenDistanceRightFromLeft(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bounds of game object
		Vector3 nLeft = nBounds.center - nBounds.extents; //Left of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nLeft); //In pixels
		return Screen.width - nScreenPos.x; //Space from right of screen to left of game object, bottom left of screen is 0,0
	}

	public static float ScreenDistanceBottomFromTop(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bound of game object
		Vector3 nTop = nBounds.center + nBounds.extents; //Top of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nTop); //In pixels
		return nScreenPos.y; //Space from bottom of screen to top of game object, bottom left of screen is 0,0
	}

	public static float ScreenDistanceTopFromBottom(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		Bounds nBounds = nObject.GetComponent<Renderer>().bounds; //Bounds of game object
		Vector3 nBot = nBounds.center - nBounds.extents; //Bottom of game object
		Vector3 nScreenPos = nCam.WorldToScreenPoint(nBot); //In pixels
		return Screen.height - nScreenPos.y; //Space from top of screen to bottom of game object, bottom left of screen is 0,0
	}
	
	public static float ScreenObjectWidth(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		return Screen.width - ScreenDistanceLeft(nObject,nCam) - ScreenDistanceRight(nObject, nCam);
	}

	public static float ScreenObjectHeight(GameObject nObject, Camera nCam = null) {
		if(nCam == null) nCam = Camera.main;
		return Screen.height - ScreenDistanceTop(nObject, nCam) - ScreenDistanceBottom(nObject,nCam);
	}

	public static bool ScreenBoxHit(GameObject nBox, float nX, float nY) {
		float bX = ScreenDistanceLeft(nBox);
		if(nX < bX) return false;

		float bY = ScreenDistanceBottom(nBox);
		if(nY < bY) return false;

		float bW = ScreenObjectWidth(nBox);
		if(nX > bX + bW) return false;

		float bH = ScreenObjectHeight(nBox);
		if(nY > bY + bH) return false; 

		return true;
	}
	//.class
}























