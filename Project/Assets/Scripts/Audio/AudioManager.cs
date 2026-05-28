using UnityEngine;

namespace Crashmania.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        private const int SfxPoolSize = 5;

        private AudioSource musicSource;
        private AudioSource[] sfxSources;
        private int nextSfxSource;
        private bool musicMuted;
        private bool sfxMuted;

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
            EnsureInitialized();
        }

        public void PlayMusic(AudioClip clip, float volume = 1f)
        {
            EnsureInitialized();
            musicSource.clip = clip;
            musicSource.volume = musicMuted ? 0f : volume;
            if (clip != null)
            {
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            EnsureInitialized();
            musicSource.Stop();
            musicSource.clip = null;
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxMuted)
            {
                return;
            }

            EnsureInitialized();
            var source = sfxSources[nextSfxSource];
            nextSfxSource = (nextSfxSource + 1) % sfxSources.Length;
            source.PlayOneShot(clip, volume);
        }

        public void SetMusicMuted(bool muted)
        {
            EnsureInitialized();
            musicMuted = muted;
            musicSource.mute = muted;
        }

        public void SetSfxMuted(bool muted)
        {
            EnsureInitialized();
            sfxMuted = muted;
            foreach (var source in sfxSources)
            {
                source.mute = muted;
            }
        }

        private void EnsureInitialized()
        {
            if (musicSource != null && sfxSources != null)
            {
                return;
            }

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
