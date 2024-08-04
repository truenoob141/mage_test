using MageTest.Common.UI.Home;
using MageTest.Core.Controllers;
using MageTest.Core.Factories;
using MageTest.Core.Pools;
using MageTest.Core.Services;
using MageTest.Core.UI.Combat;
using MageTest.Gui;
using Zenject;

namespace MageTest.Di
{
    public class CoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EnemyPool>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<ProjectilePool>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<SpellFactory>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<SpellComponentFactory>().FromNew().AsSingle();
            
            Container.BindInterfacesAndSelfTo<EnemyController>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<GameController>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<GameService>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().FromNew().AsSingle();
            
            // TODO Move out
            Container.BindInterfacesAndSelfTo<GuiController>().FromComponentInHierarchy().AsSingle();
            
            Container.BindInterfacesAndSelfTo<CombatPresenter>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<HomePresenter>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<EventDispatcher>().FromNew().AsSingle();
        }
    }
}