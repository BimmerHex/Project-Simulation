namespace Game.Interaction
{
    // Etkileşim verilerini paketledik.
    // Nesne, Player'a cevap verirken bu paketi doldurup verecek.
    public struct InteractionStatus
    {
        public bool CanInteract;       // Şu an etkileşime girilebilir mi? (Müsaitlik)
        public string PromptMessage;   // Ekranda ne yazacak? (Örn: "[E] Kutuyu Al", "[E] Işığı Aç")

        // Constructor ile hızlı veri oluşturma
        public InteractionStatus(bool canInteract, string promptMessage)
        {
            CanInteract = canInteract;
            PromptMessage = promptMessage;
        }
    }

    // TÜM etkileşilebilir nesneler (Kapı, Koli, Lamba) bu sözleşmeyi imzalamak zorunda.
    public interface IInteractable
    {
        // 1. Soru: Seninle şu an etkileşime girebilir miyim? (Raycast çarptığında sorulur)
        // 'hasItemInHand' parametresi Player'ın elinin dolu olup olmadığını bildirir.
        InteractionStatus GetInteractionStatus(bool hasItemInHand);

        // 2. Emir: Etkileşimi başlat! ("E" tuşuna basıldığında çağrılır)
        void Interact(IInteractor player);
    }
}