using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EG_ISPY_LevelController : MonoBehaviour
{
	[SerializeField] public string levelType;
	[SerializeField] public EG_ISPY_NetworkHelper netHelper;
	[SerializeField] EG_ISPY_CustomLevel myCustomLevel;
	[SerializeField] EG_ISPY_ScaffHelper scaffHelper;

	[SerializeField] public bool isMechScaffEnabled;

	[SerializeField] public Transform speechBubble;
	[SerializeField] public TMP_Text speechText;
	[SerializeField] public Transform mainCharacter;
	[SerializeField] public GameObject coin, coinjar;
	[SerializeField] public GameObject tappingHandGO;
	[SerializeField] public Image bgImage;
	[SerializeField] public ParticleSystem confetti;
	[SerializeField] public List<AudioClip> entryDialogueOptions;

	[SerializeField] public Material defaultMaterial;
	[SerializeField] public Material outlineMaterial;

	[HideInInspector] public int currLevelIndex;
	[HideInInspector] public bool[] wrongChecks;
	[HideInInspector] public bool playSFX, playBGmusic;

	private void disableAllInteractions()
	{
		foreach(var obj in myCustomLevel.allObjects)
			obj.gameobj.GetComponent<PolygonCollider2D>().enabled = false;
		mainCharacter.GetComponent<PolygonCollider2D>().enabled = false;
	}

	private void enableAllInteractions()
	{
		foreach(var obj in myCustomLevel.allObjects)
			obj.gameobj.GetComponent<PolygonCollider2D>().enabled = true;
		mainCharacter.GetComponent<PolygonCollider2D>().enabled = true;
	}

	private void enableCorrectInteractions()
	{
		foreach(var obj in myCustomLevel.customObjects[currLevelIndex].correctObjects)
			obj.GetComponent<PolygonCollider2D>().enabled = true;
		mainCharacter.GetComponent<PolygonCollider2D>().enabled = true;
	}

	private void SayDialogues(string[] dialogues)
	{
		disableAllInteractions();
		if(dialogues.Length>=1)
		{
			speechBubble.DOScale(new Vector3(45f, 45f, 45f), 1f);
			speechText.text = dialogues[0];
			speechBubble.DOScale(Vector3.zero, 0.5f).SetDelay(2.5f).OnComplete(() => {
				if(dialogues.Length>=2)
				{
					speechBubble.DOScale(new Vector3(45f, 45f, 45f), 1f);
					speechText.text = dialogues[1];
					speechBubble.DOScale(Vector3.zero, 0.5f).SetDelay(2.5f).OnComplete(() => {
						if(dialogues.Length>=3)
						{
							speechBubble.DOScale(new Vector3(45f, 45f, 45f), 1f);
							speechText.text = dialogues[2];
							speechBubble.DOScale(Vector3.zero, 0.5f).SetDelay(2.5f).OnComplete(() => {
								if(dialogues.Length>=4)
								{
									speechBubble.DOScale(new Vector3(45f, 45f, 45f), 1f);
									speechText.text = dialogues[3];
									speechBubble.DOScale(Vector3.zero, 0.5f).SetDelay(2.5f);
								}
							});
						}
					});
				}
			});
		}
	}

	private void Start()
	{
		// get level details from spreadsheet
		netHelper.fetchInfo((EG_ISPY_CustomLevel clevel) => {
			myCustomLevel = clevel;
		}, (bool done) => {
			if(done)
				print("done loading from spreadsheet ...");

			Invoke("StartDelayedInitializations", 5f);
		});

		// play music and sfx
		if(PlayerPrefs.GetInt("play_music")==1) {
			playBGmusic = true;
			gameObject.GetComponent<EG_ISPY_AudioManager>().bgAudioSource.Play(0);
		}
		else
			playBGmusic = false;
		if(PlayerPrefs.GetInt("play_sfx")==1)
			playSFX = true;
		else
			playSFX = false;

		currLevelIndex = 0;
		wrongChecks = new bool[] {false, false, false};
		speechBubble.DOScale(Vector3.zero, 0.1f);

		// isMechScaffEnabled = true;					// uncomment these lines if you want
		// PlayerPrefs.SetInt("mechScaffEnabled", 1);	// to test mechanical scaffolding
		isMechScaffEnabled = (PlayerPrefs.GetInt("mechScaffEnabled")!=0);

		coin.SetActive(false);
		coinjar.SetActive(false);
	}

	private void StartDelayedInitializations()
	{
		// attach colliders
		print("attaching colliders...");
		foreach(var combo in myCustomLevel.allObjects)
		{
			// print("adding collider to "+combo.gameobj.name);
			combo.gameobj.AddComponent<PolygonCollider2D>();
		}
		// start level
		// play opening dialogues
		string[] dialogueOptions = {"Hmm I think I was here last", "Let’s look here", "I think I left my things here"};
		int randTemp1 = UnityEngine.Random.Range(0, dialogueOptions.Length);
		SayDialogues(new string[] {dialogueOptions[randTemp1]});
		gameObject.GetComponent<EG_ISPY_AudioManager>().playCustomSound(entryDialogueOptions[randTemp1]);
		Invoke("SayGoodLuckDialogue", 3.5f);

		// have some delay before starting first level, so dialogues can finish
		Invoke("PlayIndividualLevel", 7f);
	}

	private void SayGoodLuckDialogue()
	{
		SayDialogues(new string[] {"good luck!!"});
		gameObject.GetComponent<EG_ISPY_AudioManager>().playCustomSound(entryDialogueOptions[3]);
	}

	private void PlayIndividualLevel()
	{
		print("level started");

		// get current level details
		if(currLevelIndex < myCustomLevel.customObjects.Count)
		{
			enableAllInteractions();

			var customObj = myCustomLevel.customObjects[currLevelIndex];

			// detatch existing scripts
			foreach(var obj in myCustomLevel.allObjects)
			{
				Destroy(obj.gameobj.GetComponent<EG_ISPY_WrongAnswer>());
				Destroy(obj.gameobj.GetComponent<EG_ISPY_CorrectAnswer>());
			}

			// attach apppropriate scripts
			foreach(var obj in myCustomLevel.allObjects)
			{
				if(myCustomLevel.customObjects[currLevelIndex].correctObjects.Contains(obj.gameobj))
					obj.gameobj.AddComponent<EG_ISPY_CorrectAnswer>();
				else
					obj.gameobj.AddComponent<EG_ISPY_WrongAnswer>();
			}

			// start idle scaffolding timer
			if(currLevelIndex==0)
			{
				Invoke("idleScaffoldingStart", 3f);
			}

			// say some initial dialogues
			string rawtext1 = customObj.initialDialogues[UnityEngine.Random.Range(0, customObj.initialDialogues.Count)];
			SayDialogues(new string[] {rawtext1.Split('-')[0].Trim()});
			StartCoroutine(PlayAudioFile(rawtext1.Split('-')[1].Trim(), (float x) => {}, (int isError) => {
				print("audio file not found !!!");
			}));

			// enable interaction after dialoues played
			Invoke("enableAllInteractions", 3.5f);

			// wait for any player interaction -> waiting for onCorrectObjectTap and onWrongObjectTap
		}
		else
		{
			disableAllInteractions();

			print("all objects of current scene completed");

			// background image and all objects disappear
			bgImage.enabled = false;
			foreach(var obj in myCustomLevel.allObjects)
				obj.gameobj.SetActive(false);

			// character jumps to center of screen with sparkles
			// play closing dialogues
			speechBubble.DOMove(new Vector3(2f, 2.5f, 0), 0.1f);
			mainCharacter.DOScale(new Vector3(-4.24f, 4.24f, 1f), 1.5f);
			mainCharacter.DOMove(Vector3.zero, 1.5f).OnComplete(() => {
				SayDialogues(new string[] {"Thank you for helping me find all of my things"});
				gameObject.GetComponent<EG_ISPY_AudioManager>().playCustomSound(entryDialogueOptions[4]);
				Invoke("SayRewardDialogue", 3.7f);
				// coin jar animation
				Invoke("coinJarAnimation", 7.4f);
			});
		}
	}

	private void SayRewardDialogue()
	{
		SayDialogues(new string[] {"As a reward, here's a little something for you"});
		gameObject.GetComponent<EG_ISPY_AudioManager>().playCustomSound(entryDialogueOptions[5]);
	}

	private void coinJarAnimation()
	{
		coin.SetActive(true);
		coinjar.SetActive(true);
		mainCharacter.gameObject.SetActive(false);
	}

	public void onCoinTap()
	{
		coin.transform.DOMove(new Vector3(7.7f, 4.2f, 1f), 1.5f);
		coin.transform.DOScale(Vector3.zero, 1.5f).OnComplete(() => {
			// go back to selection screen
			SceneManager.LoadScene("I SPY Home Scene");
		});
	}

	public void onCharacterTap()
	{
		// say dialogue specified in character tap field
		string dialogue1 = myCustomLevel.customObjects[currLevelIndex].onCharacterTapSentence;

		SayDialogues(new string[] {dialogue1.Split('-')[0].Trim()});

		StartCoroutine(PlayAudioFile(dialogue1.Split('-')[1].Trim(), (float x) => {}, (int isError) => {
			print("audio file not found !!!");
		}));

		// if 3rd wrong hint has already played, do not enable interactions
		if(!(wrongChecks[0] && wrongChecks[1] && wrongChecks[2]))
			Invoke("enableAllInteractions", 4f);
		else
			Invoke("enableCorrectInteractions", 4f);
	}

	public void onCorrectObjectTap()
	{
		print("correct object tapped");

		// disable mechanical scaffolding		
		isMechScaffEnabled = false;
		PlayerPrefs.SetInt("mechScaffEnabled", 0);
		// reset timer
		scaffHelper.ResetTimer();

		// remove red outline (if 3rd wrong hint was played)
		removeOutline();

		// add green outline to correct objects
		foreach(var obj in myCustomLevel.customObjects[currLevelIndex].correctObjects)
		{
			outlineMaterial.SetColor("_OutlineColor", Color.green);
			outlineMaterial.SetFloat("_EmissionIntensity", 2f);
			obj.GetComponent<Renderer>().material = outlineMaterial;
		}

		// change back to NO_OUTLINE material after some delay
		Invoke("removeOutline", 2f);

		// if 3rd hint used
		if(wrongChecks[2])
		{
			// reset the wrong checks
			wrongChecks[0] = false;
			wrongChecks[1] = false;
			wrongChecks[2] = false;

		}
		// if 3rd hint not yet used 
		else
		{
			wrongChecks[0] = false;
			wrongChecks[1] = false;
			wrongChecks[2] = false;
		}

		var customObj = myCustomLevel.customObjects[currLevelIndex];
		SayDialogues(new string[] {customObj.endDialogues[UnityEngine.Random.Range(0, customObj.endDialogues.Count)]});

		// play correct answer sfx
		gameObject.GetComponent<EG_ISPY_AudioManager>().playCorrectObjSound();

		// reset timer
		scaffHelper.ResetTimer();

		// call next level after this ending dialogue is played
		Invoke("callNextLevel", 4f);
	}

	private void callNextLevel()
	{
		currLevelIndex++;
		PlayIndividualLevel();
	}

	private void removeOutline()
	{
		foreach(var obj in myCustomLevel.allObjects)
			obj.gameobj.GetComponent<Renderer>().material = defaultMaterial;
	}

	public void onWrongObjectTap()
	{
		print("wrong object tapped");

		// disable mechanical scaffolding		
		isMechScaffEnabled = false;
		PlayerPrefs.SetInt("mechScaffEnabled", 0);
		// reset timer
		scaffHelper.ResetTimer();

		// false false false -> no wrongs yet
		// true  false false -> 1st wrong
		// true  true  false -> 2nd wrong
		// true  true  true  -> 3rd wrong

		if(wrongChecks[0]) {
			if(wrongChecks[1]) {
				// 3rd time wrong
				wrongChecks[2] = true;
				// say 3rd hint dialogue (also disable other interactions)
				SayDialogues(new string[] {myCustomLevel.customObjects[currLevelIndex].wrongHints[2]});
				// disable all colliders after 3rd hint (except for correct objects) (then only enable after correct obj is pressed)
				foreach(var obj in myCustomLevel.customObjects[currLevelIndex].correctObjects)
					obj.GetComponent<PolygonCollider2D>().enabled = true;

				// change to OUTLINE material & revert back after some delay & highlight correct objects
				Invoke("removeOutline3rdhint", gameObject.GetComponent<EG_ISPY_AudioManager>().wrongObjSound.length);

				// play wrong answer sfx
				gameObject.GetComponent<EG_ISPY_AudioManager>().playWrongObjSound();
				
			} else {
				// 2nd time wrong
				wrongChecks[1] = true;
				disableAllInteractions();
				// play this AUDIO file (from URL)
				// SayDialogues(new string[] {myCustomLevel.customObjects[currLevelIndex].wrongHints[1]});
				StartCoroutine(PlayAudioFile(myCustomLevel.customObjects[currLevelIndex].wrongHints[1], (float x) => {
					// enable interactions after a while
					Invoke("enableAllInteractions", x);
					Invoke("removeOutline", x);
				}, (int isError) => {
					// say 1st hint dialogue
					SayDialogues(new string[] {myCustomLevel.customObjects[currLevelIndex].wrongHints[0]});
					// enable interactions after a while
					Invoke("enableAllInteractions", 4.5f);

					Invoke("removeOutline", gameObject.GetComponent<EG_ISPY_AudioManager>().wrongObjSound.length+0.5f);

					// play wrong answer sfx
					gameObject.GetComponent<EG_ISPY_AudioManager>().playWrongObjSound();
				}));
				

			}
		} else {
			wrongChecks[0] = true;
			// say 1st hint dialogue
			SayDialogues(new string[] {myCustomLevel.customObjects[currLevelIndex].wrongHints[0]});
			// enable interactions after a while
			Invoke("enableAllInteractions", 4f);

			Invoke("removeOutline", gameObject.GetComponent<EG_ISPY_AudioManager>().wrongObjSound.length);

			// play wrong answer sfx
			gameObject.GetComponent<EG_ISPY_AudioManager>().playWrongObjSound();

		}

		// reset timer
		scaffHelper.ResetTimer();
	}

	private void removeOutline3rdhint()
	{
		removeOutline();

		foreach(var obj in myCustomLevel.customObjects[currLevelIndex].correctObjects)
		{
			outlineMaterial.SetColor("_OutlineColor", Color.green);
			outlineMaterial.SetFloat("_EmissionIntensity", 5f);
			obj.GetComponent<Renderer>().material = outlineMaterial;
		}
	}

	IEnumerator PlayAudioFile(string url, Action<float> startedPlaying, Action<int> isError)
	{
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();
 
            if (www.isNetworkError)
            {
                Debug.Log(www.error);
                isError(1);
            }
            else
            {
                var myClip = DownloadHandlerAudioClip.GetContent(www);
                gameObject.GetComponent<EG_ISPY_AudioManager>().audioSource.clip = myClip;
                gameObject.GetComponent<EG_ISPY_AudioManager>().audioSource.Play();
                Debug.Log("Audio from URL is playing...");
                startedPlaying(myClip.length);
            }
        }
	}

	private void idleScaffoldingStart()
	{
		scaffHelper.StartTimer();
	}

	public void onIdleScaffActivate()
	{
		print("idle scaffolding activated");

		// disable interactions
		disableAllInteractions();

		// say some audio
		StartCoroutine(PlayAudioFile(myCustomLevel.customObjects[currLevelIndex].idleScaffoldingAudioClip, (float x) => {
			// enable interactions after a while (do this after audio finishes playing)
			// if 3rd wrong hint has already played, do not enable interactions
			if(!(wrongChecks[0] && wrongChecks[1]))
				Invoke("enableAllInteractions", x);
			else
				Invoke("enableCorrectInteractions", x);
		}, (int isError) => {
			// is not able to connect to internet, play from local audio file
			gameObject.GetComponent<EG_ISPY_AudioManager>().playIdleScaffoldingSound();

			// enable interactions after a while (do this after audio finishes playing)
			// if 3rd wrong hint has already played, do not enable interactions
			if(!(wrongChecks[0] && wrongChecks[1]))
				Invoke("enableAllInteractions", gameObject.GetComponent<EG_ISPY_AudioManager>().idleScaffoldingSound.length);
			else
				Invoke("enableCorrectInteractions", gameObject.GetComponent<EG_ISPY_AudioManager>().idleScaffoldingSound.length);
		}));

		// all objects will do a jiggle
		foreach(var obj in myCustomLevel.allObjects)
			shakeObjects(obj.gameobj.transform);

		// reset timer
		scaffHelper.ResetTimer();
	}

	public void onMechScaffActivate()
	{
		print("machanical scaffolding activated");

		disableAllInteractions();

		gameObject.GetComponent<EG_ISPY_AudioManager>().playMechScaffoldingSound();

		// show tapping hand on screen
		tappingHandGO.SetActive(true);

		tappingHandGO.transform.DOScale(Vector3.zero, 0.01f).OnComplete(()=>{
			tappingHandGO.transform.DOScale(new Vector3(20f, 20f, 20f), 1f).SetDelay(0.3f).OnComplete(()=>{
				tappingHandGO.transform.DOScale(Vector3.zero, 0.1f);
			});
		});

		// enable interactions after a while (do this after audio finishes playing)
		Invoke("enableAllInteractions", gameObject.GetComponent<EG_ISPY_AudioManager>().mechScaffoldingSound.length);

		// reset timer
		scaffHelper.ResetTimer();
	}

	private void shakeObjects(Transform tr)
	{
		float duration = 1f, strength = 0.5f;
		tr.DOShakePosition(duration, strength);
		tr.DOShakeRotation(duration, strength);
		tr.DOShakeScale(duration, strength);
	}

	public void onBackButtonPress()
	{
		SceneManager.LoadSceneAsync(0);
		if(playSFX)
		{
			gameObject.GetComponent<EG_ISPY_AudioManager>().playButtonSound();
		}
	}
}
