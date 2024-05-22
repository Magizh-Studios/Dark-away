using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueItemSO dialogueItem;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CharacterController _))
        {
            DialogUiHandler.Instance.ShowDialogue(this.dialogueItem);
            Destroy(gameObject);
        }
    }
}
