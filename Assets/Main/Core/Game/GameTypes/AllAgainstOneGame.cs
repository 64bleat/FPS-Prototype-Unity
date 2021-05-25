namespace MPCore
{
    public class AllAgainstOneGame : FreeplayGame
    {
        protected override void SetTeam(CharacterInfo ci)
        {
            ci.team = loadedPlayerInfo == ci ? 0 : 1;
        }
    }
}
