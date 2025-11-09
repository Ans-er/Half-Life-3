using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkTransform))]
public class FPS_Character_Controller : NetworkBehaviour
{
    public bool canMove = true;
    public bool isSprinting;
    public NetworkVariable<int> playerHP = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 9.81f;

    public Camera fpsCamera;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public Transform gunTransform;

    public GameObject bulletPrefab;
    public Transform gunTip;
    public float bulletForce = 20f;

    public GameObject bombPrefab;
    public float bombForce = 10f;

    public int maxHP = 100;
    public Transform spawnMarker;

    private Vector2 lookInput;
    private float rotationX;
    private CharacterController characterController;
    private Vector3 moveVelocity;
    private Vector2 moveInput;
    private PlayerInput playerInput;
    private InputAction bombAction;

    private NetworkVariable<Vector3> networkSpawnPos = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Quaternion> networkSpawnRot = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Application.runInBackground = true;
        if (GetComponent<NetworkTransform>() == null)
        {
            gameObject.AddComponent<NetworkTransform>();
        }
    }

    public override void OnNetworkSpawn()
    {
        playerInput = GetComponent<PlayerInput>();
        if (IsOwner)
        {
            playerInput.enabled = true;
            bombAction = playerInput.actions != null ? playerInput.actions["Bomb"] : null;
            if (bombAction != null) bombAction.performed += HandleBombInput;
        }
        else
        {
            playerInput.enabled = false;
        }
        if (IsServer)
        {
            if (spawnMarker != null)
            {
                networkSpawnPos.Value = spawnMarker.position;
                networkSpawnRot.Value = spawnMarker.rotation;
            }
            else if (networkSpawnPos.Value == Vector3.zero && networkSpawnRot.Value == Quaternion.identity)
            {
                networkSpawnPos.Value = transform.position;
                networkSpawnRot.Value = transform.rotation;
            }
            playerHP.Value = maxHP;
        }
        playerHP.OnValueChanged += OnPlayerHPChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (bombAction != null) bombAction.performed -= HandleBombInput;
        playerHP.OnValueChanged -= OnPlayerHPChanged;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        fpsCamera = GetComponentInChildren<Camera>();
        if (!IsOwner)
        {
            if (fpsCamera != null) fpsCamera.gameObject.SetActive(false);
            characterController.enabled = false;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = canMove ? (isSprinting ? runSpeed : walkSpeed) * moveInput.y : 0f;
        float curSpeedY = canMove ? (isSprinting ? runSpeed : walkSpeed) * moveInput.x : 0f;
        float movementVelocityY = moveVelocity.y;
        moveVelocity = (forward * curSpeedX) + (right * curSpeedY);
        if (canMove)
        {
            rotationX += -lookInput.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            fpsCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            if (gunTransform != null) gunTransform.localRotation = fpsCamera.transform.localRotation;
            transform.rotation *= Quaternion.Euler(0f, lookInput.x * lookSpeed, 0f);
        }
        moveVelocity.y = movementVelocityY;
        if (!characterController.isGrounded) moveVelocity.y -= gravity * Time.deltaTime;
        characterController.Move(moveVelocity * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        playerHP.Value -= damage;
        if (playerHP.Value <= 0)
        {
            RespawnImmediate();
        }
    }

    private void RespawnImmediate()
    {
        bool wasEnabled = characterController != null && characterController.enabled;
        if (wasEnabled) characterController.enabled = false;
        transform.SetPositionAndRotation(networkSpawnPos.Value, networkSpawnRot.Value);
        if (wasEnabled) characterController.enabled = true;
        SetTransformClientRpc(networkSpawnPos.Value, networkSpawnRot.Value);
        playerHP.Value = maxHP;
    }

    [ClientRpc]
    private void SetTransformClientRpc(Vector3 pos, Quaternion rot)
    {
        var cc = characterController;
        bool wasEnabled = cc != null && cc.enabled;
        if (wasEnabled) cc.enabled = false;
        transform.SetPositionAndRotation(pos, rot);
        if (wasEnabled) cc.enabled = true;
    }

    private void OnPlayerHPChanged(int previousValue, int newValue)
    {
        HealthUIScript ui = GetComponentInChildren<HealthUIScript>();
        ui.ChangePlayerHealth(playerHP.Value);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 gunTipPosition, Vector3 gunTipForward)
    {
        if (bulletPrefab == null || gunTip == null)
        {
            Debug.LogWarning("Bullet Prefab or Gun Tip not assigned!");
            return;
        }
        GameObject bullet = Instantiate(bulletPrefab, gunTipPosition, Quaternion.LookRotation(gunTipForward));
        var no = bullet.GetComponent<NetworkObject>();
        if (no != null)
        {
            no.Spawn(true);
            if (bullet.GetComponent<NetworkTransform>() == null) bullet.AddComponent<NetworkTransform>();
        }
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.linearVelocity = gunTipForward * bulletForce;
        }
    }

    [ServerRpc]
    private void BombServerRpc(Vector3 gunTipPosition, Vector3 gunTipForward)
    {
        if (bombPrefab == null || gunTip == null)
        {
            Debug.LogWarning("Bomb Prefab or Gun Tip not assigned!");
            return;
        }
        GameObject bomb = Instantiate(bombPrefab, gunTipPosition, Quaternion.LookRotation(gunTipForward));
        var netObj = bomb.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
            if (bomb.GetComponent<NetworkTransform>() == null) bomb.AddComponent<NetworkTransform>();
        }
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.linearVelocity = gunTipForward * bombForce;
        }
    }

    public void HandleMoveInput(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    public void HandleSprintInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) isSprinting = true; else if (ctx.canceled) isSprinting = false;
    }
    public void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canMove && characterController.isGrounded) moveVelocity.y = jumpPower;
    }
    public void HandleLookInput(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }
    public void HandleShootInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canMove && IsOwner) ShootServerRpc(gunTip.position, gunTip.forward);
    }
    public void HandleBombInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canMove && IsOwner) BombServerRpc(gunTip.position, gunTip.forward);
    }
}
