using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Besttof.ScriptableObjectDrawer.Editor
{
	public class ScriptableObjectPopupContents : PopupWindowContent
	{
		private readonly UnityEditor.Editor _editor;
		private Vector2 _scrollPos;

		private readonly GUIStyle _scrollAreaStyle;
		private readonly ScriptableObject _scriptableObject;
		private readonly float _width;

		private static readonly Stack<ScriptableObjectPopupContents> _popupStack = new();

		private static class Contents
		{
			public static readonly GUIContent PopoutButton = EditorGUIUtility.IconContent("LockIcon-On", "More");
		}

		private static class Styles
		{
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

		public ScriptableObjectPopupContents(ScriptableObject scriptableObject, float width)
		{
			_scriptableObject = scriptableObject;
			_width = Mathf.Max(300f, width);
			UnityEditor.Editor.CreateCachedEditor(scriptableObject, null, ref _editor);
		}

		public override void OnOpen()
		{
			_popupStack.Push(this);
		}

		public override void OnClose()
		{
			_popupStack.TryPopUntil(this);
			base.OnClose();
		}

		public override VisualElement CreateGUI()
		{
			var root = new VisualElement
			           {
				           style = { width = _width }
			           };
			
			root.Add(new IMGUIContainer(TitleBarGUI));
			var inspector = new InspectorElement(_scriptableObject);
			root.Add(inspector);
			return root;
		}

		public void TitleBarGUI()
		{
			using (var _ = new GUILayout.HorizontalScope(Styles.Header))
			{
				if (GUILayout.Button(Contents.PopoutButton, Styles.PropertiesButton))
				{
					EditorUtility.OpenPropertyEditor(_scriptableObject);
				}

				EditorGUILayout.InspectorTitlebar(true, _scriptableObject, false);
			}
		}

		internal static void ShowNestedPopup(Rect position, ScriptableObject scriptableObject, float width)
		{
			var stackCount = _popupStack.Count;

			var contents = stackCount switch
			{
				0 => new ScriptableObjectPopupContents(scriptableObject, width),
				1 => new Level1(scriptableObject, width),
				2 => new Level2(scriptableObject, width),
				3 => new Level3(scriptableObject, width),
				4 => new Level4(scriptableObject, width),
				_ => null,
			};

			if (contents == null)
			{
				EditorUtility.OpenPropertyEditor(scriptableObject);
			}
			else
			{
				PopupWindow.Show(position, contents);
			}
		}

		internal static void ResetStack()
		{
			Debug.Assert(_popupStack.Count == 0, "Stack should have been empty.");
			_popupStack.Clear();
		}

		internal static bool IsLastPossibleLevel(EditorWindow window)
		{
			return _popupStack.TryPeek(out var top) && top.editorWindow == window && _popupStack.Count == 4;
		}

		private class Level1 : ScriptableObjectPopupContents
		{
			public Level1(ScriptableObject scriptableObject, float width) : base(scriptableObject, width) { }
		}

		public class Level2 : ScriptableObjectPopupContents
		{
			public Level2(ScriptableObject scriptableObject, float width) : base(scriptableObject, width) { }
		}

		public class Level3 : ScriptableObjectPopupContents
		{
			public Level3(ScriptableObject scriptableObject, float width) : base(scriptableObject, width) { }
		}

		public class Level4 : ScriptableObjectPopupContents
		{
			public Level4(ScriptableObject scriptableObject, float width) : base(scriptableObject, width) { }
		}
	}


	internal static class StackExtensions
	{
		internal static bool TryPopUntil<T>(this Stack<T> stack, T value)
		{
			while (stack.Count > 0)
			{
				if (stack.TryPop(out var result) && value.Equals(result))
				{
					return true;
				}
			}

			return false;
		}
	}
}