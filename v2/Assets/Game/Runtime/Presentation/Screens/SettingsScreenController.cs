using SnakeApple.Infrastructure.Save;
using SnakeApple.Presentation.Navigation;
using SnakeApple.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SnakeApple.Presentation.Screens
{
    public sealed class SettingsScreenController : MonoBehaviour
    {
        private ScreenNavigator navigator;

        private void Awake()
        {
            navigator = GetComponent<ScreenNavigator>();
            UiLookup.Find<Button>(transform, "BackButton").onClick.AddListener(() => navigator.Load(ScreenNames.Home));

            BindToggle("MusicToggle", LocalProfileStore.Music, value => LocalProfileStore.Music = value);
            BindToggle("SfxToggle", LocalProfileStore.Sfx, value => LocalProfileStore.Sfx = value);
            BindToggle("HapticsToggle", LocalProfileStore.Haptics, value => LocalProfileStore.Haptics = value);
            BindToggle("ReducedMotionToggle", LocalProfileStore.ReducedMotion, value => LocalProfileStore.ReducedMotion = value);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) navigator.Load(ScreenNames.Home);
        }

        private void BindToggle(string objectName, bool initialValue, UnityEngine.Events.UnityAction<bool> onChanged)
        {
            var toggle = UiLookup.Find<Toggle>(transform, objectName);
            toggle.SetIsOnWithoutNotify(initialValue);
            toggle.onValueChanged.AddListener(onChanged);
        }
    }
}
