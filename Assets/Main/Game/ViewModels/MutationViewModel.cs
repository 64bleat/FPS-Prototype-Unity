using UnityEngine;
using MPCore;
using System.Collections;
using System.Linq;

namespace MPGUI
{
    public class MutationViewModel : MonoBehaviour
    {
        [SerializeField] private ButtonSet available;
        [SerializeField] private ButtonSet selected;

        private PlaySettingsModel _selection;
        private IOrderedEnumerable<Mutator> _availableMutators;

        private void Awake()
        {
            _selection = Models.GetModel<PlaySettingsModel>();
            _availableMutators = ResourceLoader.GetResources<Mutator>().OrderBy(mut => mut.displayName);
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            available.Clear();
            selected.Clear();

            foreach (Mutator mut in _availableMutators)
                if (!_selection.mutators.Contains(mut))
                {
                    GameObject go = available.AddButton(mut.displayName, () => AddMutator(mut));

                    if (go.TryGetComponent(out HoverHelp hh))
                        hh.SetText(mut.description);
                }

            foreach (Mutator mut in _selection.mutators)
            {
                GameObject go = selected.AddButton(mut.displayName, () => RemoveMutator(mut));

                if (go.TryGetComponent(out HoverHelp hh))
                    hh.SetText(mut.description);
            }
        }

        private void AddMutator(Mutator mut)
        {
            _selection.mutators.Add(mut);

            Refresh();
        }

        private void RemoveMutator(Mutator mut)
        {
            _selection.mutators.Remove(mut);

            Refresh();
        }
    }
}
