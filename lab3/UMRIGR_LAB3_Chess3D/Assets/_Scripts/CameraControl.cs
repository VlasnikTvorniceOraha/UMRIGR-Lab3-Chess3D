using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class CameraControl : MonoBehaviour
{
    [Header("Camera control parameters")]
    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _fastMovementSpeed = 100f;
    [SerializeField] private float _freeLookSensitivity = 0.5f;
    [SerializeField] private float _zoomSensitivity = 10f;
    [SerializeField] private float _fastZoomSensitivity = 50f;
    [SerializeField] private float _panSensitivity = 0.3f;
    private Rigidbody _rigidbody;
    bool fastMode;
    bool rightMouse;
    Vector3 moveDirection = new Vector3();

    Vector2 look = new Vector2();
    Vector2 move = new Vector2();


    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        fastMode = false;
        rightMouse = false;
    }

    void Update()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        //var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        var movementSpeed = fastMode ? _fastMovementSpeed : this._movementSpeed;


        Vector3 movementVector = move * movementSpeed * Time.deltaTime;
        //moveDirection = moveDirection * 0.5f;

        transform.position = TryToMove(transform.position, movementVector);

        if (rightMouse)
        {
            Look(look);
        }
        
    /*     if (Input.GetKey(KeyCode.W) || (Input.GetKey(KeyCode.Mouse1) && Input.GetKey(KeyCode.Mouse2))) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection += -transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection += -transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
        if (Input.GetKey(KeyCode.Space)) moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) moveDirection += Vector3.down; */


        /* if (Input.GetKey(KeyCode.Mouse1))
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * _freeLookSensitivity;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * _freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        } */

        /* float axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var zoomSensitivity = fastMode ? this._fastZoomSensitivity : this._zoomSensitivity;
            transform.position = TryToMove(transform.position, transform.forward * axis * zoomSensitivity);
        }      */  

    }

    private void Look(Vector2 rotate)
    {
        if (rotate.sqrMagnitude < 0.01)
        {
            return;
        }
            
        float newRotationX = transform.localEulerAngles.y + rotate.x * _freeLookSensitivity * Time.deltaTime;
        float newRotationY = transform.localEulerAngles.x - rotate.y * _freeLookSensitivity * Time.deltaTime;
        transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
    }

    /// <summary>
    /// Checks if the original vector is within parameters after offset translation.
    /// </summary>
    /// <returns>Translated vector if its within parameters, or original vector if not</returns>
    private Vector3 TryToMove(Vector3 position, Vector3 offset)
    {
        offset = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0) * new Vector3(offset.x, 0, offset.y);

        if ((position + offset).x < 35f && (position + offset).x > -35f)
        {
            position.x += offset.x;
        }

        if ((position + offset).z < 35f && (position + offset).z > -35)
        {
            position.z += offset.z;
        }

        if ((position + offset).y < 35f && (position + offset).y > 2f)
        {
            position.y += offset.y;
        }

        return position;
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
           moveDirection += Vector3.up; 
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
           moveDirection -= Vector3.up;  
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
           moveDirection += Vector3.down; 
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
           moveDirection -= Vector3.down;  
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void FastModeSwitch(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
           fastMode = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
           fastMode = false; 
        }
    }

    public void Scroll(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Vector2 scrollValue = context.ReadValue<Vector2>();
            var zoomSensitivity = fastMode ? this._fastZoomSensitivity : this._zoomSensitivity;
            transform.position = TryToMove(transform.position, transform.forward * scrollValue[1] * zoomSensitivity);
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void RightMouse(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
           rightMouse = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
           rightMouse = false; 
        }
    }

}
