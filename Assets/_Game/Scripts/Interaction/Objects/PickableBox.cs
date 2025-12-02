using UnityEngine;

namespace Game.Interaction.Objects
{
    [RequireComponent(typeof(Rigidbody))]
    public class PickableBox : MonoBehaviour, IInteractable, ISecondaryUsable
    {
        [Header("Settings")]
        [SerializeField] private string _pickupMessage = "[E] Kutuyu Taşı";
        [SerializeField] private Animator _animator; // Kapak animasyonu için
        
        private bool _isOpen = false;

        // --- IInteractable (Yerdeyken Etkileşim) ---

        public InteractionStatus GetInteractionStatus(bool hasItemInHand)
        {
            // Eğer oyuncunun eli doluysa bu kutuyu alamaz.
            if (hasItemInHand)
            {
                return new InteractionStatus(false, ""); 
            }
            
            return new InteractionStatus(true, _pickupMessage);
        }

        public void Interact(IInteractor player)
        {
            // Player'a "Beni al" diyoruz.
            player.PickUpObject(this.gameObject);
        }

        // --- ISecondaryUsable (Eldeyken Sağ Tık) ---

        public void OnSecondaryUseStart()
        {
            _isOpen = !_isOpen; // Durumu tersine çevir
            
            Debug.Log($"Kutu kapağı: {(_isOpen ? "Açıldı" : "Kapandı")}");

            // Eğer Animator varsa parametreyi tetikle
            if (_animator != null)
            {
                _animator.SetBool("IsOpen", _isOpen);
            }
        }

        public void OnSecondaryUseEnd()
        {
            // Bas-çek mantığı değil toggle (aç/kapa) yaptığımız için burası boş kalabilir.
        }
    }
}