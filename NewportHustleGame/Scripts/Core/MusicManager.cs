using System.Linq;
using UnityEngine;

namespace NewportHustle.Core
{
    /// <summary>
    /// Simple global music manager so the Music volume slider only affects music AudioSources.
    /// Attach this to a persistent GameObject (for example in the main menu scene) and tag all
    /// music-only AudioSources with the "Music" tag in Unity.
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        private AudioSource[] musicSources;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                RefreshMusicSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Refresh the cached list of music AudioSources (tagged "Music").
        /// Call this if you load a new scene with additional music sources.
        /// </summary>
        public void RefreshMusicSources()
        {
            var musicObjects = GameObject.FindGameObjectsWithTag("Music");
            musicSources = musicObjects
                .Select(go => go.GetComponent<AudioSource>())
                .Where(src => src != null)
                .ToArray();

            // Apply current music volume to any newly found sources
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            ApplyMusicVolume(musicVolume);
        }

        /// <summary>
        /// Apply a 0-1 volume just to music sources, ignoring SFX.
        /// </summary>
        public void ApplyMusicVolume(float volume)
        {
            if (musicSources == null) return;

            foreach (var source in musicSources)
            {
                if (source != null)
                {
                    source.volume = volume;
                }
            }
        }
    }
}
