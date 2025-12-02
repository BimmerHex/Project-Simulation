using Game.Inputs;
using TMPro;
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
        [SerializeField] private Transform _cameraRoot;          // Raycast'in çıkış noktası (Kafa)

        // ARTIK SADECE INTERFACE TANIYORUZ (FirstPersonController yok)
        private IInteractor _interactorSource; 

        // Cache (Önbellek) - Her frame GetComponent yapmamak için
        private IInteractable _currentInteractable;

        // Player'ın elinin dolu olup olmadığını Controller bize haber verecek
        private bool _isHandFull = false;

        private bool _isPromptActive = false; // Panel şu an açık mı?
        private string _currentMessage = "";  // Şu an ne yazıyor?

        private void Awake()
        {
            // BU OBJEDEKİ INTERACTOR YETENEĞİNİ ÇEKİYORUZ
            _interactorSource = GetComponent<IInteractor>();
            
            if (_interactorSource == null)
            {
                Debug.LogError($"{name}: Bu objede IInteractor implemente eden bir script (Örn: Controller) yok!");
                enabled = false;
            }
            
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
            Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);
            
            // IInteractor'ın kendi transform'u null ise (ki olmamalı) hata vermesin
            if (_cameraRoot == null) return; 

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    _currentInteractable = interactable;

                    InteractionStatus status = interactable.GetInteractionStatus(_isHandFull);

                    if (status.CanInteract)
                    {
                        ShowPrompt(true, status.PromptMessage);
                    }
                    else
                    {
                        ShowPrompt(false);
                        _currentInteractable = null;
                    }
                }
                else
                {
                    ClearInteraction();
                }
            }
            else
            {
                ClearInteraction();
            }
        }

        // "E" tuşuna basıldığında çalışır
        private void HandleInteractionInput()
        {
            if (_currentInteractable != null)
            {
                // BURASI DEĞİŞTİ: Artık Controller'ı değil, Interface'i gönderiyoruz.
                _currentInteractable.Interact(_interactorSource);
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

            _isPromptActive = show;
            _currentMessage = message;

            if (_promptPanel != null)
            {
                if (_promptPanel.activeSelf != show) 
                    _promptPanel.SetActive(show);
            }

            if (_promptText != null && show)
            {
                _promptText.text = message;
            }
        }
        
        // Controller (veya Inventory sistemi) bu metodu çağırıp el durumunu güncelleyecek.
        public void SetHandStatus(bool isFull)
        {
            _isHandFull = isFull;
        }
    }
}