using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SnakeApple.Editor
{
    // Adds a "Snake Portrait" Game view size and selects it so Play mode matches the portrait design.
    [InitializeOnLoad]
    public static class GameViewPortrait
    {
        private const string SizeLabel = "Snake Portrait 1080x1920";
        private const string SessionKey = "SnakeApple.GameViewPortraitApplied";

        static GameViewPortrait()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("Snake Apple V2/Game View Portrait 1080x1920")]
        public static void Apply()
        {
            try
            {
                var sizesType = Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var sizes = singleType.GetProperty("instance").GetValue(null);
                var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { 0 }); // Standalone group
                var groupType = group.GetType();

                var index = FindIndex(group, groupType);
                if (index < 0)
                {
                    var sizeType = Type.GetType("UnityEditor.GameViewSize,UnityEditor");
                    var enumType = Type.GetType("UnityEditor.GameViewSizeType,UnityEditor");
                    var fixedRes = Enum.ToObject(enumType, 1);
                    var size = Activator.CreateInstance(sizeType, fixedRes, 1080, 1920, SizeLabel);
                    groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                    index = FindIndex(group, groupType);
                }

                var viewType = Type.GetType("UnityEditor.GameView,UnityEditor");
                var view = EditorWindow.GetWindow(viewType, false, null, false);
                viewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(view, new object[] { index, null });
                view.Repaint();
            }
            catch (Exception e)
            {
                Debug.LogWarning("GameViewPortrait: could not set Game view size automatically. " + e.Message);
            }
        }

        private static int FindIndex(object group, Type groupType)
        {
            var texts = (string[])groupType.GetMethod("GetDisplayTexts").Invoke(group, null);
            for (var i = 0; i < texts.Length; i++)
                if (texts[i].StartsWith(SizeLabel, StringComparison.Ordinal)) return i;
            return -1;
        }
    }
}
