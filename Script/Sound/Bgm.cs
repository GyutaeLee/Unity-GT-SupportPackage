using UnityEngine;

namespace SupportPackage.Sound
{
    public static class Constants
    {
        public const string IsBgmOn = "IsBgmOn";
        public const string IsEffectSoundOn = "IsEffectOn";

        public const string BgmVolume = "BgmVolume";
        public const string EffectSoundVolume = "EffectVolume";

        public const string LanguageType = "LanguageType";

        public const int InvalidIndex = -1;
        public const int DefualtAudioSourceCount = 4;
    }

    public class Bgm : MonoBehaviour
    {

        public static Bgm Instance { get; private set; }

        private bool isBgmOn;
        private float bgmVolume;

        private AudioClip bgmAudioClip;

        private AudioSource realBgmAudioSource;
        private AudioSource bgmAudioSource
        {
            get
            {
                if (this.realBgmAudioSource == null)
                {
                    this.realBgmAudioSource = this.gameObject.AddComponent<AudioSource>();
                    this.realBgmAudioSource.loop = true;
                    this.realBgmAudioSource.volume = this.bgmVolume;
                }

                return this.realBgmAudioSource;
            }
        }

        private void Awake()
        {
            if (Bgm.Instance == null)
            {
                Bgm.Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void Start()
        {
            this.isBgmOn = SecurityPlayerPrefs.GetBool(Constants.IsBgmOn, true);
            this.bgmVolume = SecurityPlayerPrefs.GetFloat(Constants.BgmVolume, 1.0f);
        }

        public bool Play(string bgmType, int bgmIndex)
        {
            string bgmResourceName = GetBgmResourceDirectoryPath(bgmType) + bgmIndex;
            AudioClip bgmAudioClip = Resources.Load<AudioClip>(bgmResourceName);

            if (bgmAudioClip == null)
            {
                Debug.Log("There is no Bgm audio clip.");
                return false;
            }

            if (this.bgmAudioSource.clip == bgmAudioClip && this.bgmAudioSource.isPlaying == true)
            {
                Debug.Log("The same Bgm audio clip is already playing.");
                return false;
            }

            this.bgmAudioClip = bgmAudioClip;
            this.bgmAudioSource.clip = this.bgmAudioClip;

            if (this.isBgmOn == false)
            {
                Stop();
            }
            else
            {
                this.bgmAudioSource.volume = this.bgmVolume;
                this.bgmAudioSource.Play();
            }

            return true;
        }

        public void Stop()
        {
            this.bgmAudioSource.Stop();
        }

        public void Pause()
        {
            this.bgmAudioSource.Pause();
        }

        public void Resume()
        {
            this.bgmAudioSource.Play();
        }

        public void SetIsBgmOn(bool isBgmOn)
        {
            if (this.isBgmOn == isBgmOn)
                return;

            if (isBgmOn == true)
            {
                this.bgmAudioSource.volume = this.bgmVolume;
                this.bgmAudioSource.Play();
            }
            else
            {
                this.bgmAudioSource.volume = 0;
                this.bgmAudioSource.Stop();
            }

            this.isBgmOn = isBgmOn;
        }

        public void SetVolume(float bgmVolume)
        {
            this.bgmAudioSource.volume = bgmVolume;
        }

        private string GetBgmResourceDirectoryPath(string bgmType)
        {
            string bgmResourceDirectoryPath = "Sound/Bgm/" + bgmType + "/";
            return bgmResourceDirectoryPath;
        }
    }
}