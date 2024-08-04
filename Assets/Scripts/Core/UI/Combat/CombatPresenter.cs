using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Gui;

namespace MageTest.Core.UI.Combat
{
    public class CombatPresenter : BasePresenter<CombatView>
    {
        public override UILayerEnum Layer => UILayerEnum.Main;

        public new UniTask<bool> LoadAndShowWindow(CancellationToken token)
        {
            return base.LoadAndShowWindow(token);
        }
    }
}