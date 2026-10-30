using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "Key_01";
    [SerializeField] private bool destroyOnCollect = true;

    [Header("Optional Feedback")]
    [SerializeField] private GameObject collectEffect; // partículas o lo que quieras

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectKey(keyID);
            collected = true;

            if (collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);

            if (destroyOnCollect)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}