/*using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerVR : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private float WalkSpeed = 5.5f;
    [SerializeField] private float RunningSpeed = 9.0f;
    [SerializeField] private float JumpForce = 8.0f;
    [SerializeField] private float Gravity = 20.0f;
    //[SerializeField] private float LookSensitivity = 0.2f;
   // [SerializeField] private float LookAngleLimit = 90f;
   // private float lookAngle = 0.0f;

    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;
    private Camera mainCamera;
   // private InputAction lookInput;
    private float currentMoveSpeed = 0.0f;
    private float verticalVelocity = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = GetComponentInChildren<Camera>();
        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");
       // lookInput = InputSystem.actions.FindAction("Look");
        // omitir cursor da tela
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // definindo a velocidade atual para iniciar com a velocidade de andar
        currentMoveSpeed = WalkSpeed;

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveVector = moveInput.ReadValue<Vector2>();

        // aplica a velocidade de andar ou correr dependendo se a ação de correr está ou não ativa
        currentMoveSpeed = runInput.IsPressed() ? RunningSpeed : WalkSpeed;
        
        HandleMovement(moveVector);

    }



    private void HandleMovement(Vector2 moveVector)
    {
        // No Cardboard, quem gira com a cabeça é a câmera, controlada pelo Tracked Pose Driver. 
        // A direçaõ do movimento vêm da câmera, projetada no plano horizontal. 
        // Usamos o "right" (e não o "forward") porque ele continua horizontal mesmo 
        // quando o usuário olha para baixo ou para cima.
        Vector3 right = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
        Vector3 forward = Vector3.Cross(right, Vector3.up);
        Vector3 moveDirection = (forward * moveVector.y + right * moveVector.x) * currentMoveSpeed;

        if (characterController.isGrounded)
        {
            if (jumpInput.WasPressedThisFrame())
            {
                verticalVelocity = JumpForce;
                
            }
            
            else
            {
                verticalVelocity = -2f;
                
            }
            
        }
            else
            {
                verticalVelocity -= Gravity * Time.deltaTime;
                
            }
            

            moveDirection.y = verticalVelocity;

            characterController.Move(moveDirection*Time.deltaTime);

    }



}
*/



using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerControllerVR : MonoBehaviour
{
    [SerializeField] private float WalkSpeed = 5.5f;
    [SerializeField] private float RunningSpeed = 9.0f;
    [SerializeField] private float JumpForce = 8.0f;

    [Header("Verificação de Chão")]
    [SerializeField] private float groundCheckExtra = 0.15f;
    [SerializeField] private LayerMask groundMask = ~0;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Camera mainCamera;

    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;

    private Vector2 moveVector = Vector2.zero;
    private float currentMoveSpeed = 0.0f;
    private bool jumpRequested = false;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false; // Usa apenas a gravidade radial do SC_PlanetGravity
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        capsule = GetComponent<CapsuleCollider>();
        mainCamera = GetComponentInChildren<Camera>();

        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentMoveSpeed = WalkSpeed;
    }

    void Update()
    {
        moveVector = moveInput != null ? moveInput.ReadValue<Vector2>() : Vector2.zero;

        bool isRunning = runInput != null && runInput.IsPressed();
        currentMoveSpeed = isRunning ? RunningSpeed : WalkSpeed;

        if (jumpInput != null && jumpInput.WasPressedThisFrame())
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        HandleMovement(moveVector);
    }

    private void HandleMovement(Vector2 inputVector)
    {
        // Projeta o "right" da câmera VR no plano tangente à superfície do planeta (transform.up)
        Vector3 right = Vector3.ProjectOnPlane(mainCamera.transform.right, transform.up);
        if (right.sqrMagnitude < 0.001f)
        {
            right = Vector3.ProjectOnPlane(mainCamera.transform.forward, transform.up);
        }
        right.Normalize();

        Vector3 forward = Vector3.Cross(right, transform.up).normalized;
        Vector3 moveDirection = (forward * inputVector.y + right * inputVector.x) * currentMoveSpeed;

        // Verifica se está no chão apontando para os pés do player (-transform.up)
        float distance = (capsule.height / 2f) + groundCheckExtra;
        Vector3 origin = transform.TransformPoint(capsule.center);
        isGrounded = Physics.Raycast(origin, -transform.up, distance, groundMask);

        // Mantém a velocidade radial gerada pela gravidade do planeta
        float verticalVelocity = Vector3.Dot(rb.linearVelocity, transform.up);

        if (jumpRequested && isGrounded)
        {
            verticalVelocity = JumpForce;
        }
        jumpRequested = false;

        // Combina o movimento na superfície com a gravidade/pulo local
        rb.linearVelocity = moveDirection + (transform.up * verticalVelocity);
    }
}