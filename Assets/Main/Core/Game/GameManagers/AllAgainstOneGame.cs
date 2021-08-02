namespace MPCore
{
    public class AllAgainstOneGame : FreeplayGameController
    {
        protected override void SetTeam(CharacterInfo ci)
        {
            ci.team = loadedPlayerInfo == ci ? 0 : 1;
        }
    }
}
