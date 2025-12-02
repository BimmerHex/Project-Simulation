using UnityEngine;

namespace Game.Interaction.Test
{
    // IInteractable arayüzünü uyguluyoruz
    public class ColorChangerBox : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _message = "[E] Rengi Değiştir";
        
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        // 1. Soruya Cevap
        public InteractionStatus GetInteractionStatus(bool hasItemInHand)
        {
            // Bu kutu için oyuncunun elinin dolu olması fark etmez.
            // Her zaman etkileşime açıktır.
            return new InteractionStatus(true, _message);
        }

        // 2. Emir Geldiğinde
        public void Interact(IInteractor player)
        {
            Debug.Log("Kutu ile etkileşime girildi!");
            
            // Rastgele renk ver
            _renderer.material.color = Random.ColorHSV();
        }
    }
}