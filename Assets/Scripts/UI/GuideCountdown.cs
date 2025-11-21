using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GuideCountdown : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private float waitSeconds = 5f;

    [Header("Cutscene Settings")]
    [SerializeField] private string cutsceneSceneName = "IntroCutscene";
    [SerializeField] private string nextSceneName = "FirstFloor";

    private Coroutine countdownRoutine;
    private bool hasPressed = false;

    private void OnEnable()
    {
        hasPressed = false;

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownRoutine = StartCoroutine(RunCountdown());
    }

    private void OnDisable()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
    }

    private IEnumerator RunCountdown()
    {
        if (continueButton != null)
            continueButton.interactable = false;

        float timeLeft = waitSeconds;

        while (timeLeft > 0f)
        {
            if (infoText != null)
            {
                int t = Mathf.CeilToInt(timeLeft);
                infoText.text = $"Please read the guide ({t}s)";
            }

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        if (continueButton != null)
            continueButton.interactable = true;

        if (infoText != null)
            infoText.text = "Tap to continue";
    }

    // dipanggil dari OnClick tombol Continue
    public void OnClickContinue()
    {
        if (hasPressed) return;
        hasPressed = true;

        if (continueButton != null)
            continueButton.interactable = false;

        if (infoText != null)
            infoText.text = "Waiting for other player...";

        if (SceneFlowManager.Instance == null)
        {
            Debug.LogWarning("[GuideCountdown] SceneFlowManager not found.");
            return;
        }

        // lapor ke server bahwa pemain ini sudah siap
        SceneFlowManager.Instance.ReportGuideContinueServerRpc(
            cutsceneSceneName,
            nextSceneName
        );
    }
}