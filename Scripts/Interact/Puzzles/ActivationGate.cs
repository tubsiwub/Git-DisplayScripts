﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationGate : MonoBehaviour {

	// Inspector Toggles 
	public bool timerToggle;
	public bool useSavingLoadingCheck;

	// Make sure TimerObj and WinZone are named correctly and within the same parent as the GateObj
	GameObject timerObj;
	public GameObject winZone;
	bool winState = false;	// if WinZone was reached
	public bool openAfterWin;
	public bool openBeforeWin;

	public Vector3 openLocation;
	public Vector3 openPosTemp;
	Vector3 closedLocation;

	// Not necessary for timer puzzle
	public GameObject buttonObj; 
	public GameObject questObj;	// quest type 
	public GameObject questNPC;	// quest NPC
	public GameObject triggerObj;	


	public bool questObject;

	string storageKey = "";


	void Start () {

		timerObj = GameObject.Find ("TimerUI");

		EventListeners (true);

		SceneLoader.OnSceneLoaderLoad += ResetListeners;

		closedLocation = transform.localPosition;

		// Start the gate as closed
		transform.localPosition = closedLocation;

		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;

			// If this object relies on a quest, use that instead
			if (questObject) {

				if (SavingLoading.instance.LoadQuestStatus_Container(storageKey) == QUEST_STATUS.FINISHED ||
					SavingLoading.instance.LoadQuestStatus_Container(storageKey) == QUEST_STATUS.COMPLETE) 
				{
					GateMove ();
				}

			} else {
				
				if (SavingLoading.instance.CheckStorageKeyStatus (storageKey)) 
				{
					GateMove ();
				}
			}
		}
	}

	void ResetListeners(){

		EventListeners (false);

	}

	// true to add, false to remove
	void EventListeners(bool toggle){

		if (toggle) {

			if (!timerToggle) {

				// Events
				if (buttonObj) {
					if (!openAfterWin) {
						buttonObj.GetComponent<PuzzleButton> ().OnButtonActivated += GateMove;			// Move gate to open if button check succeeds
						buttonObj.GetComponent<PuzzleButton> ().OnButtonReset += GateReset;				// Be mean and close the gate if the player sucks at counting
					}
				}

				if (questObj) {
					if (!openAfterWin) {
						questObj.GetComponent<NPC_QuestType> ().OnQuestComplete += GateMove;		
						questObj.GetComponent<NPC_QuestType> ().OnQuestFailed += GateReset;	
					}
				}

				if (questNPC) {
					if (!openAfterWin) {
						questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted += GateMove;		
						questNPC.GetComponent<NPC_QuestContainer> ().OnQuestFailed += GateReset;	
					}
				}

			}

			if (timerToggle) {

				// Events
				if (winZone) {
					winZone.GetComponent<WinZone> ().OnWinZoneActivate += Complete;
					if (openAfterWin)
						winZone.GetComponent<WinZone> ().OnWinZoneActivate += GateMove;
				}

				if (timerObj) {
					if (!openAfterWin) {
						timerObj.GetComponent<ActivatedTimer> ().OnTimerStart += GateMove;
						timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut += GateReset;
					}
				}

			}

			if (triggerObj) {
				if (!openAfterWin) {
					if (triggerObj.GetComponent<Cutscene_TriggerScript> ())
						triggerObj.GetComponent<Cutscene_TriggerScript> ().OnTriggered += GateMove;
					if (triggerObj.GetComponent<EnemyDefeatCounter_Trigger> ()) {
						triggerObj.GetComponent<EnemyDefeatCounter_Trigger> ().OnTriggered += GateMove;
					}
				}
				if(timerObj)
					timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut += GateReset;
			}
		} else {

			if(!timerToggle){

				// Events
				if(buttonObj){
					if(!openAfterWin){
						buttonObj.GetComponent<PuzzleButton>().OnButtonActivated -= GateMove;			// Move gate to open if button check succeeds
						buttonObj.GetComponent<PuzzleButton>().OnButtonReset -= GateReset;				// Be mean and close the gate if the player sucks at counting
					}
				}

				if(questObj){
					if (!openAfterWin) {
						questObj.GetComponent<NPC_QuestType> ().OnQuestComplete -= GateMove;		
						questObj.GetComponent<NPC_QuestType> ().OnQuestFailed -= GateReset;	
					}
				}

				if(questNPC){
					if (!openAfterWin) {
						questNPC.GetComponent<NPC_QuestContainer> ().OnQuestStarted -= GateMove;		
						questNPC.GetComponent<NPC_QuestContainer> ().OnQuestFailed -= GateReset;	
					}
				}

			}

			if (timerToggle) {

				// Events
				if (winZone) {
					winZone.GetComponent<WinZone> ().OnWinZoneActivate -= Complete;
					if(openAfterWin)
						winZone.GetComponent<WinZone> ().OnWinZoneActivate -= GateMove;
				}

				if (timerObj) {
					if (!openAfterWin) {
						timerObj.GetComponent<ActivatedTimer> ().OnTimerStart -= GateMove;
						timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut -= GateReset;
					}
				}

			}

			if (triggerObj) {
				if (!openAfterWin) {
					if(triggerObj.GetComponent<Cutscene_TriggerScript> ())
						triggerObj.GetComponent<Cutscene_TriggerScript> ().OnTriggered -= GateMove;
					if (triggerObj.GetComponent<EnemyDefeatCounter_Trigger> ()) {
						triggerObj.GetComponent<EnemyDefeatCounter_Trigger> ().OnTriggered -= GateMove;
					}
				}
				if(timerObj)
					timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut -= GateReset;
			}

		}

	}

	void Update () {
		
	}

	#region Event Functions

	void Complete(){
		
		winState = true;

		if (isClosing)
			StopCoroutine ("ShiftGateClosed");

		EventListeners (false);

		StartCoroutine ("ShiftGateOpen");

	}

	void GateReset() {

		if (isOpening)
			StopCoroutine ("ShiftGateOpen");

		if(!winState)
			StartCoroutine ("ShiftGateClosed");

	}

	void GateMove() {

		if (isClosing)
			StopCoroutine ("ShiftGateClosed");

		StartCoroutine ("ShiftGateOpen");

	}
	#endregion

	#region Coroutine Enums
	bool isOpening = false;
	IEnumerator ShiftGateOpen(){
		
		isOpening = true;

		while (Vector3.Distance(transform.localPosition, openLocation) > 0.001f) {

			transform.localPosition = Vector3.MoveTowards(transform.localPosition, openLocation, 15 * Time.deltaTime);

			yield return new WaitForEndOfFrame ();

		}

		isOpening = false;

	}

	bool isClosing = false;
	IEnumerator ShiftGateClosed(){

		isClosing = true;

		while (Vector3.Distance(transform.localPosition, closedLocation) > 0.001f) {

			transform.localPosition = Vector3.MoveTowards(transform.localPosition, closedLocation, 15 * Time.deltaTime);

			yield return new WaitForEndOfFrame ();

		}

		isClosing = false;

	}
	#endregion

	// temp storage
	public float x, y, z;


}
