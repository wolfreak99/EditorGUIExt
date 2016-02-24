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
    	public LabelListItem(string name) { this.name = name; }
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
            GUI.Box(mItemRect, GUIContent.none);
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
        public static GUIStyle sDefBGStyle = GUIStyle.none;

		public static void ItemList(ListItemCollection items, ref Vector2 scrollPos, ref bool guiChanged, GUIStyle backgroundStyle, params GUILayoutOption[] options) {
			ItemList(items, ref scrollPos, ref guiChanged, null, backgroundStyle, options);
		}
		public static void ItemList(ListItemCollection items, ref Vector2 scrollPos, ref bool guiChanged, Action updateContentsFunction, params GUILayoutOption[] options) {
			ItemList(items, ref scrollPos, ref guiChanged, updateContentsFunction, sDefBGStyle, options);
		}
		public static void ItemList(ListItemCollection items, ref Vector2 scrollPos, ref bool guiChanged, params GUILayoutOption[] options) {
			ItemList(items, ref scrollPos, ref guiChanged, null, sDefBGStyle, options);
		}
		
		public static bool ItemList(ListItemCollection items, ref Vector2 scrollPos, Action updateContentsFunction, GUIStyle backgroundStyle, params GUILayoutOption[] options) {
			bool guiChanged = false;
			ItemList(items, ref scrollPos, ref guiChanged, updateContentsFunction, backgroundStyle, options);
			return guiChanged;
		}
		public static bool ItemList(ListItemCollection items, ref Vector2 scrollPos, Action updateContentsFunction, params GUILayoutOption[] options) {
			return ItemList(items, ref scrollPos, updateContentsFunction, sDefBGStyle, options);
		}
		public static bool ItemList(ListItemCollection items, ref Vector2 scrollPos, GUIStyle backgroundStyle, params GUILayoutOption[] options) {
			return ItemList(items, ref scrollPos, null, backgroundStyle, options);
		}
		public static bool ItemList(ListItemCollection items, ref Vector2 scrollPos, params GUILayoutOption[] options) {
			return ItemList(items, ref scrollPos, null, sDefBGStyle, options);
		}

		public static void ItemList(ListItemCollection items, ref Vector2 scrollPos, ref bool guiChanged, Action updateContentsFunction, GUIStyle bgStyle, params GUILayoutOption[] options) {
			var e = Event.current;
			if (updateContentsFunction != null && e.type == EventType.Repaint) {
				updateContentsFunction();
			}

			// Get default nouse position out of view.
			Vector2 mpos = new Vector2(-99, -99);
			if (e.type == EventType.MouseDown && e.button == 0) {

				mpos = e.mousePosition + scrollPos;

				bool ctrlPressed = (e.modifiers & EventModifiers.Control) > 0;
				bool shiftPressed = (e.modifiers & EventModifiers.Shift) > 0;

				// handle click event
				for (int i = 0; i < items.Count; i += 1) {
					Rect itemBounds = items[i].GetBounds();
					//Check for selection
					if (itemBounds.Contains(mpos)) {
						// signal window repaint needed.
						guiChanged = true;
						if (shiftPressed && items.Selected.Count >= 1) {
							int lastIndex = items.IndexOf(items.Selected[items.Selected.Count - 1]);
							int upper = Mathf.Max(lastIndex, i);
							int lower = Mathf.Min(lastIndex, i);
							if (!ctrlPressed) {
								//Wait until after the last index was cached before clearing
								items.ClearSelection();
							}
							for (int ii = lower; ii <= upper; ii++) {
								items.Select(ii);
							}
						}
						else {
							if (!ctrlPressed)
								items.ClearSelection();
							if (items.ItemSelected(i) && ctrlPressed)
								items.Deselect(i);
							else
								items.Select(i);
						}
					}
				}
			}
			
			// Draw the ListView
			if (options == null || options.Length == 0) {
				options = new GUILayoutOption[]{ GUILayout.ExpandHeight(true) };
			}
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, bgStyle, options);
			if (items != null) {
				foreach (IListItem item in items) {
					if (items.Selected.Contains(item)) {
						item.DrawSelected();
					}
					else {
						item.DrawItem();
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}
    }
}
