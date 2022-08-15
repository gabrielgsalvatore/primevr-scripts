using FistVR;

namespace PrimeVrScripts
{
    public class LematCapBallHammerSwitch : FVRInteractiveObject
    {

		public SingleActionCapBallRevolver revolver;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
		public override void SimpleInteraction(FVRViveHand hand)
		{
			base.SimpleInteraction(hand);
			this.revolver.SwitchFireMode();
		}
#endif
	}
}
