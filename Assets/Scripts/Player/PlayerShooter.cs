using System.Collections;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerShooter : MonoBehaviour, IWeapon
    {
        [Header("References")]
        public Transform cameraPivot;
        public Camera playerCamera;
        public WeaponAssembler weaponAssembler;
        public Transform muzzlePoint;

        [Header("Movement Settings")]
        public float moveSpeed = 6.0f;
        public float gravity = -19.62f;
        public float cameraPitchLimit = 80f;

        [Header("Weapon Status")]
        private int currentAmmo;
        private bool isReloading;
        private float nextFireTime;
        private float currentPitch;
        private float currentYaw;
        private float defaultFOV = 60f;

        // Recoil state
        private Vector2 recoilOffset;

        // Interface References
        private CharacterController characterController;
        private IInputProvider inputProvider;

        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => weaponAssembler != null ? weaponAssembler.EffectiveMaxAmmo : 30;
        public bool IsReloading => isReloading;
        public float CurrentDamage => weaponAssembler != null ? weaponAssembler.EffectiveDamage : 25f;
        public float CurrentFireRate => weaponAssembler != null ? weaponAssembler.EffectiveFireRate : 0.15f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputProvider = GetComponent<IInputProvider>();

            if (playerCamera != null)
            {
                defaultFOV = playerCamera.fieldOfView;
            }
        }

        private void Start()
        {
            if (weaponAssembler != null)
            {
                currentAmmo = weaponAssembler.EffectiveMaxAmmo;
            }
            else
            {
                currentAmmo = 30;
            }
            GameEvents.TriggerAmmoChanged(currentAmmo, MaxAmmo);

            GameEvents.OnWeaponStatsUpdated += OnWeaponStatsUpdated;
        }

        private void OnDestroy()
        {
            GameEvents.OnWeaponStatsUpdated -= OnWeaponStatsUpdated;
        }

        private void Update()
        {
            if (inputProvider == null) return;

            HandleMovement();
            HandleLook();
            HandleADS();
            HandleCombat();
            RecoverRecoil();
        }

        private void HandleMovement()
        {
            Vector2 moveInput = inputProvider.MoveInput;
            Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
            Vector3 velocity = moveDir * moveSpeed;
            velocity.y = gravity * Time.deltaTime; // Simple gravity

            characterController.Move(velocity * Time.deltaTime);
        }

        private void HandleLook()
        {
            Vector2 lookInput = inputProvider.LookInput;
            currentYaw += lookInput.x;
            currentPitch -= lookInput.y;
            currentPitch = Mathf.Clamp(currentPitch, -cameraPitchLimit, cameraPitchLimit);

            transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(currentPitch + recoilOffset.y, recoilOffset.x, 0f);
            }
        }

        private void HandleADS()
        {
            if (playerCamera == null) return;

            float targetFOV = inputProvider.IsADS ? 35f : defaultFOV;
            float lerpSpeed = weaponAssembler != null ? weaponAssembler.EffectiveADSSpeed : 8f;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * lerpSpeed);
        }

        private void HandleCombat()
        {
            if (inputProvider.IsReloadingRequested && !isReloading && currentAmmo < MaxAmmo)
            {
                Reload();
            }

            if (inputProvider.IsFiring && Time.time >= nextFireTime && !isReloading)
            {
                if (currentAmmo > 0)
                {
                    Fire();
                }
                else
                {
                    Reload();
                }
            }
        }

        public void Fire()
        {
            nextFireTime = Time.time + CurrentFireRate;
            currentAmmo--;
            GameEvents.TriggerAmmoChanged(currentAmmo, MaxAmmo);
            GameEvents.TriggerWeaponFired();

            // Apply procedural recoil shake
            float recoilAmount = weaponAssembler != null ? weaponAssembler.EffectiveRecoil : 0.5f;
            recoilOffset += new Vector2(Random.Range(-recoilAmount, recoilAmount) * 0.5f, recoilAmount * 1.5f);

            // Raycast Bullet Shot
            Ray ray = playerCamera != null ? 
                playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)) : 
                new Ray(transform.position + Vector3.up * 1.6f, transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    HitboxType hitboxType = HitboxType.Body;
                    BossHitbox hitbox = hit.collider.GetComponent<BossHitbox>();
                    if (hitbox != null)
                    {
                        hitboxType = hitbox.hitboxType;
                    }

                    damageable.TakeDamage(CurrentDamage, hitboxType, hit.point, hit.normal);
                }

                // Visual Bullet Impact Tracer / Flash
                CreateBulletImpactVFX(hit.point, hit.normal);
            }
        }

        public void StartADS() { }
        public void StopADS() { }

        public void Reload()
        {
            if (isReloading) return;
            StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            isReloading = true;
            float reloadTime = weaponAssembler != null && weaponAssembler.baseWeaponData != null ? 
                weaponAssembler.baseWeaponData.reloadTime : 2.0f;

            yield return new WaitForSeconds(reloadTime);

            currentAmmo = MaxAmmo;
            isReloading = false;
            GameEvents.TriggerAmmoChanged(currentAmmo, MaxAmmo);
            GameEvents.TriggerWeaponReloaded();
        }

        private void RecoverRecoil()
        {
            recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, Time.deltaTime * 10f);
        }

        private void OnWeaponStatsUpdated()
        {
            if (currentAmmo > MaxAmmo)
            {
                currentAmmo = MaxAmmo;
                GameEvents.TriggerAmmoChanged(currentAmmo, MaxAmmo);
            }
        }

        private void CreateBulletImpactVFX(Vector3 point, Vector3 normal)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "BulletImpactVFX";
            spark.transform.position = point + normal * 0.05f;
            spark.transform.localScale = Vector3.one * 0.15f;
            
            Renderer ren = spark.GetComponent<Renderer>();
            if (ren != null)
            {
                Material m = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Diffuse"));
                m.color = Color.yellow;
                ren.material = m;
            }

            Collider c = spark.GetComponent<Collider>();
            if (c != null) Destroy(c);

            Destroy(spark, 0.2f);
        }
    }
}
