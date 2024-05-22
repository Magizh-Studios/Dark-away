using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueItemSO dialogueItem;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CharacterController controller))
        {
            DialogUiHandler.Instance.ShowDialogue(this.dialogueItem);
            Destroy(gameObject);
        }
    }
}
