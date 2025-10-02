using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles cash flow animations when money is earned.
/// Animates coin icon from customer to cash register with particle effects.
/// Simple and effective visual feedback for mobile games.
/// </summary>
public class CashFlowAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private GameObject coinIconPrefab;
    [SerializeField] private Transform cashRegisterPosition;
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private int coinsToSpawn = 5;
    [SerializeField] private float spreadRadius = 50f;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem coinCollectParticles;
    
    [Header("Audio")]
    [SerializeField] private AudioClip coinCollectSound;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// Animate money collection from source to cash register
    /// </summary>
    public void AnimateMoneyCollection(Vector3 sourceWorldPosition, int amount, System.Action onComplete = null)
    {
        if (cashRegisterPosition == null)
        {
            Debug.LogWarning("Cash register position not set for CashFlowAnimator");
            onComplete?.Invoke();
            return;
        }
        
        // Convert world position to screen position if needed
        Vector3 sourcePosition = sourceWorldPosition;
        Vector3 targetPosition = cashRegisterPosition.position;
        
        // Spawn multiple coins for visual effect
        int coinsToAnimate = Mathf.Min(coinsToSpawn, Mathf.Max(1, amount / 10));
        
        for (int i = 0; i < coinsToAnimate; i++)
        {
            AnimateSingleCoin(sourcePosition, targetPosition, i * 0.1f, i == coinsToAnimate - 1 ? onComplete : null);
        }
    }
    
    /// <summary>
    /// Animate a single coin icon
    /// </summary>
    private void AnimateSingleCoin(Vector3 startPos, Vector3 endPos, float delay, System.Action onComplete = null)
    {
        if (coinIconPrefab == null)
        {
            onComplete?.Invoke();
            return;
        }
        
        // Create coin icon
        GameObject coin = Instantiate(coinIconPrefab, startPos, Quaternion.identity, transform);
        
        // Random spread at start
        Vector3 spreadOffset = Random.insideUnitCircle * spreadRadius;
        Vector3 midPoint = startPos + new Vector3(spreadOffset.x, spreadOffset.y, 0);
        
        // Animate along bezier curve
        Sequence coinSequence = DOTween.Sequence();
        coinSequence.AppendInterval(delay);
        
        // Create bezier path animation
        Vector3[] path = new Vector3[] { startPos, midPoint, endPos };
        coinSequence.Append(coin.transform.DOPath(path, animationDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutQuad));
        
        // Add rotation for visual flair
        coinSequence.Join(coin.transform.DORotate(new Vector3(0, 360, 0), animationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear));
        
        // Scale animation
        coinSequence.Join(coin.transform.DOScale(1.2f, animationDuration * 0.5f)
            .SetLoops(2, LoopType.Yoyo));
        
        coinSequence.OnComplete(() => {
            // Play particle effect at destination
            if (coinCollectParticles != null)
            {
                coinCollectParticles.transform.position = endPos;
                coinCollectParticles.Play();
            }
            
            // Play sound effect
            if (audioSource != null && coinCollectSound != null)
            {
                audioSource.PlayOneShot(coinCollectSound);
            }
            
            // Destroy coin
            Destroy(coin);
            
            onComplete?.Invoke();
        });
    }
    
    /// <summary>
    /// Set the cash register position where coins will fly to
    /// </summary>
    public void SetCashRegisterPosition(Transform cashRegister)
    {
        cashRegisterPosition = cashRegister;
    }
}
