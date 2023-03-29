using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EG_ISPY_HelperSceneScript : MonoBehaviour
{
	[SerializeField] public InputField fgURL, bgURL;
	[SerializeField] public Image bgImage;
	[SerializeField] public TMP_Text posText;

	[HideInInspector] private GameObject fgObj, fgObj2;
	[HideInInspector] private Vector3 offset;

	public void onBGloadBtn()
	{
		putBGImage();
	}

	public void onFGloadBtn()
	{
		if(fgObj2!=null)
		{
			Destroy(fgObj2);
			fgObj2 = null;
		}
		putObjImage();
	}

	private void Update()
	{
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		
		if(Input.mouseScrollDelta.y != 0f)
		{
			if(fgObj2==null)
			{
				Collider2D target = Physics2D.OverlapPoint(mousePosition);
				fgObj2 = target.transform.gameObject;
			}
			fgObj2.transform.localScale = new Vector3(fgObj2.transform.localScale.x+Input.mouseScrollDelta.y, fgObj2.transform.localScale.y+Input.mouseScrollDelta.y, fgObj2.transform.localScale.z);
		}

		if(Input.GetMouseButtonDown(0))
		{
			Collider2D target = Physics2D.OverlapPoint(mousePosition);
			if(target)
			{
				fgObj = target.transform.gameObject;
				fgObj2 = fgObj;
				offset = fgObj.transform.position - mousePosition;
			}
		}
		if(fgObj != null)
		{
			Vector3 temp = mousePosition + offset;
			fgObj.transform.position = new Vector3(temp.x, temp.y, 0f);
			posText.text = Remap(fgObj.transform.position.x, -6.70f, 8.40f, 0f, 1920f) + "," + Remap(fgObj.transform.position.y, -4.47f, 4.47f, 0f, 1080f) + " :: " + fgObj.transform.localScale.x*0.00926f;
		}
		if(Input.GetMouseButtonUp(0) && fgObj!=null)
		{
			fgObj = null;
		}
	}

	public float Remap (float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	private void putObjImage()
	{
		var fgObj1 = new GameObject("dummy placeholder");
		fgObj1.transform.parent = GameObject.Find("Canvas").transform;
		var spriteRenderer = fgObj1.AddComponent<SpriteRenderer>();

		StartCoroutine(DownloadImage("dummy", fgURL.text, spriteRenderer, (int status) => {
			if(status==1)
			{
				print("successfully downloaded object image");
				fgObj1.transform.position = new Vector3(0f, 0f, 0f);
				fgObj1.AddComponent<PolygonCollider2D>();
			}
		}, (Texture2D t) => {}));
	}

	private void putBGImage()
	{
		//print("loading background ...");
		StartCoroutine(DownloadImage("background", bgURL.text, null, (int status) => { },
			(Texture2D t) => {
				bgImage.sprite= Sprite.Create(t, new Rect(0.0f, 0.0f, t.width, t.height), new Vector2(0f, 0f));
			}
		));
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
