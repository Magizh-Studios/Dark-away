using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CreditsUi : MonoBehaviour
{
    [SerializeField] private float openDelay = 0.3f, closeDelay = 0.3f;
    [SerializeField] private Ease openEase = Ease.Linear, closeEase = Ease.Linear;
    [SerializeField] private Button closeButton;
    private RectTransform rectTransform;
    [Space]
    [SerializeField] private RectTransform namesRectTransform;
    [SerializeField] private float moveDelay = 0.2f;

    private Tweener tweener;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        closeButton.onClick.AddListener(() =>
        {
            Hide(HideMode.Tween);
            ResetPosition();
            MainMenuUi.Instance.SetActiveMainMenuBtns(true);
        });

        Hide(HideMode.Instant);
    }
    public void Show()
    {
        rectTransform.DOScale(Vector3.one, openDelay).SetEase(openEase);
    }

    public void RollNames()
    {
        ResetPosition();

        tweener?.Kill();

        tweener = namesRectTransform.DOAnchorPosY(750f, moveDelay).OnComplete(()=>
        {
            Hide(HideMode.Tween);
            MainMenuUi.Instance.SetActiveMainMenuBtns(true);
            ResetPosition();
        });
    }

    private void ResetPosition()
    {
        tweener?.Kill();
        namesRectTransform.DOAnchorPosY(-697f, 0.1f);
    }

    public void Hide(HideMode hideMode)
    {
        if (hideMode == HideMode.Tween)
            rectTransform.DOScale(Vector3.zero, closeDelay).SetEase(closeEase);
        else
            rectTransform.localScale = Vector3.zero;
    }
    public enum HideMode { Tween, Instant }
}
