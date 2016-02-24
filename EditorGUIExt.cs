using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditorExtensions
{
    public interface IListItem
    {
        void DrawItem();
        void DrawSelected();
        Rect GetBounds();
    }

    /** A standard list item with a label and an optional icon.
     * 
     */
    public class LabelListItem : IListItem
    {
        public string name;
        public Func<Texture2D> IconFunction;
        public float height = 16f;
        public Color itemColor = Color.gray;
        private Rect mItemRect;
        public virtual void DrawItem()
        {
            Color tmp = GUI.color;
            GUI.color = itemColor;
            Texture2D icon = null;
            if (IconFunction != null)
                icon = IconFunction();
            GUILayout.Label(new GUIContent(name, icon), GUILayout.MaxHeight(height));
            if (Event.current.type == EventType.Repaint)
            {
                mItemRect = GUILayoutUtility.GetLastRect();
            }
            GUI.color = tmp;
        }

        public virtual void DrawSelected()
        {
            Color tmp = GUI.color;
            GUI.color = Color.white;
            GUI.Box(mItemRect, EditorGUIExt.sEmptyContent);
            DrawItem();
            GUI.color = tmp;
        }

        public Rect GetBounds()
        {
            return mItemRect;
        }
    }

    /** A data structure to organize ListItems within an ItemList
     * 
     */
    public class ListItemCollection : IEnumerable<IListItem>, IList<IListItem>
    {
        public ListItemCollection()
        {
            mItems = new List<IListItem>();
            mSelected = new List<IListItem>();
        }

        private List<IListItem> mItems;
        private List<IListItem> mSelected;
        public List<IListItem> Selected { get { return mSelected; } }

        public IEnumerator<IListItem> GetEnumerator()
        {
            foreach (IListItem i in mItems)
            {
                if (i == null)
                {
                    break;
                }
                yield return i;
            }    
        }
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

        public void Add(IListItem item) { mItems.Add(item); }
        public bool Remove(IListItem item) { return mItems.Remove(item); }
        public void Clear() { mItems.Clear(); }
        public int Count { get { return mItems.Count; } }
        public bool ItemSelected(IListItem item) { return mSelected.Contains(item); }
		public bool ItemSelected(int index) { return ItemSelected(mItems[index]); }
		public IListItem this[int i]
        {
            get { return mItems[i]; }
            set { mItems[i] = value; }
        }

        public void Select(IListItem item) {
			if (!mSelected.Contains(item)) {
				mSelected.Add(item);
			}
		}
		public void Deselect(IListItem item) {
			if (mSelected.Contains(item)) {
				mSelected.Remove(item);
			}
		}
		public void Select(int index) { Select(mItems[index]); }
		public void Deselect(int index) { Deselect(mItems[index]); }

        public void ClearSelection() { mSelected.Clear(); }
        public int IndexOf(IListItem item) { return mItems.IndexOf(item); }
		public void Insert(int index, IListItem item) { mItems.Insert(index, item); }
		public void RemoveAt(int index) { mItems.RemoveAt(index); }
		public bool Contains(IListItem item) { return mItems.Contains(item); }
		public void CopyTo(IListItem[] array, int arrayIndex) { mItems.CopyTo(array, arrayIndex); }
		public bool IsReadOnly {
			get { return false; }
		}
    }

    /** Contains all static GUI extention functions.
     * 
     */
    public class EditorGUIExt
    {
        private static bool sShiftKeyState = false;
        public static GUIContent sEmptyContent = new GUIContent();

        /** Creates an ItemList with the default settings for background rendering.
         * 
         */
        public static void ItemList(
            ListItemCollection items,
            Action updateContentsFunction,
            ref Vector2 scrollPos)
        {
            ItemList(items, updateContentsFunction, ref scrollPos, GUI.Box, sEmptyContent);
        }

        /** Creates an ItemList.
         * 
         */
        public static void ItemList(
            ListItemCollection items,
            Action updateContentsFunction,
            ref Vector2 scrollPos,
            Action<Rect, GUIContent> backgroundRenderFunction,
            GUIContent backgroundContent)
        {
            if (updateContentsFunction != null && Event.current.type == EventType.Repaint)
            {
                updateContentsFunction();
            }

            // Get default nouse position out of view.
            Vector2 mpos = new Vector2(-99, -99);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                mpos = Event.current.mousePosition + scrollPos;
                // handle click event
                for (int i = 0; i < items.Count; i += 1)
                {
                    bool ctrlPressed = (Event.current.modifiers & EventModifiers.Control) > 0;
                    bool shiftPressed = (Event.current.modifiers & EventModifiers.Shift) > 0;
                    // TODO: use shift for bulk select
                    Rect itemBounds = items[i].GetBounds();
                    //Check for selection
                    if (itemBounds.Contains(mpos))
                    {
                        if (!ctrlPressed && !shiftPressed) items.ClearSelection();
                        if (items.ItemSelected(i) && ctrlPressed)
                            items.Deselect(i);
                        else
                            items.Select(i);
                    }
                }
            }


            // Draw the ListView
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (items != null)
            {
                foreach (IListItem item in items)
                {   if (items.Selected.Contains(item))
                    {
                        item.DrawSelected();
                    }
                    else
                    {
                        item.DrawItem();
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            if (Event.current.type == EventType.Repaint)
            {
                Rect scrollArea = GUILayoutUtility.GetLastRect();
                backgroundRenderFunction(scrollArea, backgroundContent);
            }
        }

    }
}
