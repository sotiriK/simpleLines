using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ScriptGame : MonoBehaviour {
	public AudioClip audioCrank,audioLine,audioOver,audioPlace,audioTick,audioExplode;
	public Camera cameraAlternate;
	public GameObject goAlternate;
	public GameObject goPlayArea;
	public GameObject prefabClicker;
	public GameObject prefabBlock;

	public Canvas canvas2d;
	public GameObject panelTop;
	public GameObject panelBottom;
	public Text textLines, textScore;
	public GameObject panelTimer;
	public Text textTotalFinal, textSinglesFinal, textDoublesFinal, textTriplesFinal, textQuadsFinal, textScoreFinal; 
	public Text textTotalPause, textSinglesPause, textDoublesPause, textTriplesPause, textQuadsPause, textScorePause; 
	public Animator animeGameOver, animePause, animeIntro;
	
	private GameObject[,] mClickers = new GameObject[mGridSpaces,mGridSpaces]; //BL is c0,r0
	private ScriptPiece mCurrentPiece = null;
	private ScriptClicker mLastDropPos = null;
	private GameState mGameState = GameState.Begin;
	private List<int> mLines = new List<int>();
	private Queue<KeyValuePair<GameFlow,Object>> mActions = new Queue<KeyValuePair<GameFlow,Object>>();

	private bool mActivePlayArea = false, mHasBomb = false;
	private int mTotal = 0, mSingles = 0, mDoubles = 0, mTriples = 0, mQuads = 0, mScore = 0;
	
	private static Color mDeadColor = new Color32(105,105,105,255); 
	private const int mGridSpaces = 8, mMaxCrankHoles = 3, mLinesPerLevel = 10, mBaseScore = 8; //nScore = mBaseScore(*mBaseScore per additional line);
	private const float mGridZ = 0, mGridOffY = 0.06f, mZoomStep = 0.05f, mPanStep = 0.025f;
	private const float mMainSizeFactor = 0.95f, mAlternateSizeFactor = 0.65f;
	private const float mDelayLineSeconds = 0.15f, mDelayOverSeconds = 0.2f, mDelayFinishSeconds = 1.5f;

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Enums

	public enum GameState {
		Begin, //Startup of scene
		Wait,  //Waiting for player action
		Drop,  //Player used a piece
		Explode, //Player used a bomb
		Line,  //Drop resulted in a line
		Crank, //Board moving up
		Pause, //Game paused
		Over   //Game over
	}
	
	public enum GameFlow {
		Enter, //Play area entered
		Select, //On play area
		Drop, //At selected location
		Deselect, //Kill selection
		Rotate, //Current piece
		Pause, //Pause or unpause game
		LastFrame, //Animation last frame
		FirstFrame //Animation first frame
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Flow

	void Start() {
		CreateClickers();
		ZoomToPlayfield();

		CreateTallestPiece();
		ZoomAlternate();

		DestroyPiece();
		CreateRandomPiece();
		animeIntro.GetComponent<ScriptScreenPanel>().PushBackAlternate(); //Send current piece camera behind interface, animeIntro doesn't do this, all other animes do it
	}

    void OnApplicationPause(bool pauseStatus) {
    	if (pauseStatus) 
			PlayerActionPause();
    }

	private void Update() {
		switch(mGameState) {
		case GameState.Begin: //Startup of scene
			StateBegin();
			break;
		case GameState.Wait: //Waiting for player action
			StateWait();
			break;
		case GameState.Drop: //Player used a piece
			StateDrop();
			break;
		case GameState.Explode: //Player used a bomb
			StateExplode();
			break;
		case GameState.Line: //Drop resulted in a line
			StateLine();
			break;
		case GameState.Crank: //Board moving up
			StateCrank();
			break;
		case GameState.Pause: //Game paused
			StatePause();
			break;
		case GameState.Over: //Game over
			StateOver();
			break;
		}
	}

	private void ChangeState(GameState nState, bool nRetainSel = false) {
		mGameState = nState;
		if(!nRetainSel) { 
			mActivePlayArea = false; 
			HideDropPosition(); 
		}
		mActions.Clear();
	}

	private bool IsState(GameState nState) {
		return mGameState == nState;
	}

	private void SetLabels() {
		textLines.text = mTotal.ToString();
		textScore.text = mScore.ToString();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//State

	private void StateBegin() { //From paused state or initial startup
		if(mActions.Count == 0) return; 

		var nPair = mActions.Dequeue();
		GameFlow nAction = nPair.Key;
		Object nAnimator = nPair.Value;

		if(nAction == GameFlow.LastFrame && nAnimator == animeIntro.gameObject) {
			animeIntro.GetComponent<ScriptScreenPanel>().PullFrontAlternate(); //Bring current piece camera in front of interface, animeIntro doesn't do this, all other animes do it
			panelTimer.GetComponent<ScriptTimer>().runTimer();
			ChangeState (GameState.Wait);
		} else if(nAction == GameFlow.FirstFrame && nAnimator == animePause.gameObject){
			panelTimer.GetComponent<ScriptTimer>().paused = false;
			ChangeState (GameState.Wait);
		}
	}

	private void StateWait() { //Main game loop
		if(!panelTimer.GetComponent<ScriptTimer>().running) { ChangeState(GameState.Crank, true); return; }
		if(mActions.Count == 0) return; 
		
		var nPair = mActions.Dequeue();
		GameFlow nAction = nPair.Key;
		ScriptClicker nPosition = (ScriptClicker)nPair.Value;

		switch(nAction) {
		case GameFlow.Enter:
			mActivePlayArea = true;
			ShowDropPosition(nPosition);
			break;
		case GameFlow.Select:
			if(mActivePlayArea)
				ShowDropPosition(nPosition);
			break;
		case GameFlow.Drop:
			if(mHasBomb) {
				if(ExplodeCurrentPiece()) 
					ChangeState(GameState.Explode);
			} else if(DropCurrentPiece()) {
				ChangeState(GameState.Drop);
			}
			break;
		case GameFlow.Deselect:
			HideDropPosition();
			mActivePlayArea = false;
			break;
		case GameFlow.Rotate:
			ScriptPiece.PieceType nType = mCurrentPiece.pieceType;
			ScriptPiece.PieceRotate nRotate = mCurrentPiece.GetNextRotate();
			DestroyPiece();
			CreatePiece(nType, nRotate);
			HideDropPosition();
			CheckNeedBomb();
			break;
		case GameFlow.Pause:
			ChangeState(GameState.Pause);
			
			textTotalPause.text = mTotal.ToString();
			textSinglesPause.text = mSingles.ToString();
			textDoublesPause.text = mDoubles.ToString();
			textTriplesPause.text = mTriples.ToString();
			textQuadsPause.text = mQuads.ToString();
			textScorePause.text = mScore.ToString();

			panelTimer.GetComponent<ScriptTimer>().paused = true;
			animePause.enabled = true;
			animePause.SetBool("enter", true);
			break;
		}
	}

	private void StateDrop() { //Piece dropped, play sound, go to process lines or return to main game loop
		GetComponent<AudioSource>().PlayOneShot(audioPlace);
		DestroyPiece();
		CreateRandomPiece();

		if (CheckForLines()) {
			ChangeState(GameState.Line);
		} else { CheckNeedBomb(); ChangeState(GameState.Wait); }
	}

	private void StateExplode() { //Piece exploded, play sound, return to main game loop
		GetComponent<AudioSource>().PlayOneShot(audioExplode); 	
		DestroyPiece();
		CreateRandomPiece();
		CheckNeedBomb(); 
		ChangeState(GameState.Wait);
	}

	private void StateLine() { //Remove lines, return to main game loop
		RemoveLines();
		CheckNeedBomb(); 
		ChangeState(GameState.Wait);
		panelTimer.GetComponent<ScriptTimer>().runTimer();
	}

	private void StateCrank() { //Add row, return to main game loop
		GetComponent<AudioSource>().PlayOneShot(audioCrank);

		int nOver = 0;
		for(int r = mGridSpaces-1; r > -1; r--) {
			for(int c = 0; c < mGridSpaces; c++) {
				ScriptClicker nCell = mClickers[c,r].GetComponent<ScriptClicker>();
				if(nCell.occupant != null) {
					if(r+1 < mGridSpaces)  {
						ScriptClicker nNewCell = mClickers[c,r+1].GetComponent<ScriptClicker>();
						nNewCell.occupant = nCell.occupant;
						//Move block up on play area, added line
						//nCell.occupant.transform.position = nNewCell.transform.position;
						nCell.occupant.GetComponent<ScriptBlock>().MoveTo(nNewCell.transform.position);
						nCell.occupant = null;
					} else {
						if(nCell.occupant != null) {
							//Move block off play area, exit
							nCell.occupant.GetComponent<ScriptBlock>().MoveTo(new Vector3(nCell.transform.position.x,nCell.transform.position.y+nCell.transform.localScale.y,nCell.transform.position.z));
							//Tell block to detach, gameover
							//Destroy(nCell.occupant);
							nCell.occupant.GetComponent<ScriptBlock>().Bust();
							nCell.occupant = null;
							nOver++;
						}
					}
				}
			}
		}

		int nHoles = 0;
		if(ScriptCommon.RandomBoolean()) { //Forward through columns
			for(int c = 0; c < mGridSpaces; c++) {
				if(nHoles < mMaxCrankHoles && ScriptCommon.RandomBoolean()) { nHoles++; continue; }
				if(nHoles == 0 && c == mGridSpaces-1) { nHoles++; break; } //Last space must be hole if no others were
				InsertBlock(mClickers[c,0]);
			}
		} else { //Backward through columns
			for(int c = mGridSpaces-1; c > -1; c--) {
				if(nHoles < mMaxCrankHoles && ScriptCommon.RandomBoolean()) { nHoles++; continue; }
				if(nHoles == 0 && c == 0) { nHoles++; break; } //Last space must be hole if no others were
				InsertBlock(mClickers[c,0]);
			}
		}

		if(nOver > 0) { 
			ChangeState(GameState.Over);

			textTotalFinal.text = mTotal.ToString();
			textSinglesFinal.text = mSingles.ToString();
			textDoublesFinal.text = mDoubles.ToString();
			textTriplesFinal.text = mTriples.ToString();
			textQuadsFinal.text = mQuads.ToString();
			textScoreFinal.text = mScore.ToString();

			Invoke("GameActionDelayedOver", mDelayOverSeconds); //Audio play
			Invoke("GameActionDelayedFinish", mDelayFinishSeconds); //Game over screen enter
		} else {
			CheckNeedBomb();
			if(mLastDropPos != null) 
				ShowDropPosition(mLastDropPos);
			panelTimer.GetComponent<ScriptTimer>().runTimer();
			ChangeState(GameState.Wait, true);
		}
	}

	private void StatePause() { //Paused, wait for player action resume (pause) or quit
		if(mActions.Count == 0) return; 
		
		var nPair = mActions.Dequeue();
		GameFlow nAction = nPair.Key;
		if(nAction == GameFlow.Pause) {
			ChangeState(GameState.Begin); //Will resume on pause panel exit

			animePause.enabled = true;
			animePause.SetBool("enter", false);
		} else if(nAction == GameFlow.Enter) {
			ChangeState(GameState.Over); //Will show intro panel and then reload game

			animeIntro.enabled = true;
			animeIntro.SetBool("exit", false);
		}
	}

	private void StateOver() { //Game over or quit (from pause state), if game over wait for player action play (enter), otherwise already in process
		if(mActions.Count == 0) return; 
		
		var nPair = mActions.Dequeue();
		GameFlow nAction = nPair.Key;
		
		if(nAction == GameFlow.Enter) {
			animeIntro.enabled = true;
			animeIntro.SetBool("exit", false);
		} else if(nAction == GameFlow.FirstFrame) {
			Object nAnimator = nPair.Value;
			if(nAnimator == animeIntro.gameObject) 
				SceneManager.LoadScene("SceneGame");
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Interface

	public void PlayerActionClicker(GameFlow nAction, Object nScriptClicker) {
		if(!IsState(GameState.Wait)) {
			if(nAction == GameFlow.Drop && mLastDropPos != null)
				HideDropPosition();
			return;
		}
		if(nAction == GameFlow.Drop && !ScriptCommon.ScreenBoxHit(goPlayArea, Input.mousePosition.x, Input.mousePosition.y))
			nAction = GameFlow.Deselect;
		mActions.Enqueue(new KeyValuePair<GameFlow,Object>(nAction, nScriptClicker));
	}

	public void PlayerActionRotate() {
		if(!IsState(GameState.Wait))
			return;
		GetComponent<AudioSource>().PlayOneShot(audioTick);
		mActions.Enqueue(new KeyValuePair<GameFlow,Object>(GameFlow.Rotate, null));
	}

	public void PlayerActionPause() {
		if(!IsState(GameState.Wait))
			return;
		GetComponent<AudioSource>().PlayOneShot(audioTick);
		mActions.Enqueue(new KeyValuePair<GameFlow,Object>(GameFlow.Pause, null));
	}

	public void PlayerActionResume() {
		if(mGameState != GameState.Pause) 
			return;
		GetComponent<AudioSource>().PlayOneShot(audioTick);
		mActions.Enqueue(new KeyValuePair<GameFlow,Object>(GameFlow.Pause, null));
	}

	public void PlayerActionQuit() {
		if(mGameState != GameState.Pause) 
			return;
		GetComponent<AudioSource>().PlayOneShot(audioTick);
		mActions.Enqueue(new KeyValuePair<GameFlow,Object>(GameFlow.Enter, null));
	}

	public void PlayerActionReplay() {
		if(mGameState != GameState.Over) 
			return;
		GetComponent<AudioSource>().PlayOneShot(audioTick);
		mActions.Enqueue(new KeyValuePair<GameFlow,Object>(GameFlow.Enter, null));
	}

	public void GameActionAnime(GameFlow nAction, Object nAnimator) {
		mActions.Enqueue(new KeyValuePair<GameFlow,Object>(nAction, nAnimator));
	}

	private void GameActionDelayedFinish() {
		mCurrentPiece.SetSpark(false);
		animeGameOver.enabled = true;
		animeGameOver.SetBool("enter", true);
	}

	private void GameActionDelayedLine() {
		GetComponent<AudioSource>().PlayOneShot(audioLine);
	}

	private void GameActionDelayedOver() {
		GetComponent<AudioSource>().PlayOneShot(audioOver);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Process

	private void CheckNeedBomb() {
		mHasBomb = false; 
		ScriptClicker[] nScriptClickers = new ScriptClicker[4];
		for(int c = 0; c < mGridSpaces; c++)  {
			for(int r = 0; r < mGridSpaces; r++) {
				for(int i = 0; i < 4; i++)  
					if(GetScriptClickers(i, c, r, 0, mGridSpaces, nScriptClickers) >= 4) return;
			}
		}
		//mCurrentPiece is a bomb, turn spark on
		mHasBomb = true;
		mCurrentPiece.SetSpark(true);
	}

	private void MoveBlocksDown(int nStartRow) {
		for(int r = nStartRow; r < mGridSpaces; r++) {
			for(int c = 0; c < mGridSpaces; c++) {
				ScriptClicker nCell = mClickers[c,r].GetComponent<ScriptClicker>();
				if(nCell.occupant != null) {
					ScriptClicker nNewCell = mClickers[c,r-1].GetComponent<ScriptClicker>();
					nNewCell.occupant = nCell.occupant;
					//Move blocks down on play area, got line
					//nCell.occupant.transform.position = nNewCell.transform.position;
					nCell.occupant.GetComponent<ScriptBlock>().MoveTo(nNewCell.transform.position);
					nCell.occupant = null;
				}
			}
		}
	}

	private void RemoveLines() {
		if(mLines.Count == 0) return;
		foreach(int r in mLines) {
			for(int c = 0; c < mGridSpaces; c++) {
				ScriptClicker nCell = mClickers[c,r].GetComponent<ScriptClicker>();
				if(nCell.occupant != null) {
					//Tell block to detach, got line
					//Destroy(nCell.occupant);
					nCell.occupant.GetComponent<ScriptBlock>().Bust();
					nCell.occupant = null;
				}
			}
		}
		int nLinesCount = mLines.Count;
		if(nLinesCount == 1) mSingles++;
		else if(nLinesCount == 2) mDoubles++;
		else if(nLinesCount == 3) mTriples++;
		else mQuads++;
		mTotal += nLinesCount;
		if(mTotal >= panelTimer.GetComponent<ScriptTimer>().level*mLinesPerLevel)
			panelTimer.GetComponent<ScriptTimer>().LevelIncrease(); 
		Invoke("GameActionDelayedLine", mDelayLineSeconds); //Audio play

		int nScore = 1;
		for(int i = 0; i < nLinesCount; i++) {
			//Minus i because each lines also moves down a row as another underneath is removed
			nScore *= mBaseScore;
			MoveBlocksDown(mLines[i]+1-i); 
		}
		mScore += nScore;
		SetLabels();
		mLines.Clear();
	}
	
	private bool CheckForLines() {
		//Rows are placed in mLines in order from bottom to top
		for(int r = 0; r < mGridSpaces; r++) {
			int c = 0;
			while(c < mGridSpaces) {
				if(mClickers[c,r].GetComponent<ScriptClicker>().occupant == null) 
					break;
				c++;
			}
			if(c >= mGridSpaces) mLines.Add(r);
		}
		return mLines.Count > 0;
	}

	private void InsertBlock(GameObject nClicker) {
		ScriptClicker nPosition = nClicker.GetComponent<ScriptClicker>();
		if(nPosition.occupant != null) return;

		GameObject nBlock = (GameObject)Instantiate(prefabBlock);

		nBlock.transform.position = new Vector3(nClicker.transform.position.x,nClicker.transform.position.y-nClicker.transform.localScale.y,nClicker.transform.position.z);
		nBlock.GetComponent<ScriptBlock>().MoveTo(nClicker.transform.position);

		nBlock.GetComponent<Renderer>().material.color = mDeadColor;
		nPosition.occupant = nBlock;
	}

	private bool DropCurrentPiece() {
		int nLit = 0;
		for(int r = 0; r < mGridSpaces && nLit < 4; r++) {
			for(int c = 0; c < mGridSpaces && nLit < 4; c++) {
				ScriptClicker nScriptClicker = mClickers[c,r].GetComponent<ScriptClicker>();
				if(nScriptClicker.IsLit()) {
					GameObject nBlock = (GameObject)Instantiate(prefabBlock);
					nBlock.transform.position = mClickers[c,r].transform.position;
					nBlock.GetComponent<Renderer>().material.color = mCurrentPiece.color;
					nScriptClicker.occupant = nBlock;
					nScriptClicker.SetUnlit();
					nLit++;
				}
			}
		}
		mLastDropPos = null;
		return nLit >= 4;
	}

	private bool ExplodeCurrentPiece() {
		int nLit = 0;
		for(int r = 0; r < mGridSpaces && nLit < 4; r++) {
			for(int c = 0; c < mGridSpaces && nLit < 4; c++) {
				ScriptClicker nScriptClicker = mClickers[c,r].GetComponent<ScriptClicker>();
				if(nScriptClicker.IsLit()) {
					if(nScriptClicker.occupant != null) {
						//Tell block to detach, used bomb
						//Destroy(nCell.occupant);
						nScriptClicker.occupant.GetComponent<ScriptBlock>().Bust();
						nScriptClicker.occupant.GetComponent<ScriptBlock>().SparkOn();
						nScriptClicker.occupant = null;
					}
					nScriptClicker.SetUnlit(); 
					nScriptClicker.SparkOff(true);
					nScriptClicker.ExplodeOn();
					nLit++;
				}
			}
		}
		mLastDropPos = null;
		return nLit >= 4;
	}
	
	private void HideDropPosition() {
		for(int r = 0; r < mGridSpaces; r++) {
			for(int c = 0; c < mGridSpaces; c++) {
				ScriptClicker s = mClickers[c,r].GetComponent<ScriptClicker>();
				s.SetUnlit();
				s.SparkOff(true);
			}
		}
		mLastDropPos = null;
	}

	private void ShowDropPosition(ScriptClicker nPosition) {
		mLastDropPos = null;

		ScriptClicker[] nScriptClickers = new ScriptClicker[4];
		int nLit = 0;
		for(int i = 0; i < 4 && nLit < 4; i++) {
			int nMaxR = GetMaxRow(i, nPosition.row, nPosition.column);
			int nMinR = GetMinRow(i, nPosition.row, nPosition.column);
			nLit = GetScriptClickers(i, nPosition.column, nPosition.row, nMinR, nMaxR, nScriptClickers); //Get drop position between min/max row
		}

		//Light up drop position, if any
		HideDropPosition();
		if(nLit < 4) 
			return;
		Color nColor = mCurrentPiece.color;
		nScriptClickers[0].SetLit(nColor);
		nScriptClickers[1].SetLit(nColor);
		nScriptClickers[2].SetLit(nColor);
		nScriptClickers[3].SetLit(nColor);
		if(mHasBomb) {
			nScriptClickers[0].SparkOn();
			nScriptClickers[1].SparkOn();
			nScriptClickers[2].SparkOn();
			nScriptClickers[3].SparkOn();
		}

		mLastDropPos = nPosition;
	}

	private int GetMaxRow(int nAnchor, int nStartRow, int nColumn) {
		//Gets first occupied row above nStartRow at nColumn
		int nMax = mGridSpaces;
		for(int i = 0; i < 4; i++) {
			ScriptBlock nScrBlk = mCurrentPiece.blocks[i].GetComponent<ScriptBlock>(); //ScriptBlock from piece block
			int nScrBlkCol = nScrBlk.GetColumn(nAnchor), nScrBlkRow = nScrBlk.GetRow(nAnchor);

			if(nColumn+nScrBlkCol < 0 || nStartRow+nScrBlkRow < 0) continue; //nScriptBlock.column and .row are relative to anchor
			if(nColumn+nScrBlkCol >= mGridSpaces || nStartRow+nScrBlkRow >= mGridSpaces) continue;
			
			for(int r = nStartRow+nScrBlkRow, c = nColumn+nScrBlkCol; r < mGridSpaces; r++) { 
				if(mClickers[c,r].GetComponent<ScriptClicker>().occupant != null) {
					if(r < nMax) nMax = r;
					break;
				}
			}
		}
		return nMax;
	}
	
	private int GetMinRow(int nAnchor, int nStartRow, int nColumn) {
		//Gets first occupied row below nStartRow at nColumn
		int nMin = 0;
		for(int i = 0; i < 4; i++) {
			ScriptBlock nScrBlk = mCurrentPiece.blocks[i].GetComponent<ScriptBlock>(); //ScriptBlock from piece block
			int nScrBlkCol = nScrBlk.GetColumn(nAnchor), nScrBlkRow = nScrBlk.GetRow(nAnchor);

			if(nColumn+nScrBlkCol < 0 || nStartRow+nScrBlkRow < 0) continue; //nScriptBlock.column and .row are relative to anchor
			if(nColumn+nScrBlkCol >= mGridSpaces || nStartRow+nScrBlkRow >= mGridSpaces) continue;
			
			for(int r = nStartRow+nScrBlkRow, c = nColumn+nScrBlkCol; r > -1; r--) { 
				if(mClickers[c,r].GetComponent<ScriptClicker>().occupant != null) {
					if(r > nMin) nMin = r;
					break;
				}
			}
		}
		return nMin;
	}
	
	private int GetScriptClickers(int nAnchor, int nC, int nR, ScriptClicker[] nScriptClickers) {
		//Get play area positions from a reference c, r
		//nC, nR are of play area grid
		int nLit = 0;
		for(int i = 0; i < 4 && nLit < 4; i++) {
			ScriptBlock nScrBlk = mCurrentPiece.blocks[i].GetComponent<ScriptBlock>(); //ScriptBlock from piece block
			int nScrBlkCol = nScrBlk.GetColumn(nAnchor), nScrBlkRow = nScrBlk.GetRow(nAnchor);

			if(nC+nScrBlkCol < 0 || nR+nScrBlkRow < 0) continue; //nScriptBlock.column and .row are relative to anchor
			if(nC+nScrBlkCol >= mGridSpaces || nR+nScrBlkRow >= mGridSpaces) continue;
			
			ScriptClicker nScriptClicker = mClickers[nC+nScrBlkCol,nR+nScrBlkRow].GetComponent<ScriptClicker>(); //ScriptClicker on play area grid
			if(nScriptClicker.occupant == null || mHasBomb) {
				nScriptClickers[nLit] = nScriptClicker;
				nLit++;
			}
		}
		return nLit;
	}
	
	private int GetScriptClickers(int nAnchor, int nCol, int nRow, int nMinRow, int nMaxRow, ScriptClicker[] nScriptClickers) {
		//Get play area positions from a reference c and max/min r
		int nLit = 0;
		if (mHasBomb) {
			for(int r = nRow; r < mGridSpaces && nLit < 4; r++)
				nLit = GetScriptClickers(nAnchor, nCol, nRow, nScriptClickers);
			for(int r = nRow; r >= 0 && nLit < 4; r--) 
				nLit = GetScriptClickers(nAnchor, nCol, nRow, nScriptClickers);
		} else {
			for(int r = nMinRow; r < nMaxRow && nLit < 4; r++) {
				if(mClickers[nCol,r].GetComponent<ScriptClicker>().occupant != null) continue;
				nLit = GetScriptClickers(nAnchor, nCol, r, nScriptClickers);
			}
		}
		return nLit;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//Setup

	private void DestroyPiece() {
		GameObject[] nBlocks = mCurrentPiece.blocks;
		foreach(GameObject nBlock in nBlocks) 
			Destroy(nBlock);
		mCurrentPiece.ResetBlocks();
	}

	private void CreateRandomPiece() {
		CreatePiece((ScriptPiece.PieceType)ScriptCommon.RandomEnum(typeof(ScriptPiece.PieceType)));
	}

	private void CreateTallestPiece() {
		CreatePiece(ScriptPiece.PieceType.I);
	}
	
	private void CreatePiece(ScriptPiece.PieceType nType, ScriptPiece.PieceRotate nRotate = ScriptPiece.PieceRotate.None) {
		Quaternion nAltRotation = goAlternate.transform.localRotation;
		goAlternate.transform.localRotation = Quaternion.identity;
		ScriptPiece nPiece = new ScriptPiece();
		GameObject[] nBlocks = new GameObject[4]{
			(GameObject)Instantiate(prefabBlock),
			(GameObject)Instantiate(prefabBlock),
			(GameObject)Instantiate(prefabBlock),
			(GameObject)Instantiate(prefabBlock)}; 
		float nTopPanelH = panelTop.GetComponent<RectTransform>().rect.height * canvas2d.scaleFactor;
		Vector3 nScreenPos = new Vector3(Screen.width / 2, Screen.height - nTopPanelH / 2, -cameraAlternate.transform.position.z);
		Vector3 nWorldPos = cameraAlternate.ScreenToWorldPoint(nScreenPos);
		Vector3 nPartSize = prefabBlock.transform.localScale;
		nPiece.SetBlocks(nWorldPos, nPartSize, nPartSize, nBlocks, nType, nRotate);
		mCurrentPiece = nPiece;
		goAlternate.transform.localRotation = nAltRotation;
	}

	private void CreateClickers() {
		//Place invisible clickers to create click grid - BL is c0,r0
		float nW = prefabClicker.transform.localScale.x;
		float nH = prefabClicker.transform.localScale.y;
		float nD = prefabClicker.transform.localScale.z;
		float nX = goPlayArea.transform.position.x - nW * mGridSpaces / 2 + nW / 2;
		float nY = goPlayArea.transform.position.y - nH * mGridSpaces / 2 + nH / 2 + mGridOffY;
		for (int r = 0; r < mGridSpaces; r++) {
			for (int c = 0; c < mGridSpaces; c++) {	
				GameObject nObject = Instantiate(prefabClicker, new Vector3 (nX+nW*c, nY+nH*r, mGridZ), Quaternion.identity) as GameObject;
				nObject.transform.localScale = new Vector3(nW,nD,nD);
				nObject.GetComponent<ScriptClicker>().column = c;
				nObject.GetComponent<ScriptClicker>().row = r;
				nObject.GetComponent<ScriptClicker>().receiver = PlayerActionClicker;
				mClickers[c, r] = nObject;
			}
		}
	}

	private void ZoomToPlayfield() {
		Transform nCam = Camera.main.transform;

		//Zoom - uses top and bottom UI panels to determine available size
		float nTopPanelH = panelTop.GetComponent<RectTransform>().rect.height * canvas2d.scaleFactor;
		float nBotPanelH = panelBottom.GetComponent<RectTransform>().rect.height * canvas2d.scaleFactor;
		float nAvailH = mMainSizeFactor * (Screen.height - nTopPanelH - nBotPanelH);
		//Height moves cam to fit, if it is bigger or smaller than avail
		while(ScriptCommon.ScreenObjectHeight(goPlayArea) < nAvailH)
			nCam.position = new Vector3(nCam.position.x, nCam.position.y, nCam.position.z + mZoomStep);
		while(ScriptCommon.ScreenObjectHeight(goPlayArea) > nAvailH)
			nCam.position = new Vector3(nCam.position.x, nCam.position.y, nCam.position.z - mZoomStep);
		//Width moves cam only if it is bigger than avail
		float nAvailW = mMainSizeFactor * Screen.width;
		while(ScriptCommon.ScreenObjectWidth(goPlayArea) > nAvailW)
			nCam.position = new Vector3(nCam.position.x, nCam.position.y, nCam.position.z - mZoomStep);

		//Pan - Uses top and bottom UI panels to determine center point
		float nScreenC = nBotPanelH + (Screen.height - nTopPanelH - nBotPanelH) / 2;
		float nAreaC = ScriptCommon.ScreenDistanceBottom(goPlayArea) + ScriptCommon.ScreenObjectHeight(goPlayArea) / 2;
		while(nAreaC > nScreenC) {
			nCam.position = new Vector3(nCam.position.x, nCam.position.y + mPanStep, nCam.position.z);
			nAreaC = ScriptCommon.ScreenDistanceBottom(goPlayArea) + ScriptCommon.ScreenObjectHeight(goPlayArea) / 2;
		}
		while(nAreaC < nScreenC) {
			nCam.position = new Vector3(nCam.position.x, nCam.position.y - mPanStep, nCam.position.z);
			nAreaC = ScriptCommon.ScreenDistanceBottom(goPlayArea) + ScriptCommon.ScreenObjectHeight(goPlayArea) / 2;
		}
	}

	private void ZoomAlternate() {
		Transform nCam = cameraAlternate.transform;

		//Zoom - uses top UI panel to determine available size
		float nTopPanelH = panelTop.GetComponent<RectTransform>().rect.height * canvas2d.scaleFactor;
		float nAvailH = mAlternateSizeFactor * nTopPanelH;
		//Height moves cam to fit, if it is bigger or samller than avail
		while(mCurrentPiece.GetScreenHeight(cameraAlternate) < nAvailH)
			nCam.position = new Vector3(nCam.position.x, nCam.position.y, nCam.position.z + mZoomStep);
		while(mCurrentPiece.GetScreenHeight(cameraAlternate) > nAvailH)
			nCam.position = new Vector3(nCam.position.x, nCam.position.y, nCam.position.z - mZoomStep);
		
		//Pan - Uses top UI panel to determine center point
		float nScreenC = Screen.height - nTopPanelH / 2;
		while(mCurrentPiece.GetScreenCenterY(cameraAlternate) > nScreenC)
			nCam.position = new Vector3(nCam.position.x, nCam.position.y + mPanStep, nCam.position.z);
		while(mCurrentPiece.GetScreenCenterY(cameraAlternate) < nScreenC) 
			nCam.position = new Vector3(nCam.position.x, nCam.position.y - mPanStep, nCam.position.z);
	}
	//.Class
}










