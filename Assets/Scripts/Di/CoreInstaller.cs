using MageTest.Core.Controllers;
using MageTest.Core.Factories;
using MageTest.Core.Pools;
using MageTest.Gui;
using Zenject;

namespace MageTest.Di
{
    public class CoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EnemyPool>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<GuiController>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<ProjectilePool>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<SpellFactory>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<SpellComponentFactory>().FromNew().AsSingle();

            Container.BindInterfacesAndSelfTo<EnemyController>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerController>().FromNew().AsSingle();
        }
    }
}