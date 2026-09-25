using UnityEngine;
using System.Collections; // Necesario para usar Corrutinas

public class TemporaryWallCollision : MonoBehaviour
{
    [SerializeField] private string wallLayerName = "pared"; 
    [SerializeField] private float duration = 2f; // Tiempo en segundos que durará el efecto
    
    private int playerLayer;
    private int wallLayer;
    private bool isEffectActive = false; // Para evitar que el jugador presione E varias veces seguidas

    private void Start()
    {
        playerLayer = gameObject.layer;
        wallLayer = LayerMask.NameToLayer(wallLayerName);

        if (wallLayer == -1)
        {
            Debug.LogError("No existe una layer llamada '" + wallLayerName + "'.");
        }
    }

    private void Update()
    {
        // Si presiona E y el efecto NO está activo actualmente
        if (Input.GetKeyDown(KeyCode.E) && !isEffectActive)
        {
            // Iniciamos la corrutina del temporizador
            StartCoroutine(DisableCollisionTemporarily());
        }
    }

    // Esta es la corrutina que maneja el tiempo
    private IEnumerator DisableCollisionTemporarily()
    {
        isEffectActive = true; // Marcamos que el efecto está en uso

        // 1. Desactivamos las colisiones
        Physics.IgnoreLayerCollision(playerLayer, wallLayer, true);
        Debug.Log("Colisiones con 'pared' DESACTIVADAS por 2 segundos.");

        // 2. Esperamos el tiempo definido (2 segundos)
        yield return new WaitForSeconds(duration);

        // 3. Volvemos a activar las colisiones
        Physics.IgnoreLayerCollision(playerLayer, wallLayer, false);
        Debug.Log("Colisiones con 'pared' restauradas.");
        
        isEffectActive = false; // Liberamos el efecto para que se pueda volver a usar
    }
}