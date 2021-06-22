using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private Image healthBarImage = null;


    private void Awake()
    {
        health.ClientOnHealthChange += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        health.ClientOnHealthChange -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth, int maxhealth)
    {
        healthBarImage.fillAmount = (float)currentHealth/maxhealth;
    }
}
