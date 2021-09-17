using UnityEngine;

namespace MPCore
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] Camera _mainCamera;
        [SerializeField] Camera _farCamera;
        [SerializeField] Camera _guiCamera;

        GameModel _gameModel;
        Transform _view;
        GameObject _skycam;

        void Awake()
        {
            _gameModel = Models.GetModel<GameModel>();
            _gameModel.currentView.Subscribe(SetView);
            _skycam = GameObject.FindGameObjectWithTag("SkyCam");
        }

        void OnDestroy()
        {
            _gameModel.currentView.Unsubscribe(SetView);
        }

        void LateUpdate()
        {
            transform.position = _view.position;
            transform.rotation = _view.rotation;

            if(_skycam)
                _skycam.transform.rotation = _view.transform.rotation;
        }

        void SetView(DeltaValue<Transform> view)
        {
            _view = view.newValue;
            enabled = _view != null;
        }
    }
}
