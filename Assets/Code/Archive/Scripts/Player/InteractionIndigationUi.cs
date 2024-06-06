using UnityEngine;
using UnityEngine.UI;

public class InteractionIndigationUi : MonoBehaviour
{
    [SerializeField] private Transform interactionPanel;
    [SerializeField] private Button interactionButton;

    private IInteractable interactable;
    private void Awake()
    {
        interactable = GetComponentInParent<IInteractable>();

        interactionButton.onClick.AddListener(() =>
        {
            interactable.Interact(FindObjectOfType<Player>().transform);
        });

        Hide();
    }

    public void Hide()
    {
        interactionPanel.gameObject.SetActive(false);
    }
    public void Show()
    {
        interactionPanel.gameObject.SetActive(true);
    }
}
