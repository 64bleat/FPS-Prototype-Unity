using MPGUI;
using UnityEngine;

namespace MPCore
{
	public class GraphicsViewModel : MonoBehaviour
	{
		[SerializeField] FloatField _fov;
		[SerializeField] FloatField _fovClose;
		[SerializeField] BoolField _bloom;
		[SerializeField] IntField _iterations;

		GraphicsModel _graphicsModel;

		void Awake()
		{
			_graphicsModel = Models.GetModel<GraphicsModel>();

			_fov.SetReference(_graphicsModel.fov, nameof(_graphicsModel.fov.Value), "FOV");
			_fovClose.SetReference(_graphicsModel.fovFirstPerson, nameof(_graphicsModel.fovFirstPerson.Value), "Weapon FOV");
			_bloom.Initialize(_graphicsModel.bloomEnabled.Value, "Bloom");
			_bloom.value.Subscribe(dv => _graphicsModel.bloomEnabled.Value = dv.newValue);
			_iterations.SetReference(_graphicsModel, nameof(_graphicsModel.iterations), "Bloom Iterations");
		}
	}
}
