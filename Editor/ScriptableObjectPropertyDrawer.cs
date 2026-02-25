using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Besttof.ScriptableObjectDrawer.Editor
{
	[CustomPropertyDrawer(typeof(ScriptableObject), true)]
	public class ScriptableObjectPropertyDrawer : PropertyDrawer
	{
		private const string _lastUsedFolderKey = "besttof.createScriptableObjectFolder";
		private const float _buttonWidth = 22f;
		private const float _buttonPadding = 2f;

		private static class Styles
		{
			public static readonly GUIStyle ButtonLeft = new(EditorStyles.miniButtonLeft)
			                                             {
				                                             padding = new RectOffset(3, 3, 2, 2),
				                                             fixedHeight = EditorGUIUtility.singleLineHeight,
			                                             };

			public static readonly GUIStyle ButtonRight = new(EditorStyles.miniButtonRight)
			                                              {
				                                              padding = new RectOffset(3, 3, 2, 2),
				                                              fixedHeight = EditorGUIUtility.singleLineHeight,
			                                              };

			public static readonly GUIStyle FieldButton = new(EditorStyles.miniButton)
			                                              {
				                                              padding = new RectOffset(3, 3, 2, 2),
				                                              fixedHeight = EditorGUIUtility.singleLineHeight,
			                                              };
		}

		private static class Contents
		{
			public static readonly GUIContent AddButton = EditorGUIUtility.IconContent("CreateAddNew", "Create a new ScriptableObject asset");
			public static readonly GUIContent MagicButton = EditorGUIUtility.IconContent("FilterByType", "Automatically assign asset");
			public static readonly GUIContent PopoutButton = EditorGUIUtility.IconContent("SearchJump Icon", "More");
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using var propertyScope = new EditorGUI.PropertyScope(position, GUIContent.none, property);

			var contentPosition = EditorGUI.PrefixLabel(position, label);

			var buttonsWidth = (property.objectReferenceValue == null)
				? DrawCreateGUI(property, contentPosition)
				: DrawPopupButtonGUI(position, contentPosition, property);

			var fieldPosition = new Rect(contentPosition)
			                    {
				                    xMax = contentPosition.xMax - buttonsWidth,
			                    };

			EditorGUI.PropertyField(fieldPosition, property, GUIContent.none, true);
		}

		private static float DrawPopupButtonGUI(Rect position, Rect contentPosition, SerializedProperty property)
		{
			const float totalWidth = _buttonWidth + _buttonPadding;
			var popupButtonPosition = new Rect(contentPosition)
			                          {
				                          x = contentPosition.xMax - _buttonWidth,
				                          width = _buttonWidth,
			                          };

			if (GUI.Button(popupButtonPosition, Contents.PopoutButton, Styles.FieldButton) &&
			    property.objectReferenceValue is ScriptableObject scriptableObject)
			{
				var currentViewWidth = EditorGUIUtility.currentViewWidth;

			}

			return totalWidth;
		}

		private float DrawCreateGUI(SerializedProperty property, Rect contentPosition)
		{
			const float totalWidth = _buttonWidth * 2 + _buttonPadding;

			var addButtonPosition = new Rect(contentPosition)
			                        {
				                        x = contentPosition.xMax - _buttonWidth * 2,
				                        width = _buttonWidth
			                        };

			var magicButtonPosition = new Rect(addButtonPosition)
			                          {
				                          x = contentPosition.xMax - _buttonWidth,
			                          };

			if (GUI.Button(addButtonPosition, Contents.AddButton, Styles.ButtonLeft))
			{
				CreateScriptableObject(property);
			}

			if (GUI.Button(magicButtonPosition, Contents.MagicButton, Styles.ButtonRight))
			{
				AssignScriptableObject(property);
			}

			return totalWidth;
		}

		private void CreateScriptableObject(SerializedProperty property)
		{
			using var poolHandle = HashSetPool<Type>.Get(out HashSet<Type> typeCandidates);
			TypeUtils.GetTypeCandidates(fieldInfo, typeCandidates);

			if (typeCandidates.Count == 1)
			{
				CreateAsset(property, typeCandidates.First());
			}
			else if (typeCandidates.Count > 0)
			{
				var menu = new GenericMenu();
				foreach (var type in typeCandidates)
				{
					menu.AddItem(new GUIContent($"Create {type}"), false, x => CreateAsset(property, x as Type), type);
				}

				menu.ShowAsContext();
			}
			else
			{
				var friendlyTypeName = TypeUtils.GetFriendlyTypeName(fieldInfo.FieldType);

				var message = $"Could not create\n asset for type\n'{friendlyTypeName}'";
				if (fieldInfo.FieldType.IsGenericType)
				{
					message += "\n\n(creating assets for\ngeneric types is not supported)";
				}

				EditorWindow.focusedWindow.ShowNotification(new GUIContent(message));
			}
		}

		private void AssignScriptableObject(SerializedProperty property)
		{
			using var poolHandle = HashSetPool<Type>.Get(out HashSet<Type> typeCandidates);
			TypeUtils.GetTypeCandidates(fieldInfo, typeCandidates);

			if (typeCandidates.Count > 8)
			{
				return;
			}

			// apply some heuristics to guess which object is the most likely to be needed
			// (i.e. the most changed, latest updated one)
			// var asset = typeCandidates.SelectMany(LoadAssetsByType)
			var asset = AssetDatabaseUtils.LoadAllAssetsOfType(typeCandidates)
			                              .OrderByDescending(x =>
			                              {
				                              var count = EditorUtility.GetDirtyCount(x.Asset);
				                              Debug.Log($"{x.Path} {count}");
				                              return count;
			                              })
			                              .ThenByDescending(x => File.GetLastAccessTime(x.Path))
			                              .Select(x => x.Asset)
			                              .FirstOrDefault();

			if (asset != null)
			{
				property.serializedObject.Update();
				property.objectReferenceValue = asset;
				property.serializedObject.ApplyModifiedProperties();
			}
			else
			{
				EditorWindow.focusedWindow.ShowNotification(new GUIContent($"Could not find\n asset of type\n'{TypeUtils.GetFriendlyTypeName(fieldInfo.FieldType)}'"));
			}
		}

		private static void CreateAsset(SerializedProperty property, Type type)
		{
			var folderName = SessionState.GetString(_lastUsedFolderKey, Application.dataPath);
			var assetPath = EditorUtility.SaveFilePanelInProject($"Create new {type}", $"{type.Name}", "asset", "", folderName);
			if (string.IsNullOrWhiteSpace(assetPath)) return;

			SessionState.SetString(_lastUsedFolderKey, Path.GetDirectoryName(assetPath));
			Debug.Log(TypeUtility.CanBeInstantiated(type));
			var newObject = ScriptableObject.CreateInstance(type);

			AssetDatabase.CreateAsset(newObject, assetPath);
			AssetDatabase.SaveAssets();

			property.serializedObject.Update();
			property.objectReferenceValue = newObject;
			property.serializedObject.ApplyModifiedProperties();
		}


		private static IEnumerable<(string path, UnityEngine.Object asset)> LoadAssetsByType(Type type)
		{
			return AssetDatabase.FindAssets($"t:{type}")
			                    .Select(AssetDatabase.GUIDToAssetPath)
			                    .Select(path => (path, asset: AssetDatabase.LoadAssetAtPath(path, type)));
		}
	}

	internal static class AssetDatabaseUtils
	{
		// internal static IEnumerable<(string path, UnityEngine.Object asset)> LoadAssetsByType(Type type)
		// {
		// 	return AssetDatabase.FindAssets($"t:{type}")
		// 	                    .Select(AssetDatabase.GUIDToAssetPath)
		// 	                    .Select(path => (path, asset: AssetDatabase.LoadAssetAtPath(path, type)));
		// }


		internal static IEnumerable<AssetResult> LoadAllAssetsOfType(Type type)
		{
			var allPaths = AssetDatabase.GetAllAssetPaths();
			foreach (var path in allPaths)
			{
				var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
				if (assetType == type)
				{
					yield return new AssetResult
					             {
						             Path = path,
						             Asset = AssetDatabase.LoadAssetAtPath(path, assetType)
					             };
				}
			}
		}

		internal static IEnumerable<AssetResult> LoadAllAssetsOfType(HashSet<Type> types)
		{
			var allPaths = AssetDatabase.GetAllAssetPaths();
			foreach (var path in allPaths)
			{
				var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
				if (types.Contains(assetType))
				{
					yield return new AssetResult
					             {
						             Path = path,
						             Asset = AssetDatabase.LoadAssetAtPath(path, assetType)
					             };
				}
			}
		}

		internal struct AssetResult : IEquatable<AssetResult>
		{
			public string Path;
			public UnityEngine.Object Asset;

			public bool Equals(AssetResult other) => Path == other.Path && Asset == other.Asset;
			public override bool Equals(object obj) => obj is AssetResult other && Equals(other);
			public override int GetHashCode() => HashCode.Combine(Path, Asset);
		}
	}

	internal static class TypeUtils
	{
		internal static string GetFriendlyTypeName(Type type)
		{
			// return TypeUtility.GetTypeDisplayName(type);
			if (!type.IsGenericType) return type.Name;

			var marker = type.Name.IndexOf('`');
			var typeName = type.Name.AsSpan(0, (marker > 0 ? marker : type.Name.Length - 1));

			var genericArguments = type.GetGenericArguments().Select(GetFriendlyTypeName);
			return typeName.ToString() + "<" + string.Join(", ", genericArguments) + ">";
		}

		internal static HashSet<Type> GetTypeCandidates(FieldInfo fieldInfo)
		{
			var set = new HashSet<Type>();
			GetTypeCandidates(fieldInfo, set);
			return set;
		}

		internal static void GetTypeCandidates(FieldInfo fieldInfo, HashSet<Type> result) // hashset
		{
			result.Clear();

			var fieldType = fieldInfo.FieldType;
			var actualType = fieldType;

			// Get the type of the element if it is a list (IEnumerables) or array, for lists
			// we assume the first element to be the element type
			if (fieldType.IsArray)
			{
				actualType = fieldType.GetElementType();
			}
			else if (typeof(IEnumerable).IsAssignableFrom(fieldType))
			{
				actualType = fieldType.GenericTypeArguments.FirstOrDefault();
			}

			if (actualType is null) return;


			if (!actualType.IsGenericType) result.Add(actualType);

			result.UnionWith(TypeCache.GetTypesDerivedFrom(actualType));
		}
	}
}