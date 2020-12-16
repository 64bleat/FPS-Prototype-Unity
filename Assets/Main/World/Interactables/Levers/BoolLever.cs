using MPCore;
using UnityEngine;
using UnityEngine.Events;
namespace MPWorld
{

    public class BoolLever : MonoBehaviour, IBoolValue, IInteractable
    {
        public enum ButtonValueType { Manual, Toggle, Hold }
        public GameObject lever;
        public Transform upPosition;
        public Transform DownPosition;
        public ButtonValueType buttonType = ButtonValueType.Toggle;
        public float transitionTime = 0.3f;
        public bool defaultPosition = false;
        public bool commitToPress = false;

        public Material[] OnMaterials;
        public Material[] offMaterials;

        public UnityEvent downEvents;

        private MeshRenderer meshRenderer;
        private bool boolValue;
        private bool isPressed = false;
        private bool debounce = false;
        private bool commit = false;
        private float transition = 1f;

        public bool BoolValue
        {
            get => boolValue;

            set
            {
                boolValue = value;

                if (meshRenderer)
                    meshRenderer.materials = boolValue ? OnMaterials : offMaterials;
            }
        }

        public void Awake()
        {
            meshRenderer = lever.GetComponent<MeshRenderer>();
            BoolValue = defaultPosition;
        }

        public void Update()
        {
            if (commit || isPressed)
            {
                if (transitionTime > 0)
                    transition = Mathf.Max(0, transition - 1f / transitionTime * Time.deltaTime);
                else
                    transition = 0;
            }
            else
            {
                if (transitionTime > 0)
                    transition = Mathf.Min(1, transition + 1f / transitionTime * Time.deltaTime);
                else
                    transition = 1;
            }

            if (!debounce && transition <= 0)
            {
                commit = false;
                debounce = true;

                if (buttonType == ButtonValueType.Toggle)
                    BoolValue = !BoolValue;

                downEvents.Invoke();
            }

            lever.transform.position = Vector3.Lerp(DownPosition.position, upPosition.position, transition);
        }

        public void OnInteractStart(GameObject other, RaycastHit hit)
        {
            isPressed = true;
            debounce = false;

            if (commitToPress)
                commit = true;
        }
        public void OnInteractHold(GameObject other, RaycastHit hit) { }
        public void OnInteractEnd(GameObject other, RaycastHit hit)
        {
            isPressed = false;
        }
    }
}