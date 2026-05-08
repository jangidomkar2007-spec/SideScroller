using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    [Header("Buttons")]
    public Button nextButton;

    [Header("Animation")]
    public Animator animator;

    [Header("Typing")]
    public float typingSpeed = 0.03f;

    private Queue<DialogueLine> lines;

    private Coroutine typingCoroutine;

    public bool isDialogueActive = false;

    private bool isTyping = false;

    private string currentSentence;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        lines = new Queue<DialogueLine>();

        animator.gameObject.SetActive(false);

        nextButton.onClick.AddListener(DisplayNextDialogueLine);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;

        animator.gameObject.SetActive(true);

        animator.Play("show");

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        // Skip typing instantly
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);

            dialogueArea.text = currentSentence;

            isTyping = false;

            return;
        }

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

        characterIcon.sprite = currentLine.character.icon;
        characterName.text = currentLine.character.name;

        currentSentence = currentLine.line;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeSentence(currentLine.line));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;

        dialogueArea.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueArea.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        isDialogueActive = false;

        animator.Play("hide");

        StartCoroutine(DisableDialogueBox());
    }

    IEnumerator DisableDialogueBox()
    {
        yield return new WaitForSeconds(0.3f);

        animator.gameObject.SetActive(false);
    }
}