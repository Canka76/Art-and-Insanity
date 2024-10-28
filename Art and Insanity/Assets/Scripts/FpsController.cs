using System.Collections;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class FpsController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    private bool isSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && _characterController.isGrounded;

    private bool ShouldCrouch =>
        Input.GetKeyDown(crouchKey) && _characterController.isGrounded && !duringCrouchAnimation;

    [Header("Movement Parameters")] 
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float crouchspeed = 1.5f;
    [SerializeField] float slopeSpeed = 8f;

    [Header("Crouch Parameters")] 
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 0.5f;
    [SerializeField] private float timeToCrouch = 0.5f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0,0.5f,0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0,0,0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("HeadBob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = .1f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0f;
    private float timer;
    
    
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
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canHeadBob = true;
    [SerializeField] private bool willSlideOnSlopes = true;


    [Header("Controls")] 
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    
    private Camera playerCamera;
    private CharacterController _characterController;
    private Vector3 moveDirections;
    private Vector2 currentInput;

     private float rotationX = 0f;
     
     // Slope Sliding Parameters
     private Vector3 hitPointNormal;

     private bool isSliding
     {
         get
         {
             if (_characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down,out RaycastHit slopeHit, 1.5f))
             {
                 hitPointNormal = slopeHit.normal;
                 return Vector3.Angle(hitPointNormal, Vector3.up) > _characterController.slopeLimit;
             }
             else
             {
                 return false;
             }
         }
     }
    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        _characterController = GetComponent<CharacterController>();

        defaultYPos = playerCamera.transform.localPosition.y;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }
    
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

            if (canCrouch)
            {
                HandleCrouch();
            }

            if (canHeadBob)
            {
                HandleHeadBob();
            }
            
            ApplyFinalMovement();
        }
    }

    void HandleMovementInput() 
    {
        currentInput = new Vector2((isCrouching ? crouchspeed : isSprinting ? sprintSpeed : moveSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchspeed : isSprinting ? sprintSpeed : moveSpeed) * Input.GetAxis("Horizontal"));
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

        if (willSlideOnSlopes && isSliding)
        {
            moveDirections += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        _characterController.Move(moveDirections * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        if (!_characterController.isGrounded)
        {
            return;
        }

        if (Mathf.Abs(moveDirections.x) > 0.1f || moveDirections.z > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }
    
    
    void HandleJump()
    {
        if (ShouldJump)
        {
            moveDirections.y = jumpForce;
        }
    }

    void HandleCrouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private IEnumerator CrouchStand()
    {
        duringCrouchAnimation = true;

        float timeElapsed = 0f;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = _characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = _characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            _characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch);
            _characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _characterController.height = targetHeight;
        _characterController.center = targetCenter;

        isCrouching = !isCrouching;
        
        duringCrouchAnimation = false;

        
    }
}
