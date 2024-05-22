using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "New Dialog", order = 2)]
public class DialogueItemSO : ScriptableObject
{
    [TextArea]
    public string dialogueText;

    public float duration;
}
