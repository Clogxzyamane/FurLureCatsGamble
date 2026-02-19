using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChange : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Load the new scene here
            // For example, using UnityEngine.SceneManagement;
            // SceneManager.LoadScene("NewSceneName");
                SceneController.Instance.LoadScene("NewSceneName"); // Replace "NewSceneName" with the actual name of your scene
        }
    }
}
