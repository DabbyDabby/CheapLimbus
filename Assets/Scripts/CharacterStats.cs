using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Basic Stats")] public int maxHealth = 200;
    public int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Method to apply damage
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"{gameObject.name} is defeated!");
            // Optionally trigger animations, despawn logic, etc.
        }
    }

    // Method for healing
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} healed {amount} HP. Current HP: {currentHealth}");
    }
}