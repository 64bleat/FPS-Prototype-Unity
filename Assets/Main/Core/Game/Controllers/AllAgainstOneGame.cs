namespace MPCore
{
    public class AllAgainstOneGame : FreeplayGameController
    {
        protected override void SetTeam(CharacterInfo ci)
        {
            ci.team = _currentPlayerInfo == ci ? 0 : 1;
        }
    }
}
