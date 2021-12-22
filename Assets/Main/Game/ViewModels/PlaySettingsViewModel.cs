using MPCore;
using MPGame;
using System.Linq;
using UnityEngine;

namespace MPGUI
{
	public class PlaySettingsViewModel : MonoBehaviour
	{
		[SerializeField] IntField _botCount;
		[SerializeField] CaseSelectionDropdown _case;

		PlaySettingsModel _playModel;
		
		private void Awake()
		{
			_playModel = Models.GetModel<PlaySettingsModel>();

			_botCount.SetReference(_playModel, nameof(_playModel.botCount));
			_case.SetReference(_playModel, nameof(_playModel.scene));
			_case.AddOptions(ResourceLoader.GetResources<Case>().OrderBy(c => c.displayName));
		}
	}
}
