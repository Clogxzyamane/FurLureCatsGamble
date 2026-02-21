// ─────────────────────────────────────────────────────────────────────────────
//  WorldToUIFollower.cs
//
//  Attach this to a UI element (e.g. an enemy target button).
//  It converts a world-space Transform position to screen space every frame
//  and moves the UI element to match — keeping buttons locked above enemies
//  regardless of screen size, resolution, or arena layout.
//
//  Setup:
//    1. Attach this script to each enemy target button (Button1, Button2, Button3)
//    2. Assign the matching enemy spawn point Transform to "worldTarget"
//    3. Adjust "offset" Y to push the button above or below the enemy
// ─────────────────────────────────────────────────────────────────────────────

using UnityEngine;

public class WorldToUIFollower : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The world-space Transform to follow (your enemy spawn point).")]
    public Transform worldTarget;

    [Header("Offset (in screen pixels)")]
    [Tooltip("Shift the button relative to the enemy's screen position. " +
             "Negative Y moves it above the enemy in most canvas setups.")]
    public Vector2 offset = new Vector2(0f, -80f);

    [Header("References (auto-found if left empty)")]
    [Tooltip("The Camera used to render the scene. Leave empty to use Camera.main.")]
    public Camera sceneCamera;

    [Tooltip("The Canvas this button belongs to. Leave empty to find automatically.")]
    public Canvas canvas;

    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (sceneCamera == null)
            sceneCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        if (worldTarget == null || sceneCamera == null || canvas == null) return;

        // Convert world position → screen position
        Vector3 screenPos = sceneCamera.WorldToScreenPoint(worldTarget.position);

        // If the target is behind the camera, hide the button
        if (screenPos.z < 0f)
        {
            rectTransform.anchoredPosition = new Vector2(-99999f, -99999f);
            return;
        }

        // Convert screen position → Canvas local position
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sceneCamera,
            out canvasPos
        );

        rectTransform.anchoredPosition = canvasPos + offset;
    }
}
