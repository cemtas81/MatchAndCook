using UnityEngine;

public class RestaurantStarManager : MonoBehaviour
{
    [Header("Star Settings")]
    [SerializeField] private int maxStars = 10;
    [SerializeField] private int successReward = 1;   // Her 2 başarıda çağrılacak
    [SerializeField] private int failurePenalty = 1;  // Başarısızlıkta -1

    private int currentStars = 0;

    public System.Action<int, int> OnStarsChanged;
    public System.Action OnMaxStarsReached;
    public System.Action OnCriticalWarning;

    public int CurrentStars => currentStars;
    public int MaxStars => maxStars;

    void Start()
    {
        currentStars = 0;
        OnStarsChanged?.Invoke(currentStars, maxStars);
    }

    public void OnPizzaSuccess()
    {
        int prev = currentStars;
        currentStars = Mathf.Min(currentStars + successReward, maxStars);
        if (currentStars != prev)
            OnStarsChanged?.Invoke(currentStars, maxStars);

        if (currentStars == maxStars)
            OnMaxStarsReached?.Invoke();
    }

    public void OnPizzaFailed()
    {
        int prev = currentStars;
        currentStars = Mathf.Max(currentStars - failurePenalty, 0);
        if (currentStars != prev)
            OnStarsChanged?.Invoke(currentStars, maxStars);

        if (currentStars <= Mathf.CeilToInt(maxStars * 0.3f))
            OnCriticalWarning?.Invoke();
    }
}