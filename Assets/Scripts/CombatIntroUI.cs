using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatIntroUI : MonoBehaviour
{
    public static CombatIntroUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text combatText;
    [SerializeField] private Image vignette;

    [Header("Text Animation")]
    [SerializeField] private float textDuration = 1.2f;
    [SerializeField] private float textStartY = -100f;
    [SerializeField] private float textStartScale = 0.6f;

    [Header("Vignette")]
    [SerializeField] private float vignetteAlpha = 0.55f;
    [SerializeField] private float vignetteFadeDuration = 0.5f;

    [Header("Camera Zoom")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float zoomAmount = 1.0f;
    [SerializeField] private float zoomDuration = 0.4f;

    private RectTransform textRect;
    private Vector2 originalTextPosition;
    private Vector3 originalTextScale;

    private float originalCameraSize;
    private Coroutine textRoutine;
    private Coroutine vignetteRoutine;
    private Coroutine cameraRoutine;

    private bool combatActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (combatText != null)
        {
            textRect = combatText.GetComponent<RectTransform>();
            originalTextPosition = textRect.anchoredPosition;
            originalTextScale = textRect.localScale;
        }

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null && targetCamera.orthographic)
            originalCameraSize = targetCamera.orthographicSize;

        if (combatText != null)
            combatText.alpha = 0f;

        SetVignetteAlpha(0f);
    }

    public void StartCombatEffect()
    {
        if (combatActive)
            return;

        combatActive = true;

        if (textRoutine != null)
            StopCoroutine(textRoutine);

        textRoutine = StartCoroutine(AnimateCombatText());

        SetVignette(vignetteAlpha);

        if (targetCamera != null && targetCamera.orthographic)
        {
            if (cameraRoutine != null)
                StopCoroutine(cameraRoutine);

            cameraRoutine = StartCoroutine(
                AnimateCamera(originalCameraSize - zoomAmount)
            );
        }
    }

    public void EndCombatEffect()
    {
        combatActive = false;

        if (vignetteRoutine != null)
            StopCoroutine(vignetteRoutine);

        vignetteRoutine = StartCoroutine(
            FadeVignette(0f)
        );

        if (cameraRoutine != null)
            StopCoroutine(cameraRoutine);

        if (targetCamera != null && targetCamera.orthographic)
        {
            cameraRoutine = StartCoroutine(
                AnimateCamera(originalCameraSize)
            );
        }
    }

    private IEnumerator AnimateCombatText()
    {
        if (combatText == null || textRect == null)
            yield break;

        combatText.text = "COMBAT";

        Vector2 startPosition =
            originalTextPosition + Vector2.up * textStartY;

        textRect.anchoredPosition = startPosition;
        textRect.localScale =
            originalTextScale * textStartScale;

        combatText.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < textDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / textDuration);

            // Ease out for a dramatic arrival.
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            textRect.anchoredPosition = Vector2.Lerp(
                startPosition,
                originalTextPosition,
                eased
            );

            textRect.localScale = Vector3.Lerp(
                originalTextScale * textStartScale,
                originalTextScale * 1.1f,
                eased
            );

            combatText.alpha = Mathf.Clamp01(t * 4f);

            yield return null;
        }

        textRect.anchoredPosition = originalTextPosition;
        textRect.localScale = originalTextScale;
        combatText.alpha = 1f;

        yield return new WaitForSeconds(0.6f);

        // Fade the title away; keep the vignette while in combat.
        float fadeTime = 0.35f;
        elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            combatText.alpha = 1f - Mathf.Clamp01(elapsed / fadeTime);
            yield return null;
        }

        combatText.alpha = 0f;
        textRect.anchoredPosition = originalTextPosition;
        textRect.localScale = originalTextScale;

        textRoutine = null;
    }

    private void SetVignette(float targetAlpha)
    {
        if (vignetteRoutine != null)
            StopCoroutine(vignetteRoutine);

        vignetteRoutine = StartCoroutine(
            FadeVignette(targetAlpha)
        );
    }

    private IEnumerator FadeVignette(float targetAlpha)
    {
        if (vignette == null)
            yield break;

        float startAlpha = vignette.color.a;
        float elapsed = 0f;

        while (elapsed < vignetteFadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / vignetteFadeDuration
            );

            SetVignetteAlpha(
                Mathf.Lerp(startAlpha, targetAlpha, t)
            );

            yield return null;
        }

        SetVignetteAlpha(targetAlpha);
        vignetteRoutine = null;
    }

    private void SetVignetteAlpha(float alpha)
    {
        if (vignette == null)
            return;

        Color color = vignette.color;
        color.a = alpha;
        vignette.color = color;
    }

    private IEnumerator AnimateCamera(float targetSize)
    {
        if (targetCamera == null || !targetCamera.orthographic)
            yield break;

        float startSize = targetCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / zoomDuration);
            float eased = t * t * (3f - 2f * t);

            targetCamera.orthographicSize = Mathf.Lerp(
                startSize,
                targetSize,
                eased
            );

            yield return null;
        }

        targetCamera.orthographicSize = targetSize;
        cameraRoutine = null;
    }
}
