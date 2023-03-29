using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EG_ISPY_CorrectAnswer : MonoBehaviour
{
	[HideInInspector] public GameObject eveSystem;

	void OnMouseDown()
	{
		if(this.enabled)
		{
			eveSystem = GameObject.Find("EventSystem");
			eveSystem.GetComponent<EG_ISPY_LevelController>().onCorrectObjectTap();
			
			var width = gameObject.GetComponent<SpriteRenderer>().bounds.size.x;
			var height = gameObject.GetComponent<SpriteRenderer>().bounds.size.y;
			eveSystem.GetComponent<EG_ISPY_LevelController>().confetti.transform.position = new Vector3(transform.position.x+width/2f, transform.position.y+height/2f, transform.position.z);
			eveSystem.GetComponent<EG_ISPY_LevelController>().confetti.Play();
		}
	}
}
