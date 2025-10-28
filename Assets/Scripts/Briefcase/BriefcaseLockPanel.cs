using UnityEngine;
using TMPro;

public class BriefcaseLockPanel : MonoBehaviour
{
    public TMP_Text[] digitTexts;   // isi 4 elemen untuk tampilan digit
    private int[] digits = new int[4]; 

    private Briefcase owner;
    private string correctCode = "000";

    public void Init(Briefcase briefcase, string code)
    {
        owner = briefcase;
        correctCode = string.IsNullOrEmpty(code) ? "000" : code;

        // reset semua digit ke 0
        for (int i = 0; i < digits.Length; i++)
        {
            digits[i] = 0;
            if (digitTexts != null && i < digitTexts.Length && digitTexts[i] != null)
                digitTexts[i].text = "0";
        }
    }

    // dipanggil dari tombol ↑ (set param index di Inspector: 0..3)
    public void PressUp(int index)
    {
        if (index < 0 || index >= digits.Length) return;
        digits[index] = (digits[index] + 1) % 10;
        if (digitTexts != null && index < digitTexts.Length && digitTexts[index] != null)
            digitTexts[index].text = digits[index].ToString();
    }

    // dipanggil dari tombol ↓ (set param index di Inspector: 0..3)
    public void PressDown(int index)
    {
        if (index < 0 || index >= digits.Length) return;
        digits[index] = (digits[index] + 9) % 10; // turun 1 (wrap)
        if (digitTexts != null && index < digitTexts.Length && digitTexts[index] != null)
            digitTexts[index].text = digits[index].ToString();
    }

    // tombol Enter
    public void PressEnter()
    {
        string input = $"{digits[0]}{digits[1]}{digits[2]}";
        if (input == correctCode)
        {
            owner?.OnUnlocked();
        }
        else
        {
            // salah → reset ke 0 (sederhana; nanti bisa ditambah shake/SFX)
            Init(owner, correctCode);
        }
    }

    // tombol Close (batalkan)
    public void PressClose()
    {
        owner?.ClosePuzzle();
    }
}