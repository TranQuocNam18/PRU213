using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Compass HUD - hiển thị hướng đến từng talisman còn lại trên thanh compass nằm ngang.
/// 
/// === SETUP TRONG UNITY EDITOR ===
/// 1. Tạo Canvas (Screen Space - Overlay, Sort Order 10)
/// 2. Trong Canvas, tạo Panel con tên "CompassBar" (Anchor: top-center, Width 700, Height 70, Y = -45)
/// 3. Trong CompassBar:
///    a. Image tên "CompassBG" (màu đen, alpha 0.5) - fill toàn bộ CompassBar
///    b. (Tuỳ chọn) Text/Image chỉ điểm trung tâm
/// 4. Tạo Prefab "TalismanMarkerPrefab" gồm:
///    a. RectTransform (Width 40, Height 50)
///    b. Image con tên "Icon" (dùng sprite icon bùa hoặc sprite mặc định)
///    c. TextMeshProUGUI con tên "DistText" (font size 12, căn giữa, màu vàng)
/// 5. Gắn script này vào GameObject trong scene (ví dụ: HUDManager)
/// 6. Kéo các field vào Inspector:
///    - Player: Transform của player
///    - Compass Bar: RectTransform của CompassBar
///    - Talisman Marker Prefab: Prefab vừa tạo
///    - Compass Half Width: 330 (một nửa chiều rộng thanh compass)
/// </summary>
public class TalismanCompass : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform của Player (kéo Player GameObject vào đây)")]
    public Transform player;

    [Tooltip("RectTransform của thanh CompassBar (Panel ngang trên màn hình)")]
    public RectTransform compassBar;

    [Tooltip("Prefab marker talisman (Image + TextMeshPro)")]
    public GameObject talismanMarkerPrefab;

    [Header("Settings")]
    [Tooltip("Một nửa chiều rộng hiển thị compass (pixel). Bằng width/2 của CompassBar")]
    public float compassHalfWidth = 330f;

    [Tooltip("Khoảng cách tối đa hiển thị trên compass (Unity units). Quá xa hơn sẽ không hiện)")]
    public float maxDetectionRange = 200f;

    // Danh sách talisman đang theo dõi
    private List<TalismanRadarEntry> entries = new List<TalismanRadarEntry>();

    private class TalismanRadarEntry
    {
        public Transform talismanTransform;
        public RectTransform markerRect;
        public Image iconImage;
        public TextMeshProUGUI distText;
    }

    void Start()
    {
        // Tự động đăng ký tất cả talisman trong scene
        Talisman[] allTalismans = FindObjectsByType<Talisman>(FindObjectsSortMode.None);

        foreach (Talisman t in allTalismans)
        {
            RegisterTalisman(t.transform);
        }

        // Nếu chưa bắt đầu objective thì ẩn compass
        UpdateCompassVisibility();
    }

    /// <summary>
    /// Đăng ký một talisman mới vào radar (gọi từ Talisman.cs nếu spawn động)
    /// </summary>
    public void RegisterTalisman(Transform talismanTransform)
    {
        if (talismanMarkerPrefab == null || compassBar == null) return;

        GameObject markerGO = Instantiate(talismanMarkerPrefab, compassBar);
        RectTransform markerRect = markerGO.GetComponent<RectTransform>();

        // Tìm Image và Text trong prefab
        Image icon = markerGO.GetComponentInChildren<Image>();
        TextMeshProUGUI distText = markerGO.GetComponentInChildren<TextMeshProUGUI>();

        entries.Add(new TalismanRadarEntry
        {
            talismanTransform = talismanTransform,
            markerRect = markerRect,
            iconImage = icon,
            distText = distText
        });
    }

    void Update()
    {
        if (player == null || compassBar == null) return;

        UpdateCompassVisibility();

        // Hướng player (chỉ trên mặt phẳng XZ)
        Vector3 playerForward = player.forward;
        playerForward.y = 0f;
        if (playerForward == Vector3.zero) return;
        playerForward.Normalize();

        float playerYaw = Mathf.Atan2(playerForward.x, playerForward.z) * Mathf.Rad2Deg;

        List<TalismanRadarEntry> toRemove = new List<TalismanRadarEntry>();

        foreach (var entry in entries)
        {
            // Tilisman đã bị destroy (đã nhặt)
            if (entry.talismanTransform == null)
            {
                if (entry.markerRect != null)
                    Destroy(entry.markerRect.gameObject);
                toRemove.Add(entry);
                continue;
            }

            Vector3 dir = entry.talismanTransform.position - player.position;
            float distance = dir.magnitude;
            dir.y = 0f;

            if (dir == Vector3.zero)
            {
                entry.markerRect.gameObject.SetActive(false);
                continue;
            }
            dir.Normalize();

            // Góc từ hướng bắc đến hướng talisman
            float talismanYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // Chênh lệch góc (relative angle)
            float angleDelta = Mathf.DeltaAngle(playerYaw, talismanYaw);

            // Map sang vị trí X trên compass bar
            // angleDelta = -180..+180: -180 = trái màn, +180 = phải màn
            float normalizedAngle = angleDelta / 180f; // -1..+1
            float posX = normalizedAngle * compassHalfWidth;

            // Chỉ hiện nếu nằm trong tầm compass và trong maxDetectionRange
            bool inView = Mathf.Abs(posX) <= compassHalfWidth && distance <= maxDetectionRange;
            entry.markerRect.gameObject.SetActive(inView);

            if (inView)
            {
                entry.markerRect.anchoredPosition = new Vector2(posX, 0f);

                // Hiển thị khoảng cách
                if (entry.distText != null)
                    entry.distText.text = Mathf.RoundToInt(distance) + "m";

                // Màu sắc theo khoảng cách: gần = vàng sáng, xa = vàng mờ
                if (entry.iconImage != null)
                {
                    float alpha = Mathf.Lerp(1f, 0.4f, distance / maxDetectionRange);
                    entry.iconImage.color = new Color(1f, 0.9f, 0.2f, alpha);
                }
            }
        }

        foreach (var dead in toRemove)
            entries.Remove(dead);
    }

    void UpdateCompassVisibility()
    {
        if (compassBar == null) return;

        bool objectiveActive = ObjectiveManager.Instance != null &&
                               ObjectiveManager.Instance.objectiveStarted &&
                               !ObjectiveManager.Instance.objectiveCompleted;

        compassBar.gameObject.SetActive(objectiveActive);
    }
}
