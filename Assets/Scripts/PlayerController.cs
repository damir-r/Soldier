using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject arrow;
    public GameObject aimArrow;
    public FixedJoystick joystick;
    public FixedJoystick RotationJoystick;
    private NavMeshAgent agent;
    private InputAction moveAction;
    private InputAction moveAction2;
    private Vector3 targetPoint;
    private Vector3 movementDirection;
    private bool isGrounded;
    private LayerMask floorMask;
    private Transform cameraDock;
    private List<GameObject> fadedOutObjects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadedOutObjects = new List<GameObject>();
        cameraDock = GameObject.Find("CameraDock")?.transform;
        floorMask = LayerMask.GetMask("Floor");
        movementDirection = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogWarning("NavMeshAgent could not be found on the current gameObject. The player may be able to phase through walls and other objects using Click-To-Move.");
        if (mainCamera == null)
            mainCamera = Camera.main;
        mainCamera.transform.position = transform.position;
        moveAction = InputSystem.actions.FindAction("Attack", true);
        moveAction2 = InputSystem.actions.FindAction("Move", true);
        if (Application.isEditor)
        {
            moveAction.performed += context =>
            {
                Vector2 pos = new();
                if (Touchscreen.current == null)
                    pos = Mouse.current.position.ReadValue();
                else
                    pos = Touchscreen.current.primaryTouch.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(pos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
                {
                    targetPoint = hit.point;
                    if (agent == null)
                        movementDirection = (targetPoint - transform.position).normalized;
                }
                else
                {
                    targetPoint = Vector3.zero;
                }
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPoint.magnitude > 0)
        {
            if (agent == null)
            {
                transform.position = Vector3.Lerp(transform.position, targetPoint, Time.deltaTime * 2.0f);
            } else
            {
                agent.isStopped = false;
                agent.destination = targetPoint;
                movementDirection = transform.forward;
            }
        }

        if (isGrounded && Vector3.Distance(transform.position, targetPoint) < (transform.localScale.y + 0.5))
        {
            targetPoint = Vector3.zero;
            agent.isStopped = true;
        }
        arrow.transform.position = targetPoint + new Vector3(0, 1 + Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * 4)));
        arrow.transform.eulerAngles += new Vector3(0, Time.deltaTime * 90f);
        Vector2 wasdVec = moveAction2.ReadValue<Vector2>();
        Vector2 joyVec = joystick.Direction;
        Vector2 moveVec = wasdVec;
        if (joyVec.magnitude > 0)
            moveVec = joyVec;
        if (moveVec.magnitude > 0)
        {
            targetPoint = Vector3.zero;
            agent.isStopped = true;
            movementDirection = mainCamera.transform.rotation * new Vector3(moveVec.x, 0, moveVec.y);
            movementDirection = new Vector3(movementDirection.x, 0, movementDirection.z);
            Vector3 targetPosition = transform.position + 5f * Time.deltaTime * movementDirection;
            Ray GroundedRay = new Ray(targetPosition, -transform.up);
            if (Physics.Raycast(GroundedRay))
                transform.position = targetPosition;
        }
        aimArrow.transform.position = transform.position - new Vector3(0, transform.localScale.y) + transform.forward * 1.3f;
        aimArrow.transform.rotation = Quaternion.LookRotation(transform.forward, transform.right);
        aimArrow.transform.eulerAngles = new Vector3(90, aimArrow.transform.eulerAngles.y + 180, 90);
        Vector2 rot_dir = RotationJoystick.Direction;
        if (rot_dir.magnitude > 0)
        {
            targetPoint = Vector3.zero;
            agent.isStopped = true;
            transform.eulerAngles += 60f * Time.deltaTime * new Vector3(0, rot_dir.x, 0);
        }
    }

    public void Attack()
    {
        Ray hitRay = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(hitRay, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Enemy"))
                Destroy(hit.transform.gameObject);
        }
        targetPoint = Vector3.zero;
        agent.isStopped = true;
    }

    private struct DistanceRecord
    {
        public float distance;
        public Transform transform;
        public GameObject gameObject;
    }

    void FixedUpdate()
    {
        DistanceRecord record = new();
        record.distance = Mathf.Infinity;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist = Vector3.Distance(enemy.transform.position, transform.position);
            if (dist > record.distance)
                continue;
            record.distance = dist;
            record.transform = enemy.transform;
            record.gameObject = enemy;
        }
        if (record.transform != null && record.distance < 1)
            SceneManager.LoadScene("MainMenu");

        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        isGrounded = Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask);
        if (isGrounded)
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + transform.localScale.y;
            transform.position = pos;
        }

        if (cameraDock == null)
        {
            Vector3 targetCameraPosition = transform.position + (-transform.forward) * 2 + new Vector3(0, transform.localScale.y + 4);
            Vector3 direction = (targetCameraPosition - transform.position).normalized;
            Ray thirdPersonRay = new Ray(transform.position, direction);
            RaycastHit thirdPersonHit;
            if (Physics.Raycast(thirdPersonRay, out thirdPersonHit))
                targetCameraPosition = thirdPersonHit.point;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCameraPosition, Time.deltaTime * 2.0f);
            mainCamera.transform.LookAt(transform.position);
        } else
        {
            mainCamera.transform.rotation = cameraDock.rotation;
            mainCamera.transform.position = cameraDock.position;
        }
        Vector3 cam2PlayerDir = (transform.position - mainCamera.transform.position).normalized;
        Debug.DrawRay(mainCamera.transform.position, cam2PlayerDir * 1000);
        Ray cameraBlockageRay = new Ray(mainCamera.transform.position, cam2PlayerDir);
        RaycastHit cameraBlock;
        if (Physics.Raycast(cameraBlockageRay, out cameraBlock) && cameraBlock.transform != transform)
        {
            GameObject hitObject = cameraBlock.transform.gameObject;
            if (!fadedOutObjects.Contains(hitObject))
                fadedOutObjects.Add(hitObject);
            hitObject.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            fadedOutObjects.RemoveAll(item =>
            {
                item.GetComponent<Renderer>().enabled = true;
                return true;
            });
        }
    }
}
