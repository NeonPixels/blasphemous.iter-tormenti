
using Framework.Managers;
using IterTormenti.Utils.Audio;

namespace IterTormenti.Esdras
{
    class EsdrasAudioPlayer : AudioPlayer
    {
        //private const string ESDRAS_DEATH = "EsdrasDeath";
		
        public const string ESDRAS_NORMAL_ATTACK = "EsdrasNormalAttack";
		public const string ESDRAS_GROUND_HIT = "EsdrasGroundHit";
		public const string ESDRAS_FOOTSTEP = "EsdrasRun";
		
        //private const string ESDRAS_HEAVY_ATTACK_SMASH = "EsdrasThunderAttack";
		//public const string ESDRAS_SLIDE_ATTACK = "EsdrasSlideAttack";
		//public const string ESDRAS_SPIN_LOOP = "EsdrasSpinLoop";
		//public const string ESDRAS_SPINPROJECTILE = "EsdrasSpinProjectile";
		//public const string ESDRAS_CALL_SISTER = "EsdrasPerpetuaCall";

        private const string ESDRAS_BOSS_HEADER = "event:/SFX/Bosses/Esdras/";

        public const string ESDRAS_GET_UP = "EsdrasGetup";

        public const string ESDRAS_DROP_WEAPON = "EsdrasDropWeapon";

        private const string ESDRAS_NPC_HEADER = "event:/SFX/NPC/";

        public EsdrasAudioPlayer()
        { }

        public override void Play(string name)
        {
            if(ESDRAS_NORMAL_ATTACK.Equals(name))
            {
                Core.Audio.PlaySfx(ESDRAS_BOSS_HEADER + ESDRAS_NORMAL_ATTACK);
                return;
            }

            if(ESDRAS_GROUND_HIT.Equals(name))
            {
                Core.Audio.PlaySfx(ESDRAS_BOSS_HEADER + ESDRAS_GROUND_HIT);
                return;
            }

            if(ESDRAS_FOOTSTEP.Equals(name))
            {
                Core.Audio.PlaySfx(ESDRAS_BOSS_HEADER + ESDRAS_FOOTSTEP);
                return;
            }

            if(ESDRAS_DROP_WEAPON.Equals(name))
            {
                Core.Audio.PlaySfx(ESDRAS_NPC_HEADER + ESDRAS_DROP_WEAPON);
                return;
            }

            if(ESDRAS_GET_UP.Equals(name))
            {
                Core.Audio.PlaySfx(ESDRAS_NPC_HEADER + ESDRAS_GET_UP);
                return;
            }

            Main.IterTormenti.LogError($"EsdrasAudioPlayer::Play: ERROR: Unknown audio: {name}");
        }
    }
}