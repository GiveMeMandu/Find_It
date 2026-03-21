using Manager;
using UI.Page;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public abstract class PageViewModel : View
    {
        public override void OnEscapePressed()
        {
            Global.UIManager.CloseCurrentPage();
        }
        [Binding]
        public virtual void ClosePage()
        {
            Global.UIManager.ClosePage();
        }
        [Binding]
        public void OnClickShopButton()
        {
            if(!(Global.UIManager.GetCurrentPage() is ShopPage))
                Global.UIManager.OpenPage<ShopPage>();
        }
    }
}