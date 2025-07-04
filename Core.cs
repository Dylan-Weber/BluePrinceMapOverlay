using MelonLoader;

[assembly: MelonInfo(typeof(BluePrinceMapOverlay.Core), "BluePrinceMapOverlay", "1.0.0", "dylan", null)]
[assembly: MelonGame("Dogubomb", "BLUE PRINCE")]

namespace BluePrinceMapOverlay
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
    }
}