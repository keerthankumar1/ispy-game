using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EG_ISPY_DebugPosHelper : MonoBehaviour
{
	[HideInInspector] private GameObject selectedObject;
	[HideInInspector] private Vector3 offset;

	[SerializeField]  public TMP_Text text;

	private void Update()
	{
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetMouseButtonDown(0))
		{
			Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);
			if (targetObject)
			{
				selectedObject = targetObject.transform.gameObject;
				offset = selectedObject.transform.position - mousePosition;
				text.text = selectedObject.transform.position.x.ToString("0") + "," + selectedObject.transform.position.y.ToString("0");
			}
		}
		if (selectedObject)
		{
			selectedObject.transform.position = mousePosition + offset;
		}
		if (Input.GetMouseButtonUp(0) && selectedObject)
		{
			selectedObject = null;
		}
	}
}
