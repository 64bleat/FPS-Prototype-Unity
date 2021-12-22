using UnityEngine;

namespace MPCore
{
	/// <summary>
	/// Sets the fov of a camera
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public class CameraFOV : MonoBehaviour
	{
		[SerializeField] bool _firstPersonFov = false;

		GraphicsModel _model;

		void Awake()
		{
			_model = Models.GetModel<GraphicsModel>();

			if (_firstPersonFov)
				_model.fovFirstPerson.Subscribe(OnFovChange);
			else
				_model.fov.Subscribe(OnFovChange);
		}

		void OnDestroy()
		{
			if (_firstPersonFov)
				_model.fovFirstPerson.Subscribe(OnFovChange);
			else
				_model.fov.Subscribe(OnFovChange);
		}

		void OnFovChange(DeltaValue<float> fov)
		{
			Camera camera = GetComponent<Camera>();
			camera.fieldOfView = fov.newValue;
		}
	}
}
