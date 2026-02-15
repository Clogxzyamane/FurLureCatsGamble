using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TurnBasedHUD : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Text that shows current / max HP, e.g. \"12 / 20\".")]
    public TextMeshProUGUI healthText;

    [Tooltip("Slider representing health. Set Min = 0 and Max = unit max HP")]
    public Slider healthSlider;

    [Header("Status Effects")]
    [Tooltip("Pool of images that will be used to show status effect icons (left-to-right).")]
    public Image[] statusEffectIcons;

    [Header("Player-only (Bullets / Focus)")]
    [Tooltip("Icons representing bullets/focus for the player HUD. Appear left-to-right.")]
    public Image[] bulletIcons;

    /// <summary>
    /// Initialize HUD from unit data. Only sets health on HUD.
    /// </summary>
    public void SetHUD(PlayerUnit unit)
    {
        if (unit == null) return;

        SetMaxHP(unit.maxHP);
        SetHP(unit.currentHP);
    }

    public void SetMaxHP(int maxHP)
    {
        if (healthSlider != null)
            healthSlider.maxValue = Mathf.Max(1, maxHP);
        if (healthText != null && healthSlider != null)
            healthText.text = $"{(int)healthSlider.value} / {maxHP}";
    }

    public void SetHP(int currentHP)
    {
        if (healthSlider != null)
            healthSlider.value = Mathf.Clamp(currentHP, healthSlider.minValue, healthSlider.maxValue);

        if (healthText != null)
        {
            int max = (healthSlider != null) ? (int)healthSlider.maxValue : currentHP;
            healthText.text = $"{currentHP} / {max}";
        }
    }

    /// <summary>
    /// Set status effect icons from a list of sprites.
    /// Icons are filled left-to-right. Unused slots are hidden.
    /// </summary>
    public void SetStatusEffects(IList<Sprite> icons)
    {
        if (statusEffectIcons == null) return;
        int count = (icons != null) ? icons.Count : 0;
        for (int i = 0; i < statusEffectIcons.Length; i++)
        {
            var img = statusEffectIcons[i];
            if (img == null) continue;

            if (i < count && icons[i] != null)
            {
                img.sprite = icons[i];
                img.enabled = true;
                img.color = Color.white;
            }
            else
            {
                img.sprite = null;
                img.enabled = false;
            }
        }
    }

    /// <summary>
    /// Adds a single status effect sprite to the left-most available slot.
    /// Returns true if added; false if no slot available.
    /// </summary>
    public bool AddStatusEffectSprite(Sprite sprite)
    {
        if (statusEffectIcons == null || sprite == null) return false;
        for (int i = 0; i < statusEffectIcons.Length; i++)
        {
            var img = statusEffectIcons[i];
            if (img == null) continue;
            if (!img.enabled)
            {
                img.sprite = sprite;
                img.enabled = true;
                img.color = Color.white;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes a specific status effect sprite (if provided) by clearing the right-most matching slot.
    /// If sprite is null, removes the last visible (right-most) status icon.
    /// Returns true if a slot was cleared.
    /// </summary>
    public bool RemoveStatusEffectSprite(Sprite sprite = null)
    {
        if (statusEffectIcons == null) return false;
        // Try to remove by matching sprite from right to left
        for (int i = statusEffectIcons.Length - 1; i >= 0; i--)
        {
            var img = statusEffectIcons[i];
            if (img == null) continue;
            if (!img.enabled) continue;
            if (sprite != null)
            {
                if (img.sprite == sprite)
                {
                    img.sprite = null;
                    img.enabled = false;
                    return true;
                }
            }
            else
            {
                // remove the right-most visible icon
                img.sprite = null;
                img.enabled = false;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Clear all status icons.
    /// </summary>
    public void ClearStatusEffects()
    {
        if (statusEffectIcons == null) return;
        for (int i = 0; i < statusEffectIcons.Length; i++)
        {
            var img = statusEffectIcons[i];
            if (img == null) continue;
            img.sprite = null;
            img.enabled = false;
        }
    }

    /// <summary>
    /// Update player bullet/focus icons using explicit bullets and maxBullets.
    /// This fills icons left-to-right and dims/hides unused slots.
    /// </summary>
    public void SetBullets(int bullets, int maxBullets)
    {
        if (bulletIcons == null || bulletIcons.Length == 0) return;

        int clampedMax = Mathf.Clamp(maxBullets, 0, bulletIcons.Length);
        int clampedBullets = Mathf.Clamp(bullets, 0, clampedMax);

        for (int i = 0; i < bulletIcons.Length; i++)
        {
            var img = bulletIcons[i];
            if (img == null) continue;

            if (i < clampedMax)
            {
                img.enabled = true;
                img.color = (i < clampedBullets) ? Color.white : new Color(1f, 1f, 1f, 0.25f);
            }
            else
            {
                img.enabled = false;
            }
        }
    }

    /// <summary>
    /// Incrementally adds a bullet icon (fills next left slot). Returns true if changed.
    /// </summary>
    public bool AddBulletVisual(int maxBullets)
    {
        if (bulletIcons == null || bulletIcons.Length == 0) return false;
        int clampedMax = Mathf.Clamp(maxBullets, 0, bulletIcons.Length);

        // count already filled (white) icons among the valid range
        int filled = 0;
        for (int i = 0; i < clampedMax; i++)
        {
            var img = bulletIcons[i];
            if (img == null) continue;
            if (img.enabled && img.color.a > 0.5f) filled++;
        }

        if (filled >= clampedMax) return false; // already full

        var target = bulletIcons[filled];
        if (target == null) return false;
        target.enabled = true;
        target.color = Color.white;
        // ensure icons beyond clampedMax remain disabled
        for (int i = clampedMax; i < bulletIcons.Length; i++)
        {
            if (bulletIcons[i] != null) bulletIcons[i].enabled = false;
        }
        return true;
    }

    /// <summary>
    /// Incrementally removes a bullet icon (clears right-most filled slot). Returns true if changed.
    /// </summary>
    public bool RemoveBulletVisual(int maxBullets)
    {
        if (bulletIcons == null || bulletIcons.Length == 0) return false;
        int clampedMax = Mathf.Clamp(maxBullets, 0, bulletIcons.Length);

        // find right-most filled (white) icon within range and clear it
        for (int i = clampedMax - 1; i >= 0; i--)
        {
            var img = bulletIcons[i];
            if (img == null) continue;
            if (img.enabled && img.color.a > 0.5f)
            {
                // remove it (hide)
                img.sprite = img.sprite; // keep sprite if desired; we hide slot
                img.enabled = false;
                return true;
            }
            // if it's dimmed or enabled, still treat as occupied and clear
            if (img.enabled && img.color.a <= 0.5f)
            {
                img.enabled = false;
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("Clear Status Icons")]
    void EditorClearStatusIcons() => ClearStatusEffects();

    [ContextMenu("Clear Bullets")]
    void EditorClearBullets()
    {
        if (bulletIcons == null) return;
        for (int i = 0; i < bulletIcons.Length; i++)
        {
            if (bulletIcons[i] != null)
                bulletIcons[i].enabled = false;
        }
    }
#endif
}
