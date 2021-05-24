using UnityEngine;

namespace AssemblyCSharp.Assets.Code.UIComponents.Constants
{
    public static class AnimatorParameters
    {
        #region Model Animations
            
        public static readonly int SummoningTrigger = Animator.StringToHash("SummoningTrigger");
        public static readonly int DeathTrigger = Animator.StringToHash("Death");
        public static readonly int FusionTrigger = Animator.StringToHash("FusionSummon");
        public static readonly int ActivateSpellOrTrapTrigger = Animator.StringToHash("ActivateSpellTrap");
        public static readonly int ReturnSpellTrapToFaceDown = Animator.StringToHash("ReturnToFaceDown");
        public static readonly int RemoveSetCardTrigger = Animator.StringToHash("RemoveSetCard");
        public static readonly int RevealSetMonsterTrigger = Animator.StringToHash("RevealSetMonster");
        public static readonly int HideSetMonsterImageTrigger = Animator.StringToHash("HideSetMonsterImage");
        public static readonly int PlayMonsterAttack1Trigger = Animator.StringToHash("Attack1");
        public static readonly int PickUpPlayfieldTrigger = Animator.StringToHash("PickUpPlayfield");
        public static readonly int FadeInSetCardTrigger = Animator.StringToHash("FadeIn");

        public static readonly int DefenceBool = Animator.StringToHash("IsDefence");
        public static readonly int RemoveCardBool = Animator.StringToHash("RemoveCard");
        public static readonly int RotatePlatformBool = Animator.StringToHash("Rotate");
        public static readonly int MainMenuRotateBool = Animator.StringToHash("MainMenuRotate");

        public static readonly int PlatformDoubleSpeedFloat = Animator.StringToHash("DoubleSpeed");

        #endregion

        #region UI Animations

        public static readonly int ActivatePlayfieldTrigger = Animator.StringToHash("ActivatePlayfield");
        public static readonly int RemovePlayfieldTrigger = Animator.StringToHash("RemovePlayfield");

        public static readonly int OpenPlayfieldMenuBool = Animator.StringToHash("OpenPlayfieldMenu");
        public static readonly int PrefabMenuBool = Animator.StringToHash("PrefabMenuEnter");
        
        #endregion
    }
}
