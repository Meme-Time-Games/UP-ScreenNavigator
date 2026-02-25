using DependencyInjector.Core;
using DependencyInjector.Installers;
using UnityEngine;

namespace ScreenNavigators.Core
{
    public class ForceStopClosingAnimationOnScreenOpenCommandInstaller : SingleMonoInstaller<ForceStopClosingAnimationOnScreenOpenCommand>
    {
        [Header("References")]
        [SerializeField] private ScreenDataSO _screenDataSO;
        [SerializeField] private SingleMonoInstaller<AnimationScreenDeactivatorPresenter> _animationScreenDeactivatorPresenter;

        [Inject] private IScreenNavigator _screenNavigator;
        
        protected override ForceStopClosingAnimationOnScreenOpenCommand GetData()
        {
            return new ForceStopClosingAnimationOnScreenOpenCommand(_screenNavigator, _screenDataSO.ScreenId,
                _animationScreenDeactivatorPresenter.ServiceInstance);
        }
    }
}