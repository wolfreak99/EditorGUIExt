namespace UnityEditorExtensions.Examples
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditorExtensions;

	public class ItemListExampleWindow : EditorWindow
	{
		ListItemCollection mCollection;
		[MenuItem("Example/ItemList")]
		public static ItemListExampleWindow Init() {
			var win = GetWindow<ItemListExampleWindow>();
			win.Show();
			return win;
		}
		void OnEnable() {
			mCollection = new ListItemCollection();
			for (int i = 0; i < 20; i++) {
				mCollection.Add(new LabelListItem("Item " + i.ToString() + " example"));
			}
		}
		private Vector2 scrollPos = new Vector2();
		void OnGUI() {
			bool changed = false;
			EditorGUIExt.ItemList(mCollection, ref scrollPos, ref changed);
			if (changed) {
				Repaint();
			}

		}

	}
}
