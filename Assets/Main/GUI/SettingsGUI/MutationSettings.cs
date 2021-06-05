using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPCore;

namespace MPGUI
{
    public class MutationSettings : MonoBehaviour
    {
        [SerializeField] private MutationList mutationList;
        [SerializeField] private ButtonSet available;
        [SerializeField] private ButtonSet selected;

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            available.Clear();
            selected.Clear();

            foreach (Mutator mut in mutationList.available)
                if (!mutationList.selection.Contains(mut))
                {
                    GameObject go = available.AddButton(mut.displayName, () => AddMutator(mut));

                    if (go.TryGetComponent(out HoverHelp hh))
                        hh.SetText(mut.description);
                }

            foreach (Mutator mut in mutationList.selection)
            {
                GameObject go = selected.AddButton(mut.displayName, () => RemoveMutator(mut));

                if (go.TryGetComponent(out HoverHelp hh))
                    hh.SetText(mut.description);
            }
        }

        private void AddMutator(Mutator mut)
        {
            mutationList.selection.Add(mut);

            Refresh();
        }

        private void RemoveMutator(Mutator mut)
        {
            mutationList.selection.Remove(mut);

            Refresh();
        }
    }
}
