using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public Image characterImage;
    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI dialogueText;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;
    public float textSpeed = 0.05f;
    public Animator dialogueAnimator;

    private void Start()
    {
        if (Instance == null)
        
            Instance = this;
        }

    public void StartDialogue(DialogueNpc dialogueNpc)
    {
        isDialogueActive = true;
        dialogueAnimator.Play("show");
        lines.Clear();
        foreach (DialogueLine line in dialogueNpc.dialogueLines)
        {
            lines.Enqueue(line);
        }
        DisplaynextDialogueLine();

    }
    public void DisplaynextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }
        DialogueLine line = lines.Dequeue();
        CharacterName.text = line.character.name;
        characterImage.sprite = line.character.CharacterIcon;
        StopAllCoroutines();
        StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        dialogueText.text = "";
        foreach (char letter in line.line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialogueAnimator.Play("hide");
    }


    }

