using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnBasedHUD : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public Slider hpSlider;

    public void SetHUD(PlayerUnit unit)
    {
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
    }

    public void SetHP(int hp)
    {
        hpSlider.value = hp;
        
    }

    void Update()
    {
        hpText.text = hpSlider.value.ToString();
    }
}
