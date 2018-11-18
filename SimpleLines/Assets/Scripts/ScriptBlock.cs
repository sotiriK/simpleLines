using UnityEngine;
using System.Collections;

public class ScriptBlock : MonoBehaviour {
	public GameObject prefabSpark;
	
	private Vector3 mMoveTo;
	private GameObject mSpark = null;
	private bool mBust = false, mMove = false;
	private int mRotDirY = 1, mRotDirZ = 1, mMoveCount = 0;
	
	private const int mMoveCountToFall = 4, mMoveCountToExit = 64;
	private const float mPerSecMove = 10, mPerSecRotate = 360, mPerSecDrop = 20;
	private const int mLayerBust = 10;

	void Update() {
		if(mMove) {
			float nMove = ScriptCommon.FrameValue(mPerSecMove);

			if(transform.position.y >= mMoveTo.y) {
				transform.position = new Vector3(mMoveTo.x,transform.position.y-nMove,mMoveTo.z);
				if(transform.position.y <= mMoveTo.y) {
					transform.position = new Vector3(mMoveTo.x,mMoveTo.y,mMoveTo.z);
					mMove = false;
				}
			} else if(transform.position.y < mMoveTo.y) {
				transform.position = new Vector3(mMoveTo.x,transform.position.y+nMove,mMoveTo.z);
				if(transform.position.y >= mMoveTo.y) {
					transform.position = new Vector3(mMoveTo.x,mMoveTo.y,mMoveTo.z);
					mMove = false;
				}
			}
		} else if(mBust) {
			mMoveCount++;
			if(mMoveCount <= mMoveCountToFall) {
				float nDrop = ScriptCommon.FrameValue(mPerSecDrop);
				transform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z-nDrop);
			} else if(mMoveCount <= mMoveCountToExit) {
				float nDrop = ScriptCommon.FrameValue(mPerSecDrop);
				float nRotate = ScriptCommon.FrameValue(mPerSecRotate);
				transform.Rotate(new Vector3(-nRotate,nRotate*mRotDirY,nRotate*mRotDirZ));
				transform.position = new Vector3(transform.position.x,transform.position.y-nDrop,transform.position.z);
			} else {
				mBust = false;
				Destroy(gameObject);
			}
		}
	}

	//Interface
	public void Bust() {
		gameObject.layer = mLayerBust;
		mRotDirY = ScriptCommon.RandomSignOrZero();
		mRotDirZ = ScriptCommon.RandomSignOrZero();
		mBust = true;
	}

	public void MoveTo(Vector3 nVector) {
		mMoveTo = nVector;
		mMove = true;
	}

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

	//This cell is relative to primary anchor
	public int row { get; set; }
	public int column { get; set; }

	public void setCell(int nC, int nR) { 
		column = nC;
		row = nR;
	}

	//These cells are relative to anchor 2 through 4
	public int rowRelAnchor2 { get; set; }
	public int columnRelAnchor2 { get; set; }
	public int rowRelAnchor3 { get; set; }
	public int columnRelAnchor3 { get; set; }
	public int rowRelAnchor4 { get; set; }
	public int columnRelAnchor4 { get; set; }
	
	public void setCellRelAnchor2(int nC, int nR) {
		columnRelAnchor2 = nC;
		rowRelAnchor2 = nR;
	}

	public void setCellRelAnchor3(int nC, int nR) {
		columnRelAnchor3 = nC;
		rowRelAnchor3 = nR;
	}

	public void setCellRelAnchor4(int nC, int nR) {
		columnRelAnchor4 = nC;
		rowRelAnchor4 = nR;
	}

	//Optional anchor
	public int GetColumn(int nAnchorIndex) {
		int[] nAll = new int[]{column, columnRelAnchor2, columnRelAnchor3, columnRelAnchor4};
		return nAll[nAnchorIndex];
	}

	public int GetRow(int nAnchorIndex) {
		int[] nAll = new int[]{row, rowRelAnchor2, rowRelAnchor3, rowRelAnchor4};
		return nAll[nAnchorIndex];
	}
	//.class
}





