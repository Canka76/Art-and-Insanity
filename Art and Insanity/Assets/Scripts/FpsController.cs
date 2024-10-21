using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    private bool canSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && _characterController.isGrounded; 
    

    [Header("Movement Parameters")] 
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float sprintSpeed = 6f;
   

    [Header("Jumping Parameters")] 
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = 30f;

    [Header("Look Parameters")] 
    [SerializeField, Range(1, 10)] private float lookSpeedX = 5f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 5f;
    [SerializeField, Range(1, 100)] private float upperLookLimit = 80f;
    [SerializeField, Range(1, 100)] private float lowerLookLimit = 80f;

    [Header("Functional Options")] 
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;

    [Header("Controls")] 
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    
    private Camera playerCamera;
    private CharacterController _characterController;
    private Vector3 moveDirections;
    private Vector2 currentInput;

   private float rotationX = 0f;
    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        _characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLock();

            if (canJump)
            {
                HandleJump();
            }

            
            ApplyFinalMovement();
        }
    }

    void HandleMovementInput() 
    {
        currentInput = new Vector2((canSprinting ? sprintSpeed : moveSpeed) * Input.GetAxis("Vertical"), (canSprinting ? sprintSpeed : moveSpeed) * Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirections.y;

        moveDirections = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right)* currentInput.y);
        moveDirections.y = moveDirectionY;
    }

    void HandleMouseLock()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX,0,0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X")*lookSpeedX,0);
    }

    void ApplyFinalMovement()
    {
        if (!_characterController.isGrounded)
        {
            moveDirections.y -= gravity * Time.deltaTime;
        }

        _characterController.Move(moveDirections * Time.deltaTime);
    }

    void HandleJump()
    {
        if (ShouldJump)
        {
            moveDirections.y = jumpForce;
        }
    }
}
