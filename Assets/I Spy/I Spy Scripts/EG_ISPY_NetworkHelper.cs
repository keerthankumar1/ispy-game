using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EG_ISPY_NetworkHelper : MonoBehaviour
{
	[SerializeField] public string spreadsheetURL;
	[SerializeField] public Image bgImage;
	[SerializeField] public Transform allObjectsParentTransform;

	public void fetchInfo(Action<EG_ISPY_CustomLevel> callback, Action<bool> done)
	{
		if(spreadsheetURL==null) {
			callback(null);
			done(true);
			return;
		}

		Get(spreadsheetURL, (string error) => {
			Debug.Log(error);
		}, (string text) => {
			callback(processInfo(text));
			done(true);	// this doesn't work
		});
	}

	private EG_ISPY_CustomLevel processInfo(string rawtext)
	{
		/*
		Level 1 - playground,Background image,https://upload.wikimedia.org/wikipedia/commons/thumb/8/84/Holy_SURP_Hovhannes_Church.jpg/750px-Holy_SURP_Hovhannes_Church.jpg,,,,
		all objects,Basketball_boy_img_1,backetball_boy_img_2,skipping_girl,cycling_boy,running boy,football boy
		object positions,1 5,2.5 5,1 1,0 0,0 0,0 0
		object sizes,1,0.8,1.1,-,-,-
		Object 1 - basketball,,,,,,
		correct objects,Basketball_boy_img_1,basketball_boy_img_2,,,,
		initial dialogues,Can you help me find the boy with the basketball?,Who is playing with the basketball?,,,,
		ending dialogues,Yay! you found the boy,Hurray !!,,,,
		idle scaffolding,<audio>,,,,,
		on character tap,basketball,,,,,
		wrong hints,Boy playing basketball,<maybe audio>,"Here, let me help you",,,
		Object 2 - Skipping girl,,,,,,
		correct objects,skipping_girl,,,,,
		initial dialogues,Help me find the skipping girl,Who is playing with skipping rope?,,,,
		ending dialogues,Yay! you found her,Hurray !!,,,,
		idle scaffolding,<audio>,,,,,
		on character tap,skipping rope,,,,,
		wrong hints,Girl with skipping rope,<audio>,There she is,,,
		~,,,,,,
		*/

		// pick randomly - TODO: handle irrelevant characters coming up - DONE
		string[] allLevels = rawtext.Split('~');
		// printArray(allLevels);
		print(allLevels.Length-2 + " levels found");
		int tempNum = UnityEngine.Random.Range(0, allLevels.Length);
		if(tempNum==0 || tempNum==(allLevels.Length))
			tempNum = 1;	// this is because first & last level strings will be empty
		string someLevel = allLevels[tempNum];
		// string someLevel = allLevels[0];
		someLevel = someLevel.Trim();
		// TODO - remove ,,,,,, at the start of this text
		someLevel = removeInitialTemps(someLevel);
		print(someLevel);
		// remove ~ at end

		EG_ISPY_CustomLevel myCustomLevel = new EG_ISPY_CustomLevel();
		myCustomLevel.allObjects = new List<Name_url_combo>();
		myCustomLevel.customObjects = new List<CustomObject>();

		// extract background image URL and load into scene
		string row1 = someLevel.Split('\n')[0];
		print("Loaded: " + row1.Split(',')[0]);
		myCustomLevel.backgroundImageURL = row1.Split(',')[2];
		putBGImage(myCustomLevel);

		// extract all objects, their position and scale
		string[] row2 = someLevel.Split('\n')[1].Split(',');
		string[] row3 = someLevel.Split('\n')[2].Split(',');
		string[] row4 = someLevel.Split('\n')[3].Split(',');
		// printArray(row2);
		for(int i=1; i<row2.Length; i++)
		{
			if(row2[i].Length<=1)
				continue;	// this is to exclude empty cells
			// print("debug: "+row2[i]);
			string objName = row2[i].Split(new string[] { "->" }, StringSplitOptions.None)[0].Trim();
			string imgURL  = row2[i].Split(new string[] { "->" }, StringSplitOptions.None)[1].Trim();
			string positionx = row3[i].Split(' ')[0];
			string positiony = row3[i].Split(' ')[1];
			string scale = row4[i];
			//myCustomLevel.allObjects.Add(new Name_url_combo(objName, 
				//imgURL, new Vector2(string2float(positionx), string2float(positiony)), string2float(scale)));
			// myCustomLevel.allObjects.Add(new Name_url_combo(objName,
			// 	imgURL, getEqualizedPos(i, row2.Length-1), string2float(scale), new GameObject()));
			myCustomLevel.allObjects.Add(new Name_url_combo(objName,
				imgURL, getTransformedPos(string2float(positionx), string2float(positiony)), string2float(scale), new GameObject()));
		}

		// put every object onto scene - DONE
		putObjectsOnScene(myCustomLevel);

		// extract every object's detail
		int numObjects = (someLevel.Split('\n').Length - 4)/7;
		//print("Number of objects found - " + numObjects);

		/*
		Object 1 - basketball			
		correct objects	Basketball_boy_img_1	basketball_boy_img_2	
		initial dialogues	Can you help me find the boy with the basketball?	Who is playing with the basketball?	
		ending dialogues	Yay! you found the boy	Hurray !!	
		idle scaffolding	<audio>		
		on character tap	basketball		
		wrong hints	Boy playing basketball	<maybe audio>	Here, let me help you
		*/
		for(int i=0; i<numObjects; i++)
		{
			// indexing -> 3 + i*local_row_num
			int localRowNum = 3 + 7*i;
			CustomObject myCustomObject = new CustomObject();
			myCustomObject.correctObjects = new List<GameObject>();
			myCustomObject.initialDialogues = new List<string>();
			myCustomObject.endDialogues = new List<string>();
			myCustomObject.wrongHints = new List<string>();

			// 1st row -> which object
			Debug.Log("indexing - " + someLevel.Split('\n')[localRowNum+1].Split(',')[0]);

			// 2nd row -> all correct objects
			string[] tempRow = someLevel.Split('\n')[localRowNum+2].Split(',');
			//Debug.Log("indexing all correct objects");
			for(int j=1; j<tempRow.Length; j++)
			{
				if(string.Compare(tempRow[j],"")==0)
					break;
				//print("finding - "+tempRow[j]);
				var cfr = findGO(tempRow[j], myCustomLevel.allObjects);
				if(cfr!=null)
				{
					myCustomObject.correctObjects.Add(cfr);
				}
			}

			// 3rd row -> initial dialogues
			tempRow = someLevel.Split('\n')[localRowNum+3].Split(',');
			//Debug.Log("indexing correct dialogues");
			for(int j=1; j<tempRow.Length; j++)
			{
				if(string.Compare(tempRow[j],"")==0)
					break;
				myCustomObject.initialDialogues.Add(tempRow[j]);
			}

			// 4th row -> ending dialogues
			tempRow = someLevel.Split('\n')[localRowNum+4].Split(',');
			//Debug.Log("indexing ending dialogues");
			for(int j=1; j<tempRow.Length; j++)
			{
				if(string.Compare(tempRow[j],"")==0)
					break;
				myCustomObject.endDialogues.Add(tempRow[j]);
			}

			// 5th row -> idle scaffolding audioclip
			tempRow = someLevel.Split('\n')[localRowNum+5].Split(',');
			//Debug.Log("indexing idle scaffolding audioclip");
			myCustomObject.idleScaffoldingAudioClip = tempRow[1];

			// 6th row -> on character tap sentence
			tempRow = someLevel.Split('\n')[localRowNum+6].Split(',');
			//Debug.Log("indexing character tap sentence");
			myCustomObject.onCharacterTapSentence = tempRow[1];

			// 7th row -> 3 wrong hints
			tempRow = someLevel.Split('\n')[localRowNum+7].Split(',');
			//Debug.Log("indexing wrong hints");
			for(int j=1; j<tempRow.Length; j++)
			{
				if(string.Compare(tempRow[j],"")==0)
					break;
				myCustomObject.wrongHints.Add(tempRow[j]);
			}

			myCustomLevel.customObjects.Add(myCustomObject);
		}

		return myCustomLevel;
	}

	private Vector2 getEqualizedPos(int i, int n)
	{
		return new Vector2(Remap(i, 1, n, -5f, +5f), 0);
	}

	private Vector2 getTransformedPos(float x, float y)
	{
		print(x+","+y);
		// transform on X-axis: (0,1920) to (-5.5,7.5)
		// transform on Y-axis: (0,1080) to (-3.5,3.5)
		float newX = Remap(x, 0f, 1920f, -6.70f, 8.40f);
		float newY = Remap(y, 0f, 1080f, -4.47f, 4.47f);
		return new Vector2(newX, newY);
	}

	public float Remap (float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	private int string2int(string s)
	{
		return int.Parse(s);
	}

	private float string2float(string s)
	{
		return Convert.ToSingle(s);
	}

	private void printArray(string[] arr)
	{
		string res = "";
		foreach(string x in arr)
			res += ("!"+x+"!,");
		print("array found - "+res+".");
	}

	private string removeInitialTemps(string raw)
	{
		string[] tempArr = raw.Split('\n');
		string[] newTempArr = new string[raw.Split('\n').Length-1];
		for(int i=1; i<raw.Split('\n').Length; i++)
		{
			newTempArr[i-1] = tempArr[i];
		}
		return string.Join("\n", newTempArr);
	}

	private GameObject findGO(string name, List<Name_url_combo> allObjects)
	{
		if(name.Length==0)
			return null;
		foreach(var x in allObjects)
		{
			if(string.Compare(name, x.name)==0)
				return x.gameobj;
		}
		return null;
	}

	private void Get(string url, Action<string> onError, Action<string> onSuccess)
	{
		StartCoroutine(GetCoroutine(url, onError, onSuccess));
	}

	private IEnumerator GetCoroutine(string url, Action<string> onError, Action<string> onSuccess)
	{
		using(UnityWebRequest uwr = UnityWebRequest.Get(url))
		{
			yield return uwr.SendWebRequest();

			if(uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
			{
				onError(uwr.error);
			} else {
				onSuccess(uwr.downloadHandler.text);
			}
		}
	}

	private void putBGImage(EG_ISPY_CustomLevel myCustomLevel)
	{
		//print("loading background ...");
		StartCoroutine(DownloadImage("background", myCustomLevel.backgroundImageURL, null, (int status) => { },
			(Texture2D t) => {
				bgImage.sprite= Sprite.Create(t, new Rect(0.0f, 0.0f, t.width, t.height), new Vector2(0f, 0f));
			}
		));
	}

	private void putObjectsOnScene(EG_ISPY_CustomLevel myCustomLevel)
	{
		foreach(var combo in myCustomLevel.allObjects)
		{
			var gameObject = combo.gameobj;
			var spriteRenderer = gameObject.AddComponent<SpriteRenderer> ();
			StartCoroutine(DownloadImage(combo.name, combo.url, spriteRenderer, (int status) => {
				if(status==1) {
					// set position and scale
					gameObject.transform.position = new Vector3(combo.position.x, combo.position.y, 0f);
					gameObject.transform.localScale = new Vector3(combo.scale, combo.scale, 1f);
					// attach it to its parent
					gameObject.transform.parent = allObjectsParentTransform;
					// change its name
					gameObject.name = combo.name;
				}
			}, (Texture2D t) => {
				combo.textureWidth = t.width;
				combo.textureHeight = t.height;
			}));
		}
	}

	IEnumerator DownloadImage(string name, string MediaUrl, SpriteRenderer spR, Action<int> callback, Action<Texture2D> retTex)
	{   
	    UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
	    yield return request.SendWebRequest();
	    if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
	        Debug.Log(request.error + " - " + MediaUrl);
	    else {
	    	var myTexture2D = ((DownloadHandlerTexture) request.downloadHandler).texture;
	    	if(spR!=null)
	        	spR.sprite = Sprite.Create(myTexture2D, new Rect(0.0f, 0.0f, myTexture2D.width, myTexture2D.height), new Vector2(0f, 0f), 100, 1, SpriteMeshType.FullRect);
	        callback(1);
	        retTex(myTexture2D);
	    }
	}
}
