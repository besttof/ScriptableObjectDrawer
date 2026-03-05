using UnityEngine;

namespace Example
{
	public class BaseData<T> : ScriptableObject
	{
		[SerializeField] private T _genericProperty;
	}
}