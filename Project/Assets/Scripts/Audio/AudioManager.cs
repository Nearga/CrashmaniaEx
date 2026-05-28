using UnityEngine;

namespace Crashmania.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        private const int SfxPoolSize = 5;

        private AudioSource musicSource;
        private AudioSource[] sfxSources;

        public static AudioManager Ensure()
        {
            var existing = FindAnyObjectByType<AudioManager>();
            if (existing != null)
            {
                return existing;
            }

            var root = new GameObject("[AudioManager]");
            DontDestroyOnLoad(root);
            return root.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            sfxSources = new AudioSource[SfxPoolSize];
            for (var index = 0; index < sfxSources.Length; index++)
            {
                sfxSources[index] = gameObject.AddComponent<AudioSource>();
                sfxSources[index].playOnAwake = false;
            }
        }
    }
}
