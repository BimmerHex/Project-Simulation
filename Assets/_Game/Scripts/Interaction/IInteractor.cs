using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractor
    {
        // Nesneyi tutma yeteneği varsa
        void PickUpObject(GameObject obj);
        
        // Sadece kimlik doğrulaması gerekiyorsa transform vb. eklenebilir.
        Transform transform { get; }
    }
}