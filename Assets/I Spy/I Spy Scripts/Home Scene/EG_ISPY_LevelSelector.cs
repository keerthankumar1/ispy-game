using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class EG_ISPY_LevelSelector : MonoBehaviour
{
    [SerializeField] public Image bgImage;
    [SerializeField] public GameObject mainChar;
    [SerializeField] public GameObject btn1, btn2, btn3, btn4, btn5;
    [SerializeField] public Transform speechBubble;
    [SerializeField] public TMP_Text speechText;
    [SerializeField] public ParticleSystem confetti;

    [HideInInspector] private string[] dialogues = {"Hello there!", "I've lost some of my favourite things", "Have you come to help me find them"};
    [HideInInspector] private int globalSceneIndex;

    [SerializeField] public GameObject settingsPanel;
    [SerializeField] public TMP_Text settingsText;

    [SerializeField] public AudioClip[] audioDialogues;

    public void loadLevel(int sceneIndex)
    {
        globalSceneIndex = sceneIndex;

        bgImage.color = new Color(0.3f, 0.3f, 0.3f);
        mainChar.transform.localScale = Vector3.zero;
        speechBubble.localScale = Vector3.zero;
        speechText.text = " ";
        settingsText.text = "Settings";
        btn1.SetActive(false);
        btn2.SetActive(false);
        btn3.SetActive(false);
        btn4.SetActive(false);
        btn5.SetActive(false);

        // play SFX for button press
        if(GameObject.Find("Background Music").GetComponent<EG_ISPY_BackgroundMusic>().playSFX)
            GameObject.Find("Background Music").GetComponent<EG_ISPY_BackgroundMusic>().playButtonPressSFX();

        Invoke("actualLoadLevel", 1f);
    }

    private void actualLoadLevel()
    {
        // do animations
        // character scales up with sparkles
        confetti.Play();
        mainChar.transform.DOScale(new Vector3(8.5f, 8.5f, 0f), 1f);
        if(dialogues.Length>=1)
        {
            speechBubble.DOScale(new Vector3(50f, 50f, 50f), 1f);
            speechText.text = dialogues[0];
            GameObject.Find("Background Music").GetComponent<EG_ISPY_BackgroundMusic>().playDialogueSound(audioDialogues[0]);
            speechBubble.DOScale(Vector3.zero, 0.5f).SetDelay(2.5f).OnComplete(() => {
                if(dialogues.Length>=2)
                {
                    speechBubble.DOScale(new Vector3(50f, 50f, 50f), 1f);
                    speechText.text = dialogues[1];
                    GameObject.Find("Background Music").GetComponent<EG_ISPY_BackgroundMusic>().playDialogueSound(audioDialogues[1]);
                    speechBubble.DOScale(Vector3.zero, 0.5f).SetDelay(2.5f).OnComplete(() => {
                        if(dialogues.Length>=3)
                        {
                            speechBubble.DOScale(new Vector3(50f, 50f, 50f), 1f);
                            speechText.text = dialogues[2];
                            GameObject.Find("Background Music").GetComponent<EG_ISPY_BackgroundMusic>().playDialogueSound(audioDialogues[2]);
                            speechBubble.DOScale(Vector3.zero, 0.5f).SetDelay(2.5f).OnComplete(() => {
                                confetti.Play();
                                // character scales down with sparkles
                                mainChar.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => {
                                    // load next scene
                                    SceneManager.LoadSceneAsync(globalSceneIndex);
                                });
                            });
                        }
                    });
                }
            });
        }
    }

    private void shakeObjects(Transform tr)
    {
        float duration = 1f, strength = 0.01f;
        tr.DOShakePosition(duration, strength);
        tr.DOShakeRotation(duration, strength);
        tr.DOShakeScale(duration, strength);
    }

    public void exitGame()
    {
        // play SFX for button press
        if(GameObject.Find("Background Music").GetComponent<EG_ISPY_BackgroundMusic>().playSFX)
            GameObject.Find("Background Music").GetComponent<EG_ISPY_BackgroundMusic>().playButtonPressSFX();

        Application.Quit();
    }

    public void onSettingButtonPress()
    {
        if(settingsText.text=="Settings") {
            settingsText.text = "Close";
            settingsPanel.transform.DOLocalMove(new Vector3(-1979,0,0), 0.5f);
        }
        else {
            settingsText.text = "Settings";
            settingsPanel.transform.DOLocalMove(new Vector3(127,0,0), 0.5f);
        }
    }
}
