using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace UnityEditorExtensions
{
    public class EditorGUIExtLayout
    {
        public static Rect ItemList(
            float height,
            ref ListItemCollection items,
            Action updateContentsFunction,
            ref Vector2 scrollPos)
        {
            Rect result = EditorGUILayout.BeginVertical(GUILayout.MaxHeight(height));
            EditorGUIExt.ItemList(items, updateContentsFunction, ref scrollPos);
            EditorGUILayout.EndVertical();
            return result;
        }


        public static Rect ItemList(
            ListItemCollection items,
            Action updateContentsFunction,
            ref Vector2 scrollPos)
        {
            Rect result = EditorGUILayout.BeginVertical();
            EditorGUIExt.ItemList(items, updateContentsFunction, ref scrollPos);
            EditorGUILayout.EndVertical();
            return result;
        }
    }
}
