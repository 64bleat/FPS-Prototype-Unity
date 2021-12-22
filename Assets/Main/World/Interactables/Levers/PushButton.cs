using MPCore;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
namespace MPWorld
{
	/// <summary>
	/// An interactive lever representing a boolean value.
	/// </summary>
	public class PushButton : MonoBehaviour, IInteractable
	{
		public enum ButtonValueType { Manual, Toggle, Hold }

		public DataValue<bool> dataValue = new DataValue<bool>();
		[SerializeField] ButtonValueType _buttonType = ButtonValueType.Toggle;
		[SerializeField] Transform _buttonModel;
		[SerializeField] Transform _upPosition;
		[SerializeField] Transform _DownPosition;
		[SerializeField] float _transitionTime = 0.3f;
		[SerializeField] bool _commitToPress = false;
		[SerializeField] Material[] _onMaterials;
		[SerializeField] Material[] _offMaterials;
		public UnityEvent OnPressed;

		MeshRenderer _meshRenderer;
		Coroutine _moveCoroutine;
		Coroutine _waitCoroutine;
		DataValue<float> _lerp = new(1f);
		bool _down = false;

		public void Awake()
		{
			_meshRenderer = _buttonModel.GetComponent<MeshRenderer>();
			dataValue.Subscribe(value => 
				_meshRenderer.materials = value.newValue ? _onMaterials : _offMaterials);
			_lerp.Subscribe(lerp => 
				_buttonModel.transform.position = Vector3.Lerp(_DownPosition.position, _upPosition.position, lerp.newValue));
		}

		IEnumerator ButtonDown()
		{
			while (_lerp.Value > 0)
			{
				_lerp.Value = Mathf.MoveTowards(_lerp.Value, 0f, 1f / _transitionTime * Time.deltaTime);
				yield return null;
			}

			OnPressed?.Invoke();
			_down = true;

			if (_buttonType == ButtonValueType.Toggle)
				dataValue.Value = !dataValue.Value;
			else if (_buttonType == ButtonValueType.Hold)
				dataValue.Value = true;
		}

		IEnumerator WaitButtonUp()
		{
			if (_commitToPress)
				yield return new WaitUntil(() => _down);

			if (_moveCoroutine != null)
				StopCoroutine(_moveCoroutine);

			_moveCoroutine = StartCoroutine(ButtonUp());
		}

		IEnumerator ButtonUp()
		{
			_down = false;

			if (_buttonType == ButtonValueType.Hold)
				dataValue.Value = false;

			while(_lerp.Value < 1)
			{
				_lerp.Value = Mathf.MoveTowards(_lerp.Value, 1f, 1f / _transitionTime * Time.deltaTime);
				yield return null;
			}
		}

		public void OnInteractStart(GameObject other, RaycastHit hit)
		{
			if (_moveCoroutine != null)
				StopCoroutine(_moveCoroutine);

			_moveCoroutine = StartCoroutine(ButtonDown());
		}
		public void OnInteractHold(GameObject other, RaycastHit hit) { }
		public void OnInteractEnd(GameObject other, RaycastHit hit)
		{
			if (_waitCoroutine != null)
				StopCoroutine(_waitCoroutine);

			_waitCoroutine = StartCoroutine(WaitButtonUp());
		}
	}
}