using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnakeApple.Presentation.Navigation
{
    public sealed class ScreenNavigator : MonoBehaviour
    {
        private bool isLoading;

        public void Load(string sceneName)
        {
            if (!isLoading)
            {
                StartCoroutine(LoadRoutine(sceneName));
            }
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            isLoading = true;
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (operation != null && !operation.isDone)
            {
                yield return null;
            }
        }
    }
}

