using UnityEditor;
using UnityEditor.SceneManagement;

namespace SnakeApple.Editor
{
    public static class SceneQuickOpen
    {
        [MenuItem("Snake Apple V2/Open Boot %#b")]
        public static void OpenBoot()
        {
            EditorSceneManager.OpenScene("Assets/Game/Scenes/Boot.unity");
        }

        [MenuItem("Snake Apple V2/Open Gameplay %#g")]
        public static void OpenGameplay()
        {
            EditorSceneManager.OpenScene("Assets/Game/Scenes/Gameplay.unity");
        }
    }
}

