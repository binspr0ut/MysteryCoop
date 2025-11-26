using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CalendarSwipeController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Stack References")]
    public RectTransform calendarStack; // parent yg berisi semua bulan
    public float swipeThreshold = 150f;
    public float swipeSpeed = 1.2f;
    public float exitDistance = 1200f; // jarak lembar keluar layar
    public float returnDelay = 0.2f;   // jeda sebelum lembar masuk lagi di bawah
    public float fadeInSpeed = 1.5f;   // kecepatan fade-in

    private bool isAnimating = false;

    [Header("SFX")]
    [SerializeField] private AudioClip swipeCalendarSFX;


    public void OnDrag(PointerEventData eventData)
    {
        // Tidak perlu real-time drag
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        float delta = eventData.position.x - eventData.pressPosition.x;

        // Hanya deteksi swipe yang cukup jauh
        if (Mathf.Abs(delta) > swipeThreshold)
        {
            bool swipeLeft = delta < 0;

            // 🔊 SFX: kalender digeser
            PlaySFX(swipeCalendarSFX);

            StartCoroutine(SwipeAwayTopCalendar(swipeLeft));
        }
    }

    private IEnumerator SwipeAwayTopCalendar(bool swipeLeft)
    {
        if (calendarStack.childCount == 0) yield break;

        isAnimating = true;

        // Ambil lembar paling atas
        RectTransform topMonth = calendarStack.GetChild(calendarStack.childCount - 1).GetComponent<RectTransform>();

        // Tambahkan CanvasGroup untuk fade
        CanvasGroup cg = topMonth.GetComponent<CanvasGroup>();
        if (cg == null) cg = topMonth.gameObject.AddComponent<CanvasGroup>();

        Vector2 startPos = topMonth.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(swipeLeft ? -exitDistance : exitDistance, 0);
        float t = 0;

        // 🔸 Fade-out + geser keluar
        while (t < 1)
        {
            t += Time.deltaTime * swipeSpeed;
            topMonth.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        // 🔸 Reset posisi dan sembunyikan
        topMonth.anchoredPosition = startPos;
        topMonth.gameObject.SetActive(false);

        // 🔸 Geser ke bawah stack (jadi bulan berikutnya terlihat)
        yield return new WaitForSeconds(returnDelay);
        topMonth.SetAsFirstSibling();
        topMonth.gameObject.SetActive(true);
        cg.alpha = 0f; // mulai transparan

        // 🔸 Fade-in kembali (lembar “baru” yang muncul)
        while (cg.alpha < 1f)
        {
            cg.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }

        cg.alpha = 1f;
        isAnimating = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

}
