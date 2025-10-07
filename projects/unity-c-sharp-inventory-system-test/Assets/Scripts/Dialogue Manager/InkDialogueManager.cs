using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ink.Parsed;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEditor;

public class InkDialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI promptText;
    public TextMeshProUGUI npcName;
    public bool useTriggerPersistant;
    public InkDialogueTrigger dialogueTrigger;
    public enum DialogueMode { Script, Trigger, Null }
    public DialogueMode dialogueMode;
    public int dialogueNo;
    public int choiceNo;
    private Ink.Runtime.Story currentStory;
    public bool dialogueIsPlaying;
    private bool grabLine;
    public AudioSource speechBlip;
    public GameObject[] choices;
    public TextMeshProUGUI[] choicesText;
    string sentence;
    public int activeChoices;
    public int choiceIndex;
    public bool skipMode;
    public float textSpeedDelay;
    public GameObject choiceSelector;
    private static InkDialogueManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than one instance of dialogue manager found! Please ensure that there is no other dialogue manager present in the scene.");
        }
        grabLine = false;
        dialogueNo = 0;
        useTriggerPersistant = false;
        skipMode = false;
    }
    private void Start()
    {
        dialogueIsPlaying = false;
        grabLine = false;
        choicesText = new TextMeshProUGUI[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            choicesText[i] = choices[i].GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    private void DisplayChoices()
    {
        List<Ink.Runtime.Choice> currentChoices = currentStory.currentChoices;
        if (currentChoices.Count > choices.Length)
        {
            return;
        }
        for (int i = 0; i < currentChoices.Count; i++)
        {
            choicesText[i].text = currentChoices[i].text;
        }
        if (currentChoices.Count != 0)
        {
            promptText.text = "(Arrow keys to navigate, Z to confirm)";
            dialoguePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-225, dialoguePanel.GetComponent<RectTransform>().anchoredPosition.y);
            for (int i = 0; i < currentChoices.Count; i++)
            {
                choices[i].SetActive(true);
            }
            choiceSelector.SetActive(true);
            activeChoices = currentChoices.Count;
        }
        else
        {
            promptText.text = "(Press Z to continue)";
            dialoguePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, dialoguePanel.GetComponent<RectTransform>().anchoredPosition.y);
            for (int i = 0; i < choices.Length; i++)
            {
                choices[i].SetActive(false);
            }
            choiceSelector.SetActive(false);
            activeChoices = 0;
        }
    }
    public void EnterDialogueMode(TextAsset inkJSON, string name, string triggerName, AudioClip audioClip)
    {
        Player.GetInstance().canMove = false;
        currentStory = new Ink.Runtime.Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        grabLine = true;
        npcName.text = name;
        speechBlip.clip = audioClip;
        useTriggerPersistant = false;
        grabLine = true;
        dialogueMode = DialogueMode.Script;
        ContinueStory();
    }
    public void EnterDialogueMode(InkDialogueTrigger dialogueTrigger, TextAsset inkJSON, string name, AudioClip audioClip)
    {
        Player.GetInstance().canMove = false;
        currentStory = new Ink.Runtime.Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        grabLine = true;
        npcName.text = name;
        speechBlip.clip = audioClip;
        useTriggerPersistant = false;
        grabLine = true;
        this.dialogueTrigger = dialogueTrigger;
        dialogueMode = DialogueMode.Trigger;
        ContinueStory();
    }
    public void EnterDialogueMode(InkDialogueTrigger dialogueTrigger, TextAsset inkJSON, string name, AudioClip audioClip, BoxCollider2D trigger)
    {
        Player.GetInstance().canMove = false;
        currentStory = new Ink.Runtime.Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        npcName.text = name;
        speechBlip.clip = audioClip;
        useTriggerPersistant = true;
        trigger.enabled = false;
        grabLine = true;
        this.dialogueTrigger = dialogueTrigger;
        dialogueMode = DialogueMode.Trigger;
        ContinueStory();
    }
    public void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            StopAllCoroutines();
            grabLine = true;
            dialogueNo++;
            sentence = currentStory.currentText;
            StartCoroutine(TypeSentence(sentence));
            GameManager.GetInstance().dialogueNo = dialogueNo;
            DisplayChoices();
        }
        else
        {
            ExitDialogueMode();
        }
    }
    public void ExitDialogueMode()
    {
        if (dialogueMode == DialogueMode.Trigger)
        {
            if (dialogueTrigger.hasTransition)
            {
                GameManager.GetInstance().Transition(choiceNo, dialogueTrigger);
                choiceNo = 0;
                return;
            }
        }
        npcName.text = string.Empty;
        text.text = string.Empty;
        sentence = string.Empty;
        dialogueNo = 0;
        GameManager.GetInstance().OnDialogueExit(choiceNo, dialogueTrigger, npcName.text, dialogueMode);
        choiceNo = 0;
        Player.GetInstance().canMove = true;
        dialoguePanel.SetActive(false);
        if (dialogueMode == DialogueMode.Trigger)
        {
            if (useTriggerPersistant)
            {
                dialogueTrigger.radius.enabled = true;
            }
        }
        dialogueTrigger = null;
        dialogueIsPlaying = false;
        dialogueMode = DialogueMode.Null;
    }
    IEnumerator TypeSentence(string sentence)
    {
        text.text = "";
        foreach (char letter in currentStory.Continue().ToCharArray())
        {
            text.text += letter;
            if (text.text.Length % 2 == 0)
            {
                grabLine = false;
                speechBlip.Play();
                skipMode = true;
            }
            yield return new WaitForSeconds(textSpeedDelay);
        }
        skipMode = false;
    }
    private void Update()
    {
        if (choiceIndex < 0)
        {
            choiceIndex = activeChoices - 1;
        }
        if (choiceIndex > activeChoices - 1)
        {
            choiceIndex = 0;
        }
        choiceSelector.transform.position = choices[choiceIndex].transform.position;
        if (activeChoices == 0)
        {
            promptText.text = "(Press Z to continue)";
            choiceSelector.SetActive(false);
            choiceIndex = 0;
        }
        else
        {
            promptText.text = "(Arrow keys to navigate, Z to confirm)";
            choiceSelector.SetActive(true);
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                choiceIndex--;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                choiceIndex++;
            }
        }
        if (dialogueIsPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Z) && activeChoices == 0 && !grabLine)
            {
                CheckSkipMode();
            }
            if (Input.GetKeyDown(KeyCode.Z) && activeChoices != 0 && !grabLine)
            {
                MakeChoice(choiceIndex);
            }
        }
    }
    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        choiceNo = choiceIndex;
        ContinueStory();
    }
    public void CheckSkipMode()
    {
        if (skipMode)
        {
            StopAllCoroutines();
            text.text = currentStory.currentText;
            skipMode = false;
        }
        else
        {
            StopAllCoroutines();
            ContinueStory();
        }
    }
    public static InkDialogueManager GetInstance()
    {
        return instance;
    }
}
