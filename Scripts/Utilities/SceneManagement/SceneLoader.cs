﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class SceneLoader : MonoBehaviour
{
	public delegate void SceneLoader_Loaded();
	public static event SceneLoader_Loaded OnSceneLoaderLoad;	// fire when triger enter

	[SerializeField] string[] scenePaths = new string[1];
	[SerializeField] bool showLoadScreen = false;
	[SerializeField] LoaderID loaderId = 0;

	public enum LoaderID { None, Tutorial, Pickup, Mountain };
	public LoaderID GetID { get { return loaderId; } }

	private string[] sceneNames;

	PlayerHandler playerHandler;
	CameraControlDeluxe cam;

	Collider col;
	GameObject loadingCanvas;
	GameObject loadCanvRef = null;
	bool reEnableTrigger = true;
	bool respawnOnFinishLoad = false;

	const float LOADSCREEN_TIMEOUT = 30;
	int finishedLoadCount = 0;

	void Awake()
	{
		col = GetComponent<Collider>();

		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
		cam = Camera.main.GetComponent<CameraControlDeluxe>();
		SceneManager.sceneLoaded += SceneLoaded;
	}

	void Start()
	{
		loadingCanvas = Resources.Load<GameObject>("LoadingCanvas");

		sceneNames = new string[scenePaths.Length];
		for (int i = 0; i < sceneNames.Length; i++)
			sceneNames[i] = PathToName(scenePaths[i]);

		if (loaderId != LoaderID.None)
			LevelManager.instance.AddSceneLoader(this);
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (finishedLoadCount > 0)
		{
			finishedLoadCount--;

			if (finishedLoadCount == 0)
			{
				playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
				EndLoadScreen();
			}
		}
	}

	string PathToName(string path)
	{
		string name = "";

		for (int i = path.Length-1; i >= 0; i--)
		{
			if (path[i] == '/')
				break;

			name += path[i];
		}

		if (name != "")
		{
			name = name.Remove(0, 6);
			name = Reverse(name);
		}

		return name;
	}

	string Reverse(string s)
	{
		char[] charArray = s.ToCharArray();
		Array.Reverse(charArray);
		return new string(charArray);
	}

	void OnDrawGizmos()
	{
		if (col == null) col = GetComponent<Collider>();

		Gizmos.color = Color.green;
		Gizmos.DrawCube(col.bounds.center, col.bounds.extents * 2);
		Gizmos.DrawWireCube(col.bounds.center, col.bounds.extents * 2);
	}

	void OnTriggerEnter(Collider obj)
	{
		if (obj.gameObject.tag != "Player") return;

		if (!reEnableTrigger) return;   // make sure trigger doesn't happen a bunch
		
		BeginSceneLoad(false);

		reEnableTrigger = false;
	}

	public void BeginSceneLoad(bool respawnOnFinishLoad)
	{
		for (int i = 0; i < sceneNames.Length; i++)
		{
			if (!SceneManager.GetSceneByName(sceneNames[i]).isLoaded)
				finishedLoadCount++;
		}

		bool loadedAnything = LevelManager.instance.Load(sceneNames);

		if (loadedAnything)
		{
			if (OnSceneLoaderLoad != null)
				OnSceneLoaderLoad();
		}

		LevelManager.instance.UnloadOthers(sceneNames);

		if (loadedAnything)
		{
			this.respawnOnFinishLoad = respawnOnFinishLoad;

			if (showLoadScreen)
			{
				loadCanvRef = Instantiate(loadingCanvas);
				StartCoroutine("LoadScreenTimeOut");

				playerHandler.SetFrozen(true, true);
				cam.SetFreeze(true);
			}
		}
	}

	void OnTriggerExit(Collider obj)
	{
		if (obj.gameObject.tag != "Player") return;
		reEnableTrigger = true;
	}

	void EndLoadScreen()
	{
		StopCoroutine("LoadScreenTimeOut");

		if (loadCanvRef != null) Destroy(loadCanvRef);
		loadCanvRef = null;

		playerHandler.SetFrozen(false, false);
		cam.SetFreeze(false);

		StartCoroutine(WaitForCheckpointToClearThenSet());
	}

	IEnumerator WaitForCheckpointToClearThenSet()
	{
		yield return null;
		playerHandler.SetCheckpoint(transform.position + (Vector3.up * 5), Quaternion.LookRotation(Vector3.forward));

		if (respawnOnFinishLoad)
			playerHandler.Respawn();

		respawnOnFinishLoad = false;
	}

	IEnumerator LoadScreenTimeOut()
	{
		yield return new WaitForSeconds(LOADSCREEN_TIMEOUT);
		EndLoadScreen();
	}
}
