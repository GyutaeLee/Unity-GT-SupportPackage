using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SupportPackage.Sound
{
    public class Effect : MonoBehaviour
    {
        public static Effect Instance { get; private set; }

        private bool isEffectSoundOn;
        private float effectSoundVolume;

        private List<AudioSource> realEffectSoundAudioSources;
        private List<AudioSource> effectSoundAudioSources
        {
            get
            {
                if (this.realEffectSoundAudioSources == null)
                {
                    this.realEffectSoundAudioSources = new List<AudioSource>();
                }
                return this.realEffectSoundAudioSources;
            }
        }

        private Dictionary<string, AudioClip> realEffectSoundAudioClipDictionary;
        private Dictionary<string, AudioClip> effectSoundAudioClipDictionary
        {
            get
            {
                if (this.realEffectSoundAudioClipDictionary == null)
                {
                    this.realEffectSoundAudioClipDictionary = new Dictionary<string, AudioClip>();
                }
                return this.realEffectSoundAudioClipDictionary;
            }
        }


        private void Awake()
        {
            if (Effect.Instance == null)
            {
                Effect.Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void Start()
        {
            this.isEffectSoundOn = SecurityPlayerPrefs.GetBool(Constants.IsEffectSoundOn, true);
            this.effectSoundVolume = SecurityPlayerPrefs.GetFloat(Constants.EffectSoundVolume, 1.0f);
        }

        private int IncreaseAudioPool()
        {
            for (int i = 0; i < Constants.DefualtAudioSourceCount; i++)
            {
                AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
                this.effectSoundAudioSources.Add(audioSource);
            }

            return this.effectSoundAudioSources.Count - Constants.DefualtAudioSourceCount;
        }

        private void DecreaseAudioPool()
        {
            if (this.effectSoundAudioSources.Count <= Constants.DefualtAudioSourceCount)
            {
                Debug.LogError("The number of Effect sound audio sources is less than the default audio source count.");
                return;
            }

            int count = this.effectSoundAudioSources.Count;

            for (int i = 0; i < count; i++)
            {
                if (this.effectSoundAudioSources[i].isPlaying == false)
                {
                    this.effectSoundAudioSources.RemoveAt(i);
                    i--;
                    count--;

                    if (count <= Constants.DefualtAudioSourceCount)
                        break;
                }
            }
        }

        private int GetAvailableAudioSourceIndex()
        {
            for (int i = 0; i < this.effectSoundAudioSources.Count; i++)
            {
                if (this.effectSoundAudioSources[i].isPlaying == false)
                    return i;
            }

            return IncreaseAudioPool();
        }

        /// <summary>
        /// Play effect sound and return sound audio source index.
        /// </summary>
        /// <param name="effectSoundType">  : </param>
        /// <param name="effectSoundIndex"> : </param>
        /// <returns> sound audio source index </returns>
        public int Play(string effectSoundType, int effectSoundIndex)
        {
            if (this.isEffectSoundOn == false)
            {
                Debug.Log("isEffectSoundOn is false");
                return Constants.InvalidIndex;
            }

            string effectSoundResourcePath = GetEffectSoundResourceDirectoryPath(effectSoundType) + effectSoundIndex;
            if (this.effectSoundAudioClipDictionary.ContainsKey(effectSoundResourcePath) == false)
            {
                AudioClip audioClip = Resources.Load<AudioClip>(effectSoundResourcePath);
                if (audioClip == null)
                {
                    Debug.LogError($"{effectSoundType} is does not exist.");
                    return Constants.InvalidIndex;
                }

                this.effectSoundAudioClipDictionary[effectSoundResourcePath] = audioClip;
            }

            int audioSourceIndex = GetAvailableAudioSourceIndex();

            this.effectSoundAudioSources[audioSourceIndex].clip = this.effectSoundAudioClipDictionary[effectSoundResourcePath];
            this.effectSoundAudioSources[audioSourceIndex].Play();
            this.effectSoundAudioSources[audioSourceIndex].volume = effectSoundVolume;

            return audioSourceIndex;
        }

        public int PlayLoop(string effectSoundType, int effectSoundIndex)
        {
            int audioSourceIndex = Play(effectSoundType, effectSoundIndex);
            if (audioSourceIndex == Constants.InvalidIndex)
                return Constants.InvalidIndex;

            this.effectSoundAudioSources[audioSourceIndex].loop = true;

            return audioSourceIndex;
        }

        public void Stop(int audioSourceIndex)
        {
            if (audioSourceIndex == Constants.InvalidIndex || audioSourceIndex < 0 || audioSourceIndex >= this.effectSoundAudioSources.Count)
            {
                Debug.LogError($"audioSourceIndex({audioSourceIndex}) is not in range.");
                return;
            }

            this.effectSoundAudioSources[audioSourceIndex].loop = false;
            this.effectSoundAudioSources[audioSourceIndex].Stop();
        }

        public void PlaySeveralTimes(string effectSoundType, int effectSoundIndex, int times)
        {
            StartCoroutine(CoroutinePlaySoundSeveralTimes(effectSoundType, effectSoundIndex, times));
        }

        private IEnumerator CoroutinePlaySoundSeveralTimes(string effectSoundType, int effectSoundIndex, int times)
        {
            string effectSoundResourcePath = GetEffectSoundResourceDirectoryPath(effectSoundType) + effectSoundIndex;
            if (this.effectSoundAudioClipDictionary.ContainsKey(effectSoundResourcePath) == false)
            {
                AudioClip audioClip = Resources.Load<AudioClip>(effectSoundResourcePath);
                if (audioClip == null)
                {
                    Debug.LogError($"{effectSoundType} is does not exist.");
                    yield break;
                }
                this.effectSoundAudioClipDictionary[effectSoundResourcePath] = audioClip;
            }

            WaitForSeconds WFS = new WaitForSeconds(this.effectSoundAudioClipDictionary[effectSoundResourcePath].length);

            for (int i = 0; i < times; i++)
            {
                Play(effectSoundType, effectSoundIndex);
                yield return WFS;
            }
        }

        private string GetEffectSoundResourceDirectoryPath(string effectSoundType)
        {
            string soundResourceDirectoryPath = "Sound/Effect/" + effectSoundType.ToString() + "/";
            return soundResourceDirectoryPath;
        }

        public void UpdateEffectSoundVolume()
        {
            this.effectSoundVolume = SecurityPlayerPrefs.GetFloat(Constants.EffectSoundVolume, 1.0f);

            foreach (var effectSoundAudioSource in this.effectSoundAudioSources)
            {
                effectSoundAudioSource.volume = this.effectSoundVolume;
            }
        }
    }
}