using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Steps.StdLib;
using UnityEngine;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    [CreateAssetMenu(menuName = "Dora/Samples/PlayerLocationStep")]
    public class PlayerLocationStep : LocationStep
    {
        protected override bool ProcessEvent(IGameplayEvent e, ref State _)
        {
            if (e is MemberEnteredLocationEvent member)
            {
                if (member.Member != DoraPlayerManager.Current.mainActor)
                    return false;
                
            }
            
            return base.ProcessEvent(e, ref _);
        }
        
        //
        // No need to override 'CanProcess' here,
        // as MemberEnteredLocationEvent inherits from EnteredLocationEvent, and thus passes the ckeck
        //
    }
}