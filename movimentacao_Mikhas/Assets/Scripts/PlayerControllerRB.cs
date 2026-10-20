using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerRB : MonoBehaviour
{
    // Atributos de configuração de velocidade de movimentação
    [SerializeField] private float WalkSpeed = 5.5f;
    [SerializeField] private float RunningSpeed = 9.0f;
    [SerializeField] private float JumpForce = 8.0f;

    // Atributos para verificação se o player está no chão
    [SerializeField] private float groundCheckExtra = 0.1f;
    private CapsuleCollider capsule;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;

    // Atributos utilizados para a movimentação do player
    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private float currentMoveSpeed = 0.0f;

    // Flag para indicar se deve realizar pulo na próxima atualização de física (FixedUpdate)
    private bool jumpRequested = false;

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

        // Recupera o CapsuleCollider para o Raycast
        capsule = GetComponent<CapsuleCollider>();

        // CORREÇÃO: Inicializa a referência da câmera que estava faltando!
        mainCamera = GetComponentInChildren<Camera>();

        // Referências as actions do Input System
        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");
        lookInput = InputSystem.actions.FindAction("Look");

        // Omitir cursor da tela
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Definindo a velocidade atual para iniciar com a velocidade de andar
        currentMoveSpeed = WalkSpeed;
    }

    private void Update()
    {
        // Input é lido todo frame; a física é aplicada no FixedUpdate
        moveVector = moveInput.ReadValue<Vector2>();

        // Define a velocidade de andar ou correr
        currentMoveSpeed = runInput.IsPressed() ? RunningSpeed : WalkSpeed;

        // Verifica se o pulo foi apertado
        if (jumpInput.WasPressedThisFrame())
        {
            jumpRequested = true;
        }

        // Aplica a movimentação de câmera
        Vector2 lookVector = lookInput.ReadValue<Vector2>();
        HandleCamera(lookVector);
    }

    private void HandleCamera(Vector2 lookVector)
    {
        lookAngle += -lookVector.y * LookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);

        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookVector.x * LookSensitivity, 0);
    }

    private void FixedUpdate()
    {
        // Calcula a direção do movimento considerando a posição do player pelo transform
        Vector3 move = (transform.forward * moveVector.y + transform.right * moveVector.x) * currentMoveSpeed;

        // Mantém a mesma velocidade na vertical vinda da física por padrão
        float verticalVelocity = rb.linearVelocity.y;

        // CORREÇÃO: Lógica de verificação de chão e pulo que havia sido cortada!
        float distance = capsule.height / 2f + groundCheckExtra;
        Vector3 origin = transform.TransformPoint(capsule.center);
        
        // Executa o Raycast para baixo
        isGrounded = Physics.Raycast(origin, Vector3.down, distance, groundMask);

        // Altera a velocidade do eixo y se o player estiver pulando e no chão
        if (jumpRequested && isGrounded)
        {
            verticalVelocity = JumpForce;
        }
        jumpRequested = false;

        // Aplica a velocidade final no Rigidbody
        rb.linearVelocity = new Vector3(move.x, verticalVelocity, move.z);
    }
}
