using System;
using UnityEngine;

namespace SnakeApple.Presentation.UI
{
    public static class UiLookup
    {
        public static T Find<T>(Transform root, string objectName) where T : Component
        {
            var component = FindOptional<T>(root, objectName);
            if (component != null)
            {
                return component;
            }

            throw new InvalidOperationException(
                $"Required UI object '{objectName}' with component {typeof(T).Name} was not found under {root.name}.");
        }

        public static T FindOptional<T>(Transform root, string objectName) where T : Component
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == objectName)
                {
                    var component = transforms[index].GetComponent<T>();
                    if (component != null)
                    {
                        return component;
                    }
                }
            }

            return null;
        }

        public static GameObject FindObject(Transform root, string objectName)
        {
            return Find<RectTransform>(root, objectName).gameObject;
        }
    }
}

