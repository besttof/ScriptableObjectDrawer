using UnityEngine;

namespace Example
{
	public class Data : BaseData<string>
	{
		[Space(100)]
		[SerializeField] private BaseData<string> _data;

		[Space(200)]
		[SerializeField] private Data _nestedData;

		[SerializeField] private ALotOfData _aLotOfData;

		[Header("Normal properties")]
		[SerializeField] private string _title;

		[Range(0f, 5f)]
		[SerializeField] private float _range;
	}
}