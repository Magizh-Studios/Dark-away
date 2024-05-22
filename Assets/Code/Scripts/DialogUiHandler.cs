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
        //StartCoroutine(DisplayDialogueFor(dialogueItem.dialogueText, dialogueItem.duration));
        SetActiveDialogueObject(true);
        SetDialogue(string.Empty);
        DisplayDialogueFor(dialogueItem.dialogueText, dialogueItem.duration);
    }

    //IEnumerator DisplayDialogueFor(string dialogue, float duration)
    //{
    //    SetActiveDialogueObject(true);
    //    SetDialogue(string.Empty);
    //    dialogueHolder.DOText(dialogue, dialogue.Length * typeSpeed).SetEase(Ease.Linear);

    //    yield return new WaitForSeconds(duration);
    //    SetActiveDialogueObject(false);
    //}



    private void DisplayDialogueFor(string dialogue, float duration)
    {
        float textDuration = (dialogue.Length / duration) * typeSpeed;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(dialogueHolder.DOText(dialogue, textDuration).SetEase(Ease.Linear));
        sequence.AppendInterval(duration); // Add delay after text animation completes
        sequence.OnComplete(() => {
            SetActiveDialogueObject(false);
        });
    }

    private void SetActiveDialogueObject(bool active)
    {
        dialogueHolder.gameObject.SetActive(active);
    }

    private void SetDialogue(string text)
    {
        dialogueHolder.text = text;
    }
}
