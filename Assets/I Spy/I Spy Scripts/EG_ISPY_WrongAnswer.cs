using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EG_ISPY_WrongAnswer : MonoBehaviour
{
	[HideInInspector] public GameObject eveSystem;

	void OnMouseDown()
	{
		if(this.enabled)
		{
			eveSystem = GameObject.Find("EventSystem");

			// set OUTLINE material
			eveSystem.GetComponent<EG_ISPY_LevelController>().outlineMaterial.SetColor("_OutlineColor", new Color(0.2f, 0f, 0f));
			eveSystem.GetComponent<EG_ISPY_LevelController>().outlineMaterial.SetFloat("_EmissionIntensity", 10f);
			gameObject.GetComponent<Renderer>().material = eveSystem.GetComponent<EG_ISPY_LevelController>().outlineMaterial;

			eveSystem.GetComponent<EG_ISPY_LevelController>().onWrongObjectTap();
		}
	}
}
