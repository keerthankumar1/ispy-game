using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EG_ISPY_ScaffHelper : MonoBehaviour
{
	[HideInInspector] public float refTime, currTime;

	[HideInInspector] public bool startTimer;
	[Range(5f,50f)][SerializeField] public float idleScaffTime;
	[Range(5f,50f)][SerializeField] public float mechScaffTime;

	[SerializeField] public float timer;

	[HideInInspector] public EG_ISPY_LevelController levelController;

	private void Start()
	{
		startTimer = false;

		var obj = GameObject.Find("EventSystem");
		levelController = obj.GetComponent<EG_ISPY_LevelController>();
	}

	private void Update()
	{
		if(startTimer)
		{
			timer = currTime-refTime;
			
			// keep updating currTime constantly & if it exceeds idleScaffTime from refTime, call OnIdleScaffActivate
			currTime = Time.time;
			if(currTime-refTime > idleScaffTime && !levelController.isMechScaffEnabled)
			{
				levelController.onIdleScaffActivate();
			}
			if(currTime-refTime > mechScaffTime && levelController.isMechScaffEnabled)
			{
				levelController.onMechScaffActivate();
			}
		}
	}

	// reset timer whenever any interaction is registered
	public void ResetTimer()
	{
		refTime = Time.time;
		currTime = refTime;
	}

	// start timer at start of scene
	public void StartTimer()
	{
		ResetTimer();
		startTimer = true;
	}

	// stop timer when correct answer is tapped
	public void StopTimer()
	{
		startTimer = false;
	}
}
