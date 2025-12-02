using Game.Inputs;
using Game.Interaction;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour, IInteractor
    {
        [Header("References")]
        [Tooltip("Project/Assets/_Game/Settings/Input altındaki InputReader assetini buraya sürükle.")]
        [SerializeField] private InputReader _input;
        [SerializeField] private Transform _cameraRoot;

        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 4.0f;
        [SerializeField] private float _sprintSpeed = 8.0f;
        [SerializeField] private float _acceleration = 10.0f;

        [Header("Look Settings")]
        [SerializeField] private float _lookSensitivityX = 0.3f;
        [SerializeField] private float _lookSensitivityY = 0.3f;
        [SerializeField] private float _upperLookLimit = 80.0f;
        [SerializeField] private float _lowerLookLimit = -80.0f;

        [Header("Jump & Gravity")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -15.0f;
        [SerializeField] private float _terminalVelocity = 53.0f;

        [Header("Interaction / Hold System")]
        [SerializeField] private InteractionDetector _interactionDetector; // El durumunu bildirmek için
        [SerializeField] private Transform _holdPoint;        // Eşyanın duracağı konum (Kamera önü)
        [SerializeField] private float _throwForce = 10f;     // Fırlatma gücü

        // State Variables
        private CharacterController _controller;
        private float _cameraPitch = 0f;
        private float _verticalVelocity;
        private float _currentSpeed;
        private Rigidbody _currentHeldObjectRb;               // Şu an tutulan objenin fiziği
        private GameObject _currentHeldObject;                // Şu an tutulan obje (Interface sorgusu için)

        // Constants
        private const float Threshold = 0.01f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (_input == null)
            {
                Debug.LogError($"{name}: InputReader atanmamış! Lütfen Inspector'dan atayın.");
                enabled = false;
            }
        }

        private void Start()
        {
            // Mouse imlecini gizle ve kilitle
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnJumpPerformed += HandleJump;
                _input.OnPrimaryActionPerformed += HandlePrimaryAction;
                _input.OnSecondaryActionPerformed += HandleSecondaryAction;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnJumpPerformed -= HandleJump;
                _input.OnPrimaryActionPerformed -= HandlePrimaryAction;
                _input.OnSecondaryActionPerformed -= HandleSecondaryAction;
            }
        }

        private void Update()
        {
            HandleGravity();
            HandleMovement();
        }

        private void LateUpdate()
        {
            HandleRotation();
        }

        private void HandleMovement()
        {
            // 1. Hedef hızı belirle
            float targetSpeed = _input.IsSprinting ? _sprintSpeed : _walkSpeed;

            // Eğer hareket girdisi yoksa hedef hız 0
            if (_input.MoveInput == Vector2.zero) targetSpeed = 0.0f;

            // 2. Hızı yumuşat (Linear Interpolation - Lerp)
            // Mathf.Lerp kullanarak anlık hızı hedef hıza zamanla yaklaştırıyoruz.
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _acceleration);

            // Çok küçük hızları sıfırla (micro-movement önlemek için)
            if (_currentSpeed < 0.1f) _currentSpeed = 0f;

            // 3. Yönü hesapla
            Vector2 input = _input.MoveInput;
            Vector3 direction = (transform.right * input.x + transform.forward * input.y).normalized;

            // 4. Hareketi uygula (Yatay + Dikey)
            Vector3 finalMove = direction * _currentSpeed + new Vector3(0.0f, _verticalVelocity, 0.0f);
            
            _controller.Move(finalMove * Time.deltaTime);
        }

        private void HandleRotation()
        {
            Vector2 lookInput = _input.LookInput;

            // Input yoksa işlem yapma
            if (lookInput.sqrMagnitude < Threshold) return;

            float lookSpeedX = lookInput.x * _lookSensitivityX;
            float lookSpeedY = lookInput.y * _lookSensitivityY;

            // 1. Gövdeyi döndür (Sağ/Sol)
            transform.Rotate(Vector3.up * lookSpeedX);

            // 2. Kamerayı (Baş) döndür (Yukarı/Aşağı)
            _cameraPitch -= lookSpeedY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _lowerLookLimit, _upperLookLimit);

            _cameraRoot.localRotation = Quaternion.Euler(_cameraPitch, 0.0f, 0.0f);
        }

        private void HandleGravity()
        {
            if (_controller.isGrounded)
            {
                // Yerdeyken hızı sıfırlama, -2f ile yere yapıştır.
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;
            }
            else
            {
                // Terminal hıza ulaşmadıysa yerçekimi uygula
                if (_verticalVelocity > -_terminalVelocity) 
                {
                    _verticalVelocity += _gravity * Time.deltaTime;
                }
            }
        }

        private void HandleJump()
        {
            if (_controller.isGrounded)
            {
                // Fiziksel zıplama formülü: v = sqrt(h * -2 * g)
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }
        }

        public void PickUpObject(GameObject obj)
        {
            if (_currentHeldObject != null) return; // Zaten elimiz dolu

            _currentHeldObject = obj;
            _currentHeldObjectRb = obj.GetComponent<Rigidbody>();

            if (_currentHeldObjectRb != null)
            {
                _currentHeldObjectRb.isKinematic = true; // Fiziği kapat (Elimizde sallanmasın)
                _currentHeldObjectRb.useGravity = false;
                _currentHeldObjectRb.interpolation = RigidbodyInterpolation.None; // Titremeyi önle
            }

            // Objeyi HoldPoint'e taşı ve çocuk yap
            obj.transform.SetParent(_holdPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            // Dedektöre haber ver: "Elim Doldu!"
            if (_interactionDetector != null) _interactionDetector.SetHandStatus(true);
        }

        public void DropObject(bool throwItem)
        {
            if (_currentHeldObject == null) return;

            // Parent'lıktan çıkar
            _currentHeldObject.transform.SetParent(null);

            if (_currentHeldObjectRb != null)
            {
                _currentHeldObjectRb.isKinematic = false; // Fiziği aç
                _currentHeldObjectRb.useGravity = true;
                _currentHeldObjectRb.interpolation = RigidbodyInterpolation.Interpolate;

                // Fırlatma mantığı
                if (throwItem)
                {
                    _currentHeldObjectRb.AddForce(_cameraRoot.forward * _throwForce, ForceMode.Impulse);
                }
                else
                {
                    // Yavaşça bırakma (Hafif ileri koy ki içine girmesin)
                    _currentHeldObjectRb.linearVelocity = Vector3.zero; 
                }
            }

            // Temizlik
            _currentHeldObject = null;
            _currentHeldObjectRb = null;

            // Dedektöre haber ver: "Elim Boş!"
            if (_interactionDetector != null) _interactionDetector.SetHandStatus(false);
        }

        // Input Eventleri
        private void HandlePrimaryAction() // Sol Tık
        {
            if (_currentHeldObject != null)
            {
                // 1. Önce objede özel bir sol tık emri var mı bak (IUsable)
                if (_currentHeldObject.TryGetComponent(out IUsable usableItem))
                {
                    usableItem.OnUse();
                }
                else
                {
                    // 2. Yoksa varsayılan davranış: Fırlat!
                    DropObject(true);
                }
            }
        }

        private void HandleSecondaryAction() // Sağ Tık
        {
            if (_currentHeldObject != null)
            {
                // Objede sağ tık özelliği var mı? (Koli kapağı açma)
                if (_currentHeldObject.TryGetComponent(out ISecondaryUsable secondaryItem))
                {
                    secondaryItem.OnSecondaryUseStart();
                }
            }
        }
    }
}
