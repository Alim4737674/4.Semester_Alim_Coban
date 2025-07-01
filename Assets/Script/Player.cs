using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class Player : NetworkBehaviour
{
    public Animator animator;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpForce = 5f;

    public float coyoteTime = 0.2f;
    private float coyoteTimer = 0f;

    private CharacterController controller;
    private float verticalVelocity = 0f;

    public GameObject spawnablePrefab;

    private int points = 0;
    private float pointTimer = 0f;
    private const int costToSpawn = 3;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animator = animator ?? GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("❌ CharacterController fehlt auf dem Player!");
            enabled = false;
            return;
        }

        if (IsOwner)
        {
            // Kamera finden
            OrbitCamera cam = FindFirstObjectByType<OrbitCamera>();
            if (cam != null)
            {
                cam.SetTarget(transform);

                Camera playerCam = cam.GetComponent<Camera>();
                if (playerCam != null)
                {
                    AudioListener listener = playerCam.GetComponent<AudioListener>();
                    if (listener != null) listener.enabled = true;
                }
            }

            // Alle anderen AudioListener deaktivieren
            foreach (AudioListener al in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            {
                if (al.gameObject != gameObject)
                    al.enabled = false;
            }
        }
    }


    void Update()
    {
        pointTimer += Time.deltaTime;
        if (pointTimer >= 1f)
        {
            points += 1;
            pointTimer = 0f;
            Debug.Log("Punkte erhalten → Aktuell: " + points);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E gedrückt. Aktuelle Punkte: " + points);

            if (points >= costToSpawn)
            {
                points -= costToSpawn;
                Debug.Log("Objekt wird gespawnt. Neue Punkte: " + points);
                SpawnServerRpc();
            }
            else
            {
                Debug.Log("Nicht genug Punkte zum Spawnen! Du hast: " + points);
            }
        }



        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnServerRpc();
        }

        MoveAndJump();


        if (!IsOwner) return;
        MoveAndJump();
    }

    void MoveAndJump()
    {
        float hor = 0f;
        float ver = 0f;

        if (Input.GetKey(KeyCode.W)) ver += 1;
        if (Input.GetKey(KeyCode.S)) ver -= 1;
        if (Input.GetKey(KeyCode.D)) hor += 1;
        if (Input.GetKey(KeyCode.A)) hor -= 1;

        Vector3 moveDir = Vector3.zero;
        Camera cam = Camera.main;

        if (cam != null)
        {
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * ver + camRight * hor).normalized;
        }

        bool isWalking = moveDir.magnitude > 0f;
        animator?.SetBool("Walking", isWalking);

        // Coyote Time & Gravity
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (verticalVelocity < 0)
                verticalVelocity = -1f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimer > 0f)
        {
            verticalVelocity = jumpForce;
            coyoteTimer = 0f;
        }

        Vector3 move = moveDir * moveSpeed;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        if (moveDir != Vector3.zero)
        {
            Vector3 lookDir = new Vector3(moveDir.x, 0f, moveDir.z);
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    [ServerRpc]
    void SpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (spawnablePrefab == null)
        {
            Debug.LogWarning("Kein Spawn-Prefab zugewiesen!");
            return;
        }

        Vector3 spawnPos = transform.position + transform.forward * 2f;

        GameObject obj = Instantiate(spawnablePrefab, spawnPos, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn(true); // gibt dir direkt Ownership
    }


}
