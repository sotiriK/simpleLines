using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScriptTimer : MonoBehaviour {
	public GameObject goGame;
	public GameObject goImageLeft;
	public GameObject goImageRight;
	public GameObject goImageMaskRight;
	public GameObject goImageContainer;
	public Text textLevel;

	private int mLevel = mStartLevel;
	private float mSeconds = mMaxSeconds;
	private float mRotationRight = 0, mRotationLeft = 0;
	private bool mRunning = false, mPaused = false;
	private Sprite mSwapped = null;

	private const float mMaxSeconds = 18, mMinSeconds = 6, mDropPerCycle = 1, mSwapRotation = 135, mMaxRotation = 180, mFullRotation = 360;
	private const int mStartLevel = 1, mMaxLevel = 99;
	
	public bool running { get { return mRunning; } }
	public float seconds { get { return mSeconds; } }
	public int level { get { return mLevel; } }

	public bool paused { 
		get { 
			return mPaused; 
		} set { 
			mPaused = value; 
			//if(mPaused) 
			//	GetComponent<AudioSource>().Stop(); //Danger off
			//else if(mRunning && !goImageRight.activeSelf) GetComponent<AudioSource>().Play(); //Danger on
		} 
	}

	void Update() {
		if(!mRunning || mPaused) 
			return;
		float nFrameRotation = ScriptCommon.FrameValue(mFullRotation / mSeconds);
		RectTransform nTransLeft = goImageLeft.GetComponent<RectTransform>();

		if(goImageRight.activeSelf) {
			RectTransform nTransRight = goImageRight.GetComponent<RectTransform>();
			mRotationRight -= nFrameRotation;

			if(mRotationRight > -mMaxRotation) {
				if(mSwapped == null && mRotationRight < -mSwapRotation) {
					mSwapped = goImageMaskRight.GetComponent<Image>().sprite;
					goImageMaskRight.GetComponent<Image>().sprite = goImageContainer.GetComponent<Image>().sprite;
				}

				nTransRight.rotation = Quaternion.Euler(0,0,mRotationRight);
			} else {
				//GetComponent<AudioSource>().Play(); //Danger on
				goImageRight.SetActive(false);

				float nOver = -mMaxRotation - mRotationRight; 
				if(nOver > 0) {
					mRotationLeft -= nOver;
					nTransLeft.rotation = Quaternion.Euler(0,0,mRotationLeft);
				}
			}
		} else {
			mRotationLeft -= nFrameRotation;

			if(mRotationLeft > -mMaxRotation) {
				nTransLeft.rotation = Quaternion.Euler(0,0,mRotationLeft);
			} else {
				//GetComponent<AudioSource>().Stop(); //Danger off
				goImageLeft.SetActive(false);
				mRunning = false;
			}
		}
	}

	public void runTimer() {
		//GetComponent<AudioSource>().Stop(); //Danger off
		mPaused = false;
		mRotationRight = 0;
		mRotationLeft = 0;
		goImageLeft.GetComponent<RectTransform>().rotation = Quaternion.identity;
		goImageRight.GetComponent<RectTransform>().rotation = Quaternion.identity;
		goImageRight.SetActive(true);
		goImageLeft.SetActive(true);
		mRunning = true;
		if(mSwapped == null) 
			return;
		goImageMaskRight.GetComponent<Image> ().sprite = mSwapped;
		mSwapped = null;
	}

	public void StopTimer() {
		//GetComponent<AudioSource>().Stop(); //Danger off
		goImageRight.SetActive(false);
		goImageLeft.SetActive(false);
		mRunning = false;
	}

	public void LevelIncrease() {
		mLevel++;
		if(mLevel > mMaxLevel)
			mLevel = mMaxLevel;
		textLevel.text = mLevel.ToString();

		mSeconds -= mDropPerCycle;
		if(mSeconds < mMinSeconds) mSeconds = mMinSeconds;
	}
	//.class
}











