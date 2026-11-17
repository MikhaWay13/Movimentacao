/*using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerRB : MonoBehaviour
{
    // Atributos de configuração de velocidade de movimentação
    [SerializeField] private float WalkSpeed = 5.5f;
    [SerializeField] private float RunningSpeed = 9.0f;
    [SerializeField] private float JumpForce = 8.0f;

    // Atributos utilizados para a movimentação do player
    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private float currentMoveSpeed = 0.0f;

    // Flag para indicar se deve realizar pulo na próxima atualização de física (FixedUpdate)
    private bool jumpRequested = false;

    // Atributos para verificação se o player está no chão
    [SerializeField] private float groundCheckExtra = 0.1f;
    private CapsuleCollider capsule;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;


    // Atributos das actions de movimentação e câmera
    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;


    private InputAction lookInput;
    private Camera mainCamera;

    [SerializeField] private float LookSensitivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;
    private float lookAngle = 0.0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    // será utilizada para definir distância do Raycast para verificar
    // se o player está no chão
        capsule = GetComponent<CapsuleCollider>();


        mainCamera = GetComponentInChildren<Camera>();

        // Referências as actions do Input System
        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");
        lookInput = InputSystem.actions.FindAction("Look");

        // omitir cursor da tela
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // definindo a velocidade atual para inicar com a velocidade de andar
        currentMoveSpeed = WalkSpeed;
    }

    private void Update()
    {
        //input é lido todo frame; a física é aplicada no FixedUpdate
        moveVector = moveInput.ReadValue<Vector2>();

        //define a velocidade de andar ou correr dependendo se a ação de correr está ou não ativa
        currentMoveSpeed = runInput.IsPressed() ? RunningSpeed : WalkSpeed;
        
        //verifica se o pulo foi apertado
        if (jumpInput.WasPressedThisFrame())
        {
            jumpRequested = true;
        }


        Vector2 lookVector = lookInput.ReadValue<Vector2>();
        HandleCamera(lookVector);
    }

    private void FixedUpdate()
    {
        //calcula a direção do movimento considerando a posição do player pelo transform
        Vector3 move = (transform.forward * moveVector.y + transform.right * moveVector.x) * currentMoveSpeed;

        // por enquanto, mantém a mesma velocidade na vertical
        float verticalVelocity = rb.linearVelocity.y;

        //verifica se o player está no chão
        float distance = capsule.height / 2f + groundCheckExtra;
        Vector3 origin = transform.TransformPoint(capsule.center);
        isGrounded = Physics.Raycast(origin, Vector3.down, distance, groundMask);
        

        //alterra a velocidade do eixo y se o player estiver pulando e no chão
        if (jumpRequested && isGrounded)
        {
            verticalVelocity = JumpForce;
        }
        jumpRequested = false;

        // aplica a velocidade para movimentar o rigidbody na direção definida pelo input 
        //alterar a linearVelocity tem resposta imediata, sem aceleração nem deslize.
        //a física ainda cuida da gravidade e das colisões, mas qualquer força externa aplicada ao 
        // objeto é sobrescrita
        rb.linearVelocity = new Vector3(move.x, verticalVelocity, move.z);
    }



    private void HandleCamera(Vector2 lookVector)
    {
        lookAngle += -lookVector.y * LookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);

        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookVector.x * LookSensitivity, 0);
    }
}
*/


using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerControllerRB : MonoBehaviour
{
    // Atributos de configuração de velocidade de movimentação
    [SerializeField] private float WalkSpeed = 5.5f;
    [SerializeField] private float RunningSpeed = 9.0f;
    [SerializeField] private float JumpForce = 8.0f;

    // Atributos utilizados para a movimentação do player
    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private float currentMoveSpeed = 0.0f;

    // Flag para indicar se deve realizar pulo na próxima atualização de física (FixedUpdate)
    private bool jumpRequested = false;

    // Atributos para verificação se o player está no chão
    [SerializeField] private float groundCheckExtra = 0.15f;
    private CapsuleCollider capsule;
    [SerializeField] private LayerMask groundMask = ~0;
    private bool isGrounded;

    // Atributos das actions de movimentação e câmera
    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;
    private InputAction lookInput;

    private Camera mainCamera;

    [SerializeField] private float LookSensitivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;
    private float lookAngle = 0.0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false; // Usa apenas a gravidade do SC_PlanetGravity
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        capsule = GetComponent<CapsuleCollider>();
        mainCamera = GetComponentInChildren<Camera>();

        // Referências às actions do New Input System
        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");
        lookInput = InputSystem.actions.FindAction("Look");

        // Omitir cursor da tela
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentMoveSpeed = WalkSpeed;
    }

    private void Update()
    {
        moveVector = moveInput.ReadValue<Vector2>();
        currentMoveSpeed = runInput.IsPressed() ? RunningSpeed : WalkSpeed;

        if (jumpInput.WasPressedThisFrame())
        {
            jumpRequested = true;
        }

        Vector2 lookVector = lookInput.ReadValue<Vector2>();
        HandleCamera(lookVector);
    }

    private void FixedUpdate()
    {
        // Calcula a direção do movimento tangente à superfície do planeta
        Vector3 move = (transform.forward * moveVector.y + transform.right * moveVector.x) * currentMoveSpeed;

        // Verifica se o player está no chão apontando para os pés (-transform.up)
        float distance = (capsule.height / 2f) + groundCheckExtra;
        Vector3 origin = transform.TransformPoint(capsule.center);
        isGrounded = Physics.Raycast(origin, -transform.up, distance, groundMask);

        // Preserva a velocidade radial acumulada pela gravidade do planeta
        float verticalSpeed = Vector3.Dot(rb.linearVelocity, transform.up);

        // Aplica o pulo na direção oposta ao centro do planeta (transform.up)
        if (jumpRequested && isGrounded)
        {
            verticalSpeed = JumpForce;
        }
        jumpRequested = false;

        // Combina o deslocamento na superfície com a velocidade da gravidade/pulo
        rb.linearVelocity = move + (transform.up * verticalSpeed);
    }

    private void HandleCamera(Vector2 lookVector)
    {
        lookAngle += -lookVector.y * LookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);

        if (mainCamera != null)
        {
            mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0f, 0f);
        }

        transform.rotation *= Quaternion.Euler(0f, lookVector.x * LookSensitivity, 0f);
    }
}