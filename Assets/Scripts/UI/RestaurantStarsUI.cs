using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Yýldýz göstericisi UI - basit ve efektif
/// </summary>
public class RestaurantStarsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] starImages; // 10 yýldýz image'i
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private Image warningPanel; // Kýrmýzý uyarý paneli

    [Header("Star Colors")]
    [SerializeField] private Color activeStarColor = Color.yellow;
    [SerializeField] private Color inactiveStarColor = Color.gray;
    [SerializeField] private Color warningColor = Color.red;

    // References
    private RestaurantStarManager starManager;

    void Start()
    {
        starManager = FindFirstObjectByType<RestaurantStarManager>();

        if (starManager != null)
        {
            starManager.OnStarsChanged += UpdateStarsDisplay;
            starManager.OnCriticalWarning += ShowWarning;
            starManager.OnMaxStarsReached += CelebrateFull;
        }

        // Uyarý panelini gizle
        if (warningPanel != null)
            warningPanel.gameObject.SetActive(false);
    }

    private void UpdateStarsDisplay(int currentStars, int maxStars)
    {
        // Yýldýzlarý güncelle
        for (int i = 0; i < starImages.Length && i < maxStars; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].color = i < currentStars ? activeStarColor : inactiveStarColor;

                // Yýldýz animasyonu
                if (i < currentStars)
                {
                    starImages[i].transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBounce)
                        .OnComplete(() => starImages[i].transform.DOScale(1f, 0.1f));
                }
            }
        }

        // Metin güncelle
        if (starsText != null)
            starsText.text = $"{currentStars}/{maxStars} YILDIZ";
    }

    private void ShowWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.gameObject.SetActive(true);
            warningPanel.color = warningColor;

            // Kýrmýzý titreme efekti
            warningPanel.DOFade(0.8f, 0.3f).SetLoops(6, LoopType.Yoyo)
                .OnComplete(() => warningPanel.gameObject.SetActive(false));

            // Ekraný sallamak için kamera efekti eklenebilir
            Camera.main.transform.DOShakePosition(0.5f, 0.1f);
        }
    }

    private void CelebrateFull()
    {
        // Maksimum yýldýz kutlama efekti
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].transform.DORotate(Vector3.forward * 360f, 1f, RotateMode.FastBeyond360);
                starImages[i].DOColor(Color.gold, 0.5f).SetLoops(4, LoopType.Yoyo);
            }
        }

        if (starsText != null)
        {
            starsText.DOColor(Color.gold, 0.5f).SetLoops(6, LoopType.Yoyo);
            starsText.transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBounce)
                .OnComplete(() => starsText.transform.DOScale(1f, 0.3f));
        }
    }

    void OnDestroy()
    {
        if (starManager != null)
        {
            starManager.OnStarsChanged -= UpdateStarsDisplay;
            starManager.OnCriticalWarning -= ShowWarning;
            starManager.OnMaxStarsReached -= CelebrateFull;
        }
    }
}