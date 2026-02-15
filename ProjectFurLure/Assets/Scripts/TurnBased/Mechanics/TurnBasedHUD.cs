using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TurnBasedHUD : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Text that shows current HP as a single number.")]
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

        // Do not update healthText here; keep display as current HP only.
    }

    public void SetHP(int currentHP)
    {
        if (healthSlider != null)
            healthSlider.value = Mathf.Clamp(currentHP, healthSlider.minValue, healthSlider.maxValue);

        if (healthText != null)
        {
            // Show only the current health number (no "/max")
            healthText.text = currentHP.ToString();
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

#if UNITY_EDITOR
    [ContextMenu("Clear Status Icons")]
    void EditorClearStatusIcons() => SetStatusEffects(null);

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
