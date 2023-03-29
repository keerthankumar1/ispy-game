using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EG_ISPY_BackgroundMusic : MonoBehaviour
{
	// variable to keep track of whether to play music or not
	public bool playBGmusic;
	public bool playSFX;

	// variable to reference audio source
	public AudioSource source1, source2;
	public AudioClip buttonPress;

	public GameObject square1, circle1, square2, circle2;
	public Vector3 moveDist;

	public Color ON, OFF;

	/*
	Attribution for background music

	Move Forward by Kevin MacLeod | https://incompetech.com/
	Music promoted by https://www.chosic.com/free-music/all/
	Creative Commons CC BY 3.0
	https://creativecommons.org/licenses/by/3.0/

	Attribution for icons

	<a href="https://www.flaticon.com/free-icons/musical-note" title="musical note icons">Musical note icons created by Freepik - Flaticon</a>
	*/

	private void Start()
	{
		playBGmusic = (PlayerPrefs.GetInt("play_music")==1);
		playSFX = (PlayerPrefs.GetInt("play_sfx")==1);

		if(playBGmusic) {
			source1.Play(0);
			square1.GetComponent<Image>().color = ON;
		} else {
			square1.GetComponent<Image>().color = OFF;
			circle1.transform.DOLocalMove(new Vector3((playBGmusic?+1:-1)*moveDist.x, moveDist.y, moveDist.z), 0.2f);
		}
		if(playSFX) {
			square2.GetComponent<Image>().color = ON;
		} else {
			square2.GetComponent<Image>().color = OFF;
			circle2.transform.DOLocalMove(new Vector3((playSFX?+1:-1)*moveDist.x, moveDist.y, moveDist.z), 0.2f);
		}
	}

	private void Update()
	{
		if(playBGmusic == false)
		{
			source1.Stop();
		}
		else if(playBGmusic && !source1.isPlaying)
		{
			source1.Play(0);
		}
	}

	public void onMusicTogglePress()
	{
		playBGmusic = !playBGmusic;
		PlayerPrefs.SetInt("play_music", playBGmusic?1:0);

		circle1.transform.DOLocalMove(new Vector3((playBGmusic?+1:-1)*moveDist.x, moveDist.y, moveDist.z), 0.2f);
		
		if(square1.GetComponent<Image>().color == ON)
		{
			square1.GetComponent<Image>().color = OFF;
		}
		else
		{
			square1.GetComponent<Image>().color = ON;
		}
	}

	public void onSfxTogglePress()
	{
		playSFX = !playSFX;
		PlayerPrefs.SetInt("play_sfx", playSFX?1:0);

		circle2.transform.DOLocalMove(new Vector3((playSFX?+1:-1)*moveDist.x, moveDist.y, moveDist.z), 0.2f);
		
		if(square2.GetComponent<Image>().color == ON)
		{
			square2.GetComponent<Image>().color = OFF;
		}
		else
		{
			square2.GetComponent<Image>().color = ON;
		}
	}

	public void playButtonPressSFX()
	{
		if(!source2.isPlaying)
			source2.PlayOneShot(buttonPress);
	}

	public void playDialogueSound(AudioClip clip)
	{
		source2.PlayOneShot(clip);
	}
}
