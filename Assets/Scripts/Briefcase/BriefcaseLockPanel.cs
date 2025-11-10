using UnityEngine;
using TMPro;
using System.Collections;

public class BriefcaseLockPanel : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text[] digitTexts;
    private int[] digits = new int[3];

    private Briefcase owner;
    public string correctCode = "389";

    public void Init(Briefcase briefcase, string code)
    {
        owner = briefcase;
        correctCode = string.IsNullOrEmpty(code) ? "000" : code;

        for (int i = 0; i < digits.Length; i++)
        {
            digits[i] = 0;
            if (digitTexts[i] != null)
                digitTexts[i].text = "0";
        }
    }

    public void PressUp(int index)
    {
        if (index < 0 || index >= digits.Length) return;
        digits[index] = (digits[index] + 1) % 10;
        digitTexts[index].text = digits[index].ToString();
    }

    public void PressDown(int index)
    {
        if (index < 0 || index >= digits.Length) return;
        digits[index] = (digits[index] + 9) % 10;
        digitTexts[index].text = digits[index].ToString();
    }

    public void PressEnter()
    {
        string entered = $"{digits[0]}{digits[1]}{digits[2]}";
        Debug.Log($"[BriefcaseLockPanel] Entered code: {entered}");

        // Kirim ke Briefcase (yang punya NetworkObject)
        owner?.ValidateCodeFromUI(entered);
    }

    public void PressClose()
    {
        owner?.ClosePuzzle();
    }

    public IEnumerator ShakeDigits()
    {
        float dur = 0.3f;
        float t = 0;
        while (t < dur)
        {
            t += Time.deltaTime;
            float offset = Mathf.Sin(t * 50f) * 5f;
            foreach (var text in digitTexts)
                text.rectTransform.anchoredPosition = new Vector2(offset, 0);
            yield return null;
        }

        foreach (var text in digitTexts)
            text.rectTransform.anchoredPosition = Vector2.zero;
    }
}
