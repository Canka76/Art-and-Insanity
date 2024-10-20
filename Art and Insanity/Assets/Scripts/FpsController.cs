using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    [Header("Movement Parameters")] 
    [SerializeField] private float gravity = 30f;
    [SerializeField] float moveSpeed = 3f;

    [Header("Look Parameters")] 
    [SerializeField, Range(1, 10)] private float lookSpeedX = 5f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 5f;
    [SerializeField, Range(1, 100)] private float upperLookLimit = 80f;
    [SerializeField, Range(1, 100)] private float lowerLookLimit = 80f;

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
            
            ApplyFinalMovement();
        }
    }

    void HandleMovementInput()
    {
        currentInput = new Vector2(moveSpeed * Input.GetAxis("Vertical"), moveSpeed * Input.GetAxis("Horizontal"));
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
}
