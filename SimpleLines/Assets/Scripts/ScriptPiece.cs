using UnityEngine;
using System.Collections;

public class ScriptPiece {
	private GameObject[] mBlocks = new GameObject[4]; 
	private Vector2 mOffset = new Vector2(); //Primary anchor offset from center point of piece, aka from center of piece to center of anchor 
	private PieceType mType = PieceType.I;
	private PieceRotate mRotate = PieceRotate.None;
	private const int mLayer = 9;

	public PieceType pieceType { get { return mType; } }
	public Color color { get { return mBlocks[0].GetComponent<Renderer>().material.color; } }
	public GameObject[] blocks { get { return mBlocks; } }

	public ScriptPiece() {
		ResetBlocks();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Enums

	public enum PieceType { //I,J,L,O,S,T,Z
		I, //Line
		J, //Backward L
		L, //Forward L
		O, //Square
		S, //Backward Z
		T, //Stumpy T
		Z  //Forward Z
	}
	
	public enum PieceRotate {
		None,
		Rotate270,
		Rotate180,
		Rotate90
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Positioning

	private void ResetBlocks(Vector3 nPosition, Vector3 nGridSize, Vector3 nBlockSize, Color nColor) {
		mBlocks[0].transform.position = new Vector3(nPosition.x+mOffset.x, nPosition.y+mOffset.y, nPosition.z);
		Vector3 nAnchorPos = mBlocks[0].transform.position;

		for(int i = 0; i < 4; i++) {
			mBlocks[i].layer = mLayer;
			mBlocks[i].transform.localScale = nBlockSize;
			mBlocks[i].GetComponent<Renderer>().material.color = nColor;

			ScriptBlock nScriptBlock = mBlocks[i].GetComponent<ScriptBlock>();
			float nX = nAnchorPos.x + nGridSize.x * nScriptBlock.column; //column and row are relative to primary anchor
			float nY = nAnchorPos.y + nGridSize.y * nScriptBlock.row;
			mBlocks[i].transform.position = new Vector3(nX, nY, nAnchorPos.z);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//External

	public PieceRotate GetNextRotate() {
		if(mRotate == PieceRotate.Rotate90) 
			return PieceRotate.None;
		return mRotate + 1;
	}

	public void ResetBlocks() {
		for(int i = 0; i < 4; i++) 
			mBlocks[i] = null;
		mOffset.x = 0;  
		mOffset.y = 0;
		mType = PieceType.I;
		mRotate = PieceRotate.None;
	}

	public void SetSpark(bool nSpark) {
		if (nSpark) {
			for (int i = 0; i < 4; i++) 
				mBlocks [i].GetComponent<ScriptBlock>().SparkOn();
		} else {
			for (int i = 0; i < 4; i++) 
				mBlocks [i].GetComponent<ScriptBlock>().SparkOff();
		}
	}

	public float GetScreenHeight(Camera nCam) {
		GameObject nTop = null, nBot = null;
		foreach(GameObject nBlock in mBlocks) {
			if(nBlock == null) continue;
			if(nTop == null || nTop.transform.position.y < nBlock.transform.position.y) nTop = nBlock;
			if(nBot == null || nBot.transform.position.y > nBlock.transform.position.y) nBot = nBlock;
		}
		if(nTop == null || nBot == null) return 0;
		return ScriptCommon.ScreenDistanceBottomFromTop(nTop, nCam) - ScriptCommon.ScreenDistanceBottom(nBot, nCam);
	}

	public float GetScreenCenterY(Camera nCam) {
		GameObject nTop = null, nBot = null;
		foreach(GameObject nBlock in mBlocks) {
			if(nBlock == null) continue;
			if(nTop == null || nTop.transform.position.y < nBlock.transform.position.y) nTop = nBlock;
			if(nBot == null || nBot.transform.position.y > nBlock.transform.position.y) nBot = nBlock;
		}
		if(nTop == null || nBot == null) return 0;
		float nScreenBot = ScriptCommon.ScreenDistanceBottom(nBot, nCam);
		return nScreenBot + (ScriptCommon.ScreenDistanceBottomFromTop(nTop, nCam) - nScreenBot) / 2;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Setup

	public void SetBlocks(Vector3 nPosition, Vector3 nGridSize, Vector3 nBlockSize, GameObject[] nBlocks, PieceType nType, PieceRotate nRotate) {
		ResetBlocks(); 
		mType = nType;
		mRotate = nRotate;
		Color nColor;
		switch(nType) {
		case PieceType.I:
			nColor = SetI(nGridSize,nBlocks);
			break;
		case PieceType.J:
			nColor = SetJ(nGridSize,nBlocks);
			break;
		case PieceType.L:
			nColor = SetL(nGridSize,nBlocks);
			break;
		case PieceType.O:
			nColor = SetO(nGridSize,nBlocks);
			break;
		case PieceType.S:
			nColor = SetS(nGridSize,nBlocks);
			break;
		case PieceType.T:
			nColor = SetT(nGridSize,nBlocks);
			break;
		default: //case PieceType.Z:
			nColor = SetZ(nGridSize,nBlocks);
			break;
		}
		ResetBlocks(nPosition, nGridSize, nBlockSize, nColor);
	}

	private Color SetO(Vector3 nGridSize, GameObject[] nBlocks) {
		Color nColor = new Color32(0,255,0,255); //Green
		mBlocks = nBlocks.Clone() as GameObject[];

		//PieceRotate.None to PieceRotate.Rotate90 all same
		mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
		mBlocks[1].GetComponent<ScriptBlock>().setCell(0, 1);
		mBlocks[2].GetComponent<ScriptBlock>().setCell(1, 0);
		mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 1);

		mOffset.x = 0 - nGridSize.x / 2; 
		mOffset.y = 0 - nGridSize.y / 2;

		mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, -1);
		mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
		mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(1, -1);
		mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);

		mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 0);
		mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 1);
		mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
		mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 1);

		mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
		mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
		mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -1);
		mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
		return nColor;
	}

	private Color SetI(Vector3 nGridSize, GameObject[] nBlocks) {
		Color nColor = new Color32(255,125,0,255); //Orange
		mBlocks = nBlocks.Clone() as GameObject[];

		if(mRotate == PieceRotate.None || mRotate == PieceRotate.Rotate180) {
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(0, 2);

			mOffset.x = 0; 
			mOffset.y = 0 - nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 3);

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 1);

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -2);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -3);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
		} else { //PieceRotate.Rotate270 and PieceRotate.Rotate90
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(-1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(2, 0);

			mOffset.x = 0 - nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(2, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(3, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(-2, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-3, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
		}
		return nColor;
	}

	private Color SetS(Vector3 nGridSize, GameObject[] nBlocks) {
		Color nColor = new Color32(255,0,255,255); //Pink
		mBlocks = nBlocks.Clone() as GameObject[];

		if(mRotate == PieceRotate.None || mRotate == PieceRotate.Rotate180) {
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(-1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 1);

			mOffset.x = 0; 
			mOffset.y = 0 - nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(-1, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(2, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
		} else { //PieceRotate.Rotate270 and PieceRotate.Rotate90
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(1, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 0);

			mOffset.x = 0 - nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(1, -2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, -1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
		}
		return nColor;
	}
	
	private Color SetZ(Vector3 nGridSize, GameObject[] nBlocks) {
		Color nColor = new Color32(255,255,0,255); //Yellow
		mBlocks = nBlocks.Clone() as GameObject[];

		if(mRotate == PieceRotate.None || mRotate == PieceRotate.Rotate180) {
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(-1, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 0);

			mOffset.x = 0; 
			mOffset.y = 0 - nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(-1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, -1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(2, -1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
		} else { //PieceRotate.Rotate270 and PieceRotate.Rotate90
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 1);

			mOffset.x = 0 - nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 2);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
		}
		return nColor;
	}
	
	private Color SetJ(Vector3 nGridSize, GameObject[] nBlocks) {
		Color nColor = new Color32(255,0,0,255); //Red
		mBlocks = nBlocks.Clone() as GameObject[];

		switch(mRotate) {
		case PieceRotate.None:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(-1, -1);

			mOffset.x = 0 + nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(-1, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, -2);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(1, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(1, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		case PieceRotate.Rotate90:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(-1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(-1, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 0);

			mOffset.x = 0; 
			mOffset.y = 0 - nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(2, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(2, -1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		case PieceRotate.Rotate180:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 1);

			mOffset.x = 0 - nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 2);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		default: //case PieceRotate.Rotate270:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(-1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(1, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 0);

			mOffset.x = 0; 
			mOffset.y = 0 + nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(2, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(2, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(-2, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		}
		return nColor;
	}

	private Color SetL(Vector3 nGridSize, GameObject[] nBlocks) {
		Color nColor = new Color32(0,255,255,255); //Sky
		mBlocks = nBlocks.Clone() as GameObject[];

		switch(mRotate) {
		case PieceRotate.None:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, -1);

			mOffset.x = 0 - nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(1, -2);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		case PieceRotate.Rotate90:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(-1, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(-1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 0);

			mOffset.x = 0; 
			mOffset.y = 0 + nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(2, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(2, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		case PieceRotate.Rotate180:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(-1, 1);

			mOffset.x = 0 + nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(-1, 2);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(1, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		default: //case PieceRotate.Rotate270:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(-1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 1);

			mOffset.x = 0; 
			mOffset.y = 0 - nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(2, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(2, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(-2, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		}
		return nColor;
	}

	private Color SetT(Vector3 nGridSize, GameObject[] nBlocks) {
		Color nColor = new Color32(0,0,255,255); //Blue
		mBlocks = nBlocks.Clone() as GameObject[];

		switch(mRotate) {
		case PieceRotate.None:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(-1, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 1);

			mOffset.x = 0; 
			mOffset.y = 0 - nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(-1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(2, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		case PieceRotate.Rotate90:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(1, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 1);

			mOffset.x = 0 - nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(-1, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 2);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(0, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		case PieceRotate.Rotate180:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(-1, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 0);

			mOffset.x = 0; 
			mOffset.y = 0 - nGridSize.y / 2;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(-1, -1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, -1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(1, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(2, 0);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-2, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		default: //case PieceRotate.Rotate270:
			mBlocks[0].GetComponent<ScriptBlock>().setCell(0, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCell(0, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCell(0, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCell(1, 0);

			mOffset.x = 0 - nGridSize.x / 2; 
			mOffset.y = 0;

			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 0);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor2(0, 2);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor2(1, 1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -1);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor3(0, -2);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor3(0, 0);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor3(1, -1);
			
			mBlocks[0].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 0);
			mBlocks[1].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, -1);
			mBlocks[2].GetComponent<ScriptBlock>().setCellRelAnchor4(-1, 1);
			mBlocks[3].GetComponent<ScriptBlock>().setCellRelAnchor4(0, 0);
			break;
		}
		return nColor;
	}
	//.class
}

























