using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Besttof.ScriptableObjectDrawer.Editor
{
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

		internal static void GetTypeCandidates(FieldInfo fieldInfo, HashSet<Type> result)
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