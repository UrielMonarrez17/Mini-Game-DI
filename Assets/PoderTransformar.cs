using UnityEngine;

public class PoderTransformar : MonoBehaviour
{
    public GameObject cuerpoPrimitivas;
    public GameObject cuerpoMixamo;

    private bool transformado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            transformado = !transformado;
            cuerpoPrimitivas.SetActive(!transformado);
            cuerpoMixamo.SetActive(transformado);
        }
    }
}