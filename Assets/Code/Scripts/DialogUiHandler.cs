using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class DialogUiHandler : SingletonBehavior<DialogUiHandler>
{

    [SerializeField] private Text dialogueHolder;
    [SerializeField] private float typeSpeed = 0.10f;

    public void ShowDialogue(DialogueItemSO dialogueItem)
    {
        StartCoroutine(DisplayDialogueFor(dialogueItem.dialogueText, dialogueItem.duration));
    }

    IEnumerator DisplayDialogueFor(string dialogue, float duration)
    {
        dialogueHolder.gameObject.SetActive(true);
        dialogueHolder.text = "";
        dialogueHolder.DOText(dialogue, dialogue.Length * typeSpeed).SetEase(Ease.Linear);

        yield return new WaitForSeconds(duration);
        dialogueHolder.gameObject.SetActive(false);

    }

}
