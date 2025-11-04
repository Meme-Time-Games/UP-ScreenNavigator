using System;
using DependencyInjector.Core;
using ScreenNavigators.Core;
using ServiceLocatorPattern;
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class OpenedScreenMonitor : EditorWindow
    {
        private IScreenNavigator _screenNavigator;
        
        [MenuItem("Tools/OpenedScreenMonitor")]
        public static void GetWindow()
        {
            EditorWindow.GetWindow(typeof(OpenedScreenMonitor));
        }

        private void OnEnable()
        {
            
        }

        private void OnGUI()
        {
            if (_screenNavigator == null)
            {
                GUILayout.Label("No ScreenNavigator instance found.");

                if (ServiceLocatorInstance.Instance.IsContained<IDIContainer>())
                {
                    IDIContainer container = ServiceLocatorInstance.Instance.Get<IDIContainer>();
                    
                    if(container.IsTypeContained(typeof(IScreenNavigator)))
                        _screenNavigator = container.Get<IScreenNavigator>();
                }
                
                return;
            }
            
            string[] openScreenIds = _screenNavigator.GetOpenScreenIds();

            foreach (var screenId in openScreenIds)
            {
                GUILayout.Label(screenId);
            }
        }
    }
}