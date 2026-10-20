using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCC : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private float WalkSpeed = 5.5f;
    [SerializeField] private float RunningSpeed = 9.0f;
    [SerializeField] private float JumpForce = 8.0f;
    [SerializeField] private float Gravity = 20.0f;
    [SerializeField] private float LookSensitivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;
    private float lookAngle = 0.0f;

    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;
    private Camera mainCamera;
    private InputAction lookInput;
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
        lookInput = InputSystem.actions.FindAction("Look");
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

        // aplica a movimentação de câmera
        Vector2 lookVector = lookInput.ReadValue<Vector2>();
        HandleCamera(lookVector);
    }

    private void HandleCamera(Vector2 lookVector)
    {
        // atualiza o ângulo da câmera a partir do eixo y lido (para cima ou para baixo)
        // é necessário inverter pois quando vamos para frente com o mouse ou joystick
        // queremos que a rotação da câmera em relação ao eixo x diminua
        lookAngle += -lookVector.y * LookSensitivity;

        // aqui garantimos não permitir uma rotação maior que o limite definido (90 graus)
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);

        // aplica a rotação da câmera para cima ou para baixo
        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);

        // aplica a rotação para esquerda ou direita no objeto do player
        transform.rotation *= Quaternion.Euler(0, lookVector.x * LookSensitivity, 0);
    }



    private void HandleMovement(Vector2 moveVector)
    {
        // calcula o movimento a partir das direções atuais do player - por isso usa o transform
        // e do input lido (esquerda, direita, frente ou trás)
        // já multiplica pela velocidade do player
        Vector3 moveDirection = (transform.forward * moveVector.y + transform.right * moveVector.x) * currentMoveSpeed;

        if (characterController.isGrounded) // verifica se o player está no chão
        {
            if (jumpInput.WasPressedThisFrame()) // verifica se o player pressionou o pulo
                verticalVelocity = JumpForce;
            else
                verticalVelocity = -2f; // mantém o contato com o chão
        }
        else
        {
            // aplica gravidade se o player estiver no ar
            verticalVelocity -= Gravity * Time.deltaTime;
        }

        // altera a velocidade no eixo y
        moveDirection.y = verticalVelocity;

        // aplica a movimentação utilizando o CharacterController
        characterController.Move(moveDirection * Time.deltaTime);
    }



}
