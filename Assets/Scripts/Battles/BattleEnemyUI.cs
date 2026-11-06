
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleEnemyUI : MonoBehaviour
{
    [Header("Interfaz")]
    public TMP_Text hpText;
    public Image portrait;

    [Header("Salto de selección")]
    public float jumpHeight = 12f;
    public float jumpDuration = 0.5f;

    [Header("Efecto de daño")]
    public TMP_Text damagePopupPrefab;
    public float shakeStrength = 8f;
    public float shakeDuration = 0.5f;
    public float popupHeight = 80f;
    public float popupRise = 40f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private Coroutine jumpCoroutine;
    private bool hitInProgress;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
            originalPosition = rectTransform.anchoredPosition;
    }

    public void UpdateUI(BattleEnemy enemy, bool selected)
    {
        if (enemy == null || enemy.data == null || enemy.IsDead)
        {
            StopJump();
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        UpdateHealthText(enemy);

        if (portrait != null)
            portrait.sprite = enemy.data.portrait;

        if (!hitInProgress)
        {
            if (selected)
                StartJump();
            else
                StopJump();
        }
    }

    public void UpdateHealthText(BattleEnemy enemy)
    {
        if (enemy == null || enemy.data == null)
            return;

        if (hpText != null)
        {
            hpText.text =
                "HP: " + enemy.currentHP + "/" + enemy.data.maxHP;
        }
    }

    // ====================================
    // SALTO DE SELECCIÓN
    // ====================================

    private void StartJump()
    {
        if (rectTransform == null || jumpCoroutine != null)
            return;

        jumpCoroutine = StartCoroutine(JumpAnimation());
    }

    private void StopJump()
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
        }

        if (rectTransform != null && !hitInProgress)
            rectTransform.anchoredPosition = originalPosition;
    }

    private IEnumerator JumpAnimation()
    {
        while (true)
        {
            float elapsed = 0f;

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;

                float progress = (elapsed / jumpDuration) * Mathf.PI;

                float height = Mathf.Sin(progress) * jumpHeight;

                rectTransform.anchoredPosition =
                    originalPosition + new Vector2(0f, height);

                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // ====================================
    // TEMBLOR Y NÚMERO DE DAÑO
    // ====================================

    public IEnumerator PlayHitEffect(int damage)
    {
        hitInProgress = true;

        StopJump();

        TMP_Text popup = null;

        if (damagePopupPrefab != null)
        {
            popup = Instantiate(
                damagePopupPrefab,
                transform
            );

            popup.text = damage.ToString();

            popup.rectTransform.anchoredPosition =
                new Vector2(0f, popupHeight);
        }

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / shakeDuration;

            // Temblor horizontal.
            float shakeX =
                Mathf.Sin(elapsed * 65f) *
                shakeStrength *
                (1f - progress);

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition =
                    originalPosition + new Vector2(shakeX, 0f);
            }

            // Animación del número de daño.
            if (popup != null)
            {
                popup.rectTransform.anchoredPosition =
                    new Vector2(
                        0f,
                        popupHeight + progress * popupRise
                    );

                Color color = popup.color;
                color.a = 1f - progress;
                popup.color = color;
            }

            yield return null;
        }

        if (rectTransform != null)
            rectTransform.anchoredPosition = originalPosition;

        if (popup != null)
            Destroy(popup.gameObject);

        hitInProgress = false;
    }

    private void OnDisable()
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
        }

        hitInProgress = false;

        if (rectTransform != null)
            rectTransform.anchoredPosition = originalPosition;
    }
}
