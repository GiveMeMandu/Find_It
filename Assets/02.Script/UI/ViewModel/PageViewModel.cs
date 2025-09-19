using Manager;
using UI.Page;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class PageViewModel : BaseViewModel
    {
        [Binding]
        public void ClosePage()
        {
            Global.UIManager.ClosePage(this);
        }
        [Binding]
        public void OnClickShopButton()
        {
            if(!(Global.UIManager.GetCurrentPage() is ShopPage))
                Global.UIManager.OpenPage<ShopPage>();
        }
    }
}