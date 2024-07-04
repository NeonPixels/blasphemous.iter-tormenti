
using Framework.Managers;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Audio;
using IterTormenti.Utils.Audio;

namespace IterTormenti.Esdras
{
    class EsdrasAudioPlayer : AudioPlayer
    {
        private const string ESDRAS_DEATH = "EsdrasDeath";
		private const string ESDRAS_NORMAL_ATTACK = "EsdrasNormalAttack";
		private const string ESDRAS_GROUND_HIT = "EsdrasGroundHit";
		private const string ESDRAS_FOOTSTEP = "EsdrasRun";
		private const string ESDRAS_HEAVY_ATTACK_SMASH = "EsdrasThunderAttack";
		public const string ESDRAS_SLIDE_ATTACK = "EsdrasSlideAttack";
		public const string ESDRAS_SPIN_LOOP = "EsdrasSpinLoop";
		public const string ESDRAS_SPINPROJECTILE = "EsdrasSpinProjectile";
		public const string ESDRAS_CALL_SISTER = "EsdrasPerpetuaCall";

        private const string ESDRAS_BOSS_HEADER = "event:/SFX/Bosses/Esdras/";

        public EsdrasAudioPlayer()
        { }

        public override void Play(string name)
        {
            Main.IterTormenti.Log($"EsdrasAudioPlayer::Play: {name}");

            if(ESDRAS_NORMAL_ATTACK.Equals(name))
            {
                //EsdrasAudio.PlayLightAttack_AUDIO(); //TODO: Use material properties from this function?
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

            // if(name.Equals("EsdrasGetup"))
            // {
            //     Core.Audio.PlaySfx("event:/SFX/NPC/EsdrasGetup"); //TODO: Add constants. Will this specific one be used?
            //     return;
            // }

            Main.IterTormenti.Log($"EsdrasAudioPlayer::Play: Unknown audio: {name}");
        }
    }
}