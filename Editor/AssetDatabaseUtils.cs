using System;
using System.Collections.Generic;
using UnityEditor;

namespace Besttof.ScriptableObjectDrawer.Editor
{
	internal static class AssetDatabaseUtils
	{
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
}