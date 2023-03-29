using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EG_ISPY_CustomLevel
{
	// gameobject -> name, image URL
	public List<Name_url_combo> allObjects;
	public string backgroundImageURL;
	public List<CustomObject> customObjects;
}

[System.Serializable]
public class CustomObject
{
	public List<GameObject> correctObjects;
	
	public List<string> initialDialogues;
	public List<string> endDialogues;
	public List<string> wrongHints;

	public string onCharacterTapSentence;
	public string idleScaffoldingAudioClip;
}

[System.Serializable]
public class Name_url_combo
{
	public string name;
	public string url;
	public Vector2 position;
	public float scale;
	public GameObject gameobj;

	public int textureWidth, textureHeight;

	public Name_url_combo(string s1, string s2, Vector2 pos, float s, GameObject g)
	{
		name = s1;
		url = s2;
		position = new Vector2(pos.x, pos.y);
		scale = s;
		gameobj = g;
	}
}