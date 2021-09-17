using MPCore;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
namespace MPWorld
{
    /// <summary>
    /// An interactive lever representing a boolean value.
    /// </summary>
    public class BoolLever : MonoBehaviour, IInteractable
    {
        public DataValue<bool> dataValue = new DataValue<bool>();
        public enum ButtonValueType { Manual, Toggle, Hold }
        [SerializeField] ButtonValueType buttonType = ButtonValueType.Toggle;
        [SerializeField] GameObject lever;
        [SerializeField] Transform upPosition;
        [SerializeField] Transform DownPosition;
        [SerializeField] float transitionTime = 0.3f;
        [SerializeField] bool commitToPress = false;
        [SerializeField] Material[] OnMaterials;
        [SerializeField] Material[] offMaterials;
        [SerializeField] UnityEvent downEvents;

        private MeshRenderer _meshRenderer;
        Coroutine _coroutine;
        DataValue<float> _lerp = new DataValue<float>(1f);

        public void Awake()
        {
            _meshRenderer = lever.GetComponent<MeshRenderer>();
            dataValue.Subscribe(dv => downEvents.Invoke());
            dataValue.Subscribe(SetMaterial);
            _lerp.Subscribe(SetPosition);
        }

        IEnumerator ButtonUp(float transitionTime)
        {
            while(_lerp.Value < 1)
            {
                _lerp.Value = Mathf.MoveTowards(_lerp.Value, 1f, 1f / transitionTime * Time.deltaTime);
                yield return null;
            }
        }

        IEnumerator ButtonDown()
        {
            while(_lerp.Value > 0)
            {
                _lerp.Value = Mathf.MoveTowards(_lerp.Value, 0f, 1f / transitionTime * Time.deltaTime);
                yield return null;
            }

            if (buttonType == ButtonValueType.Toggle)
                dataValue.Value = !dataValue.Value;
        }

        void SetMaterial(DeltaValue<bool> value)
        {
            _meshRenderer.materials = value.newValue ? OnMaterials : offMaterials;
        }

        void SetPosition(DeltaValue<float> lerp)
        {
            lever.transform.position = Vector3.Lerp(DownPosition.position, upPosition.position, lerp.newValue);
        }

        public void SetValue(bool value)
        {
            dataValue.Value = value;
        }

        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(ButtonDown());
        }
        public void OnInteractHold(GameObject other, RaycastHit hit) { }
        public void OnInteractEnd(GameObject other, RaycastHit hit)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(ButtonUp(transitionTime));
        }
    }
}