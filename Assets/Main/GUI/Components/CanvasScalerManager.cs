using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
	[RequireComponent(typeof(CanvasScaler))]
	public class CanvasScalerManager : MonoBehaviour
	{
		GraphicsModel _graphicsModel;
		CanvasScaler _scaler;

		void Awake()
		{
			_graphicsModel = Models.GetModel<GraphicsModel>();
			_scaler = GetComponent<CanvasScaler>();

			_graphicsModel.canvasScale.Subscribe(SetScale);
		}

		void OnDestroy()
		{
			_graphicsModel.canvasScale.Unsubscribe(SetScale);
		}

		void SetScale(DeltaValue<float> scale)
		{
			_scaler.scaleFactor = scale.newValue;
		}
	}
}
