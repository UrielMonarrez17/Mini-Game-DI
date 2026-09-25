using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController cc;
    [SerializeField] private GameObject player;
    [SerializeField] private Camera cam;
    [SerializeField] private float Sensitivity;
    
    // Nueva variable para controlar la velocidad de giro con el teclado
    [SerializeField] private float turnSpeed = 150f; 
    
    [SerializeField] private float speed, walk, run, crouch;

    private Vector3 crouchScale;

    public bool isMoving, isCrouching, isRunning;

    private float X, Y;

    private void Start()
    {
        speed = walk;
        crouchScale = new Vector3(1, .75f, 1);
        cc = GetComponent<CharacterController>();
        cc.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        // Obtenemos los inputs al inicio para usarlos en rotación y movimiento
        float horizontal = Input.GetAxis("Horizontal"); // Teclas A y D
        float vertical = Input.GetAxis("Vertical");     // Teclas W y S

        #region Camera Limitation Calculator
        //Camera limitation variables
        const float MIN_Y = -60.0f;
        const float MAX_Y = 70.0f;

        // Rotación horizontal: Sumamos tanto el mouse como las teclas A y D
        X += Input.GetAxis("Mouse X") * (Sensitivity * Time.deltaTime);
        X += horizontal * (turnSpeed * Time.deltaTime); // <-- Cambio aquí (Giro con A y D)
        
        Y -= Input.GetAxis("Mouse Y") * (Sensitivity * Time.deltaTime);

        if (Y < MIN_Y)
            Y = MIN_Y;
        else if (Y > MAX_Y)
            Y = MAX_Y;
        #endregion
        
        transform.localRotation = Quaternion.Euler(Y, X, 0.0f);

        // Movimiento: Ahora solo nos movemos hacia adelante y atrás
        Vector3 forward = transform.forward * vertical;

        // Se eliminó el vector "right" para que ya no haga "strafe" a los lados
        cc.SimpleMove(Vector3.Normalize(forward) * speed);

        // Determines if the speed = run or walk
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = run;
            isRunning = true;
        }
        //Crouch
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            isRunning = false;
            speed = crouch;
            player.transform.localScale = crouchScale;
        }
        else
        {
            isRunning = false;
            isCrouching = false;
            speed = walk;
        }
        // Detects if the player is moving.
        // Useful if you want footstep sounds and or other features in your game.
        isMoving = cc.velocity.sqrMagnitude > 0.0f;
    }
}