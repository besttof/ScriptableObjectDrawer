using UnityEditor;
using UnityEngine;

namespace Besttof.ScriptableObjectDrawer.Editor
{
	public class ScriptableObjectPopupContents : PopupWindowContent
	{
		private readonly UnityEditor.Editor _editor;
		private Vector2 _scrollPos;

		private readonly GUIStyle _scrollAreaStyle;
		private readonly ScriptableObject _scriptableObject;
		private readonly float _width;

		private static class Contents
		{
			public static readonly GUIContent PopoutButton = EditorGUIUtility.IconContent("LockIcon", "More");
		}

		private static class Styles
		{
			public static readonly GUIStyle ScrollArea = new()
			                                             {
				                                             margin = new RectOffset(2, 2, 2, 2),
			                                             };

			public static readonly GUIStyle Header = new(EditorStyles.boldLabel)
			                                         {
				                                         padding = new RectOffset(2, 2, 2, 2)
			                                         };

			public static readonly GUIStyle PropertiesButton = new(EditorStyles.miniButton)
			                                                   {
				                                                   fixedWidth = 22f,
				                                                   fixedHeight = 22f,
				                                                   padding = new RectOffset(2, 2, 2, 2),
			                                                   };
		}

		public ScriptableObjectPopupContents(ScriptableObject scriptableObject, SerializedProperty property, float width)
		{
			_scriptableObject = scriptableObject;
			_width = Mathf.Max(300f, width);
			UnityEditor.Editor.CreateCachedEditor(scriptableObject, null, ref _editor);
		}

		public override Vector2 GetWindowSize()
		{
			var so = new SerializedObject(_scriptableObject);
			var it = so.GetIterator();
			it.NextVisible(true);

			var height = EditorGUIUtility.singleLineHeight +
			             EditorGUIUtility.standardVerticalSpacing * 2f +
			             Styles.ScrollArea.margin.vertical +
			             Styles.Header.padding.vertical +
			             EditorStyles.inspectorDefaultMargins.padding.vertical +
			             EditorStyles.inspectorDefaultMargins.margin.vertical;

			do
			{
				var propertyHeight = EditorGUI.GetPropertyHeight(it, GUIContent.none, true);
				// Debug.Log($"{it.propertyPath} {propertyHeight}");
				height += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
			} while (it.NextVisible(false));

			return new Vector2(_width, height);
		}

		public override void OnClose()
		{
			Debug.Log($"close");
			base.OnClose();
		}

		// This complains that the window is 0,0 size. UIToolkit is _so_ ready to replace IMGui...
		// public override UnityEngine.UIElements.VisualElement CreateGUI()
		// {
		// 	return _editor.CreateInspectorGUI();
		// }

		public override void OnGUI(Rect rect)
		{
			using (var _ = new GUILayout.HorizontalScope(Styles.Header))
			{
				if (GUILayout.Button(Contents.PopoutButton, Styles.PropertiesButton))
				{
					EditorUtility.OpenPropertyEditor(_scriptableObject);
				}

				EditorGUILayout.InspectorTitlebar(true, _scriptableObject, false);
			}
			
			using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos, Styles.ScrollArea))
			{
				_editor.OnInspectorGUI();
				_scrollPos = scroll.scrollPosition;
			}
		}
	}

	public class NestedScriptableObjectPopupContents : ScriptableObjectPopupContents
	{
		public NestedScriptableObjectPopupContents(ScriptableObjectPopupContents parent, ScriptableObject scriptableObject, SerializedProperty property, float width) : base(scriptableObject, property, width)
		{
			
		}
	}
}