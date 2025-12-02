namespace Game.Interaction
{
    // Eğer bir eşya Sağ Tık (Secondary Action) ile bir şey yapacaksa bunu kullanır.
    // Örn: Koli kapağı açmak, Silahla nişan almak.
    public interface ISecondaryUsable
    {
        void OnSecondaryUseStart(); // Tıkladığında
        void OnSecondaryUseEnd();   // Bıraktığında (Opsiyonel, bas-çek için)
    }

    // Eğer bir eşya Sol Tık (Primary Action) ile özel bir şey yapacaksa.
    // Örn: Silah ateşlemek. (Fırlatmak genel bir eylem olduğu için buna dahil etmeyebiliriz)
    public interface IUsable
    {
        void OnUse();
    }
}