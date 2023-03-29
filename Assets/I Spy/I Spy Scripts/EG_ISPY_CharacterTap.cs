using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EG_ISPY_CharacterTap : MonoBehaviour
{
	[HideInInspector] public GameObject eveSystem;

	void OnMouseDown()
	{
		if(this.enabled)
		{
			eveSystem = GameObject.Find("EventSystem");
			eveSystem.GetComponent<EG_ISPY_LevelController>().onCharacterTap();
		}
	}
}
