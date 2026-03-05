using UnityEngine;

namespace Example
{
	public class ALotOfData : ScriptableObject
	{
		[SerializeField] private string _title;

		[Range(0f, 1f)]
		[SerializeField] private float[] _ranges = new float[20];

		[SerializeField] private string[] _names = new string[20];
	}
}