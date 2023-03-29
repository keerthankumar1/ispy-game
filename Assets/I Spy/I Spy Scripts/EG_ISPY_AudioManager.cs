using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EG_ISPY_AudioManager : MonoBehaviour
{
	[SerializeField] public AudioClip correctObjSound, wrongObjSound, mechScaffoldingSound, idleScaffoldingSound, buttonPressClip;
	[SerializeField] public AudioSource audioSource, bgAudioSource;

	public void playCorrectObjSound()
	{
		audioSource.PlayOneShot(correctObjSound);
	}

	public void playWrongObjSound()
	{
		audioSource.PlayOneShot(wrongObjSound);
	}

	public void playMechScaffoldingSound()
	{
		audioSource.PlayOneShot(mechScaffoldingSound);
	}

	public void playIdleScaffoldingSound()
	{
		audioSource.PlayOneShot(idleScaffoldingSound);
	}

	public void playCustomSound(AudioClip clip)
	{
		audioSource.PlayOneShot(clip);
	}

	public void playButtonSound()
	{
		audioSource.PlayOneShot(buttonPressClip);
	}
}
