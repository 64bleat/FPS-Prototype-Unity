using UnityEngine;
using MPCore;

namespace MPWorld
{ 
	public class FloatLever : MonoBehaviour, IInteractable
	{
		public Transform lever;
		public Transform leverMin;
		public Transform leverMax;
		public float restingValue = 0f;
		public float decayDelay = 0f;
		public float rateOfDecay = 0f;
		public DataValue<float> dataValue = new();

		float _lastInteractTime = 0;
		float _maxMag;

		public float Value
		{
			get => dataValue.Value;

			set
			{
				value = Mathf.Clamp01(value);
				dataValue.Value = value;
			}
		}

		void Awake()
		{
			_maxMag = (leverMax.position - leverMin.position).magnitude;

			dataValue.Subscribe(value =>
				lever.position = Vector3.Lerp(leverMin.position, leverMax.position, value.newValue));
		}

		void Start()
		{

			PositionToValue(lever.position);
		}

		void Update()
		{
			if (rateOfDecay > 0 && Time.time - _lastInteractTime > decayDelay)
				Value = Mathf.Lerp(Value, restingValue + (Value - restingValue) * 0.5f, Time.deltaTime / rateOfDecay);
		}

		void PositionToValue(Vector3 point)
		{
			Vector3 direction = leverMax.position - leverMin.position;
			Vector3 offset = point - leverMin.position;

			Value = Vector3.Project(offset, direction).magnitude / _maxMag * Mathf.Sign(Vector3.Dot(offset, direction));
		}

		public void OnInteractEnd(GameObject other, RaycastHit hit) { }
		public void OnInteractStart(GameObject other, RaycastHit hit) { }
		public void OnInteractHold(GameObject other, RaycastHit hit)
		{
			{   // Project on the lever plane
				Interactor interactor = other.GetComponentInChildren<Interactor>();
				Vector3 interactDirection = (hit.point - interactor.gameObject.transform.position).normalized;
				float a = Vector3.Dot(leverMin.position - interactor.gameObject.transform.position, leverMin.transform.up);
				float b = Vector3.Dot(interactDirection, leverMin.transform.up);

				if (b != 0 && a != 0)
					hit.point = interactor.gameObject.transform.position + interactDirection * a / b;
			}

			PositionToValue(hit.point);

			_lastInteractTime = Time.time;
		}
	}
}