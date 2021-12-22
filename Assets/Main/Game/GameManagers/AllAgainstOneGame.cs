using MPCore;

namespace MPGame
{
	public class AllAgainstOneGame : FreeplayGameController
	{
		protected override void SetTeam(CharacterInfo ci)
		{
			ci.team = _gameModel.currentPlayer.Value == ci ? 0 : 1;
		}
	}
}
