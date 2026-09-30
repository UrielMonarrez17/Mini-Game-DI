
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Character")]
    public GameObject characterPrefab;

    [Header("Camera")]
    public GameObject camera;

    [Header("Menu")]
    public GameObject menu;

    [Header("Spawn Position")]
    public Transform spawnPoint;

    private GameObject currentCharacter;

    public void SpawnCharacter()
    {
        camera.SetActive(false);
        menu.SetActive(false);
        if (characterPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning(
                "Assign the character prefab and spawn point!"
            );

            return;
        }

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
        currentCharacter = Instantiate(
            characterPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Camera characterCamera =
            currentCharacter.GetComponentInChildren<Camera>(
                true
            );

        if (characterCamera != null)
        {
            Camera[] allCameras =
                FindObjectsByType<Camera>(
                    FindObjectsSortMode.None
                );

            foreach (Camera cam in allCameras)
            {
                if (cam != characterCamera)
                {
                    cam.gameObject.SetActive(false);
                }
            }

            characterCamera.gameObject.SetActive(true);

            characterCamera.enabled = true;

            AudioListener listener =
                characterCamera.GetComponent<AudioListener>();

            if (listener != null)
            {
                listener.enabled = true;
            }
        }


        Debug.Log("Character spawned!");
    }
}