using Game.Inputs;
using Game.Player;
using TMPro; // UI için
using UnityEngine;

namespace Game.Interaction
{
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _interactionRange = 3.0f; // Ne kadar uzaktan etkileşime girilir?
        [SerializeField] private LayerMask _interactionLayer;    // Hangi katmandaki objeler taranacak?
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _promptText;    // Ekranda çıkacak yazı "[E] Kapıyı Aç"
        [SerializeField] private GameObject _promptPanel;        // Yazının arkasındaki panel (Varsa)

        [Header("References")]
        [SerializeField] private InputReader _input;
        [SerializeField] private FirstPersonController _playerController;
        [SerializeField] private Transform _cameraRoot;          // Raycast'in çıkış noktası (Kafa)

        // Cache (Önbellek) - Her frame GetComponent yapmamak için
        private IInteractable _currentInteractable;

        // Player'ın elinin dolu olup olmadığını Controller'dan öğreneceğiz (Şimdilik manuel false)
        // İleride burayı _playerController.HasItem() gibi bir şeye bağlayacağız.
        private bool _isHandFull = false;

        private bool _isPromptActive = false; // Panel şu an açık mı?
        private string _currentMessage = "";  // Şu an ne yazıyor?

        private void Awake()
        {
            // Defensive coding
            if (_playerController == null) _playerController = GetComponent<FirstPersonController>();
            if (_promptPanel != null) _promptPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_input != null) _input.OnInteractPerformed += HandleInteractionInput;
        }

        private void OnDisable()
        {
            if (_input != null) _input.OnInteractPerformed -= HandleInteractionInput;
        }

        private void Update()
        {
            DetectInteractable();
        }

        // Raycast ile tarama yapan ana metod
        private void DetectInteractable()
        {
            // Ray, kameranın olduğu yerden ileriye doğru atılır
            Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);
            
            // Debug için Editörde kırmızı çizgi çiz (Sadece Scene ekranında görünür)
            Debug.DrawRay(ray.origin, ray.direction * _interactionRange, Color.red);

            // Raycast atıyoruz
            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer))
            {
                // Çarptığımız obje IInteractable mı?
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    _currentInteractable = interactable;

                    // Nesneye sor: "Seninle şu an etkileşime girebilir miyim?"
                    InteractionStatus status = interactable.GetInteractionStatus(_isHandFull);

                    if (status.CanInteract)
                    {
                        // Evet girebilirsin -> UI Göster
                        ShowPrompt(true, status.PromptMessage);
                    }
                    else
                    {
                        // Hayır giremezsin (Elim dolu vs.) -> UI Gizle
                        ShowPrompt(false);
                        _currentInteractable = null; // Etkileşimi iptal et
                    }
                }
                else
                {
                    // IInteractable değil (Duvar vs.)
                    ClearInteraction();
                }
            }
            else
            {
                // Boşluğa bakıyor
                ClearInteraction();
            }
        }

        // "E" tuşuna basıldığında çalışır
        private void HandleInteractionInput()
        {
            // Eğer geçerli bir etkileşim nesnesi varsa ve o an bakıyorsak
            if (_currentInteractable != null)
            {
                _currentInteractable.Interact(_playerController);
                
                // Etkileşim sonrası UI'ı anlık güncellemek için tekrar kontrol yapılabilir
                // veya nesne kendini yok ettiyse (Örn: Coin toplama) hata vermemesi sağlanır.
            }
        }

        private void ClearInteraction()
        {
            _currentInteractable = null;
            ShowPrompt(false);
        }

        private void ShowPrompt(bool show, string message = "")
        {
            if (show == _isPromptActive && message == _currentMessage) return;

            // Durumu güncelle
            _isPromptActive = show;
            _currentMessage = message;

            if (_promptPanel != null)
            {
                // Sadece durum değiştiğinde SetActive çağır
                if (_promptPanel.activeSelf != show) 
                    _promptPanel.SetActive(show);
            }

            if (_promptText != null && show) // Sadece panel açılacaksa yazıyı değiştir
            {
                _promptText.text = message;
            }
        }
        
        // Bu metodu ileride Inventory sistemi çağırıp "Elim doldu" diyecek.
        public void SetHandStatus(bool isFull)
        {
            _isHandFull = isFull;
        }
    }
}