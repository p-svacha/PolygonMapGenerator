using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Settings")]
        public int OneShotPoolSize = 12;
        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 0.5f;
        [Range(0f, 1f)] public float SfxVolume = 1f;

        [Header("Music")]
        public AudioClip[] AmbientTracks;
        public AudioClip ElectionTrack;
        public float MusicCrossfadeDuration = 2f;

        [Header("SFX")]
        public AudioClip ButtonClick;
        public AudioClip ButtonClick_NoEffect;
        public AudioClip Chimes;
        public AudioClip Ding;
        public AudioClip DiceClack;
        public AudioClip AddBot;
        public AudioClip RemoveBot;
        public AudioClip StartGame;
        public AudioClip WinGame;
        public AudioClip LoseGame;
        public AudioClip Gong;
        public AudioClip Woosh;
        public AudioClip Error;
        public AudioClip Swoosh;
        public AudioClip Lever;
        public AudioClip Heartbeat;
        public AudioClip HeartbeatEnd;

        public AudioClip NewspaperSpin;
        public AudioClip NewspaperSpinEnd;
        public AudioClip NewspaperRustle1;
        public AudioClip NewspaperRustle2;

        public AudioClip GraphAnimationSound;

        // Music
        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private bool musicSourceAActive = true;
        private int currentTrackIndex = -1;
        private Coroutine crossfadeCoroutine;

        // One-shot pool
        private AudioSource[] oneShotPool;
        private int nextOneShotIndex;

        // Charging sounds
        private Dictionary<string, ChargingSound> chargingSounds = new Dictionary<string, ChargingSound>();

        // General
        private static float sfxSpeedModifier = 1f;
        private int volumeLevel = 3; // 0 = mute, 1, 2, 3 = full
        private static readonly float[] volumeLevelValues = { 0f, 0.33f, 0.66f, 1f };
        public static int VolumeLevel => Instance != null ? Instance.volumeLevel : 0;

        private class ChargingSound
        {
            public AudioSource Source;
            public float BasePitch;
            public float MaxPitch;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitMusicSources();
            InitOneShotPool();
        }

        private void InitMusicSources()
        {
            musicSourceA = gameObject.AddComponent<AudioSource>();
            musicSourceA.loop = false;
            musicSourceA.playOnAwake = false;
            musicSourceA.volume = 0f;

            musicSourceB = gameObject.AddComponent<AudioSource>();
            musicSourceB.loop = false;
            musicSourceB.playOnAwake = false;
            musicSourceB.volume = 0f;
        }

        private void InitOneShotPool()
        {
            oneShotPool = new AudioSource[OneShotPoolSize];
            for (int i = 0; i < OneShotPoolSize; i++)
            {
                oneShotPool[i] = gameObject.AddComponent<AudioSource>();
                oneShotPool[i].playOnAwake = false;
            }
        }

        void Update()
        {
            // Auto-advance to next track only when playing standard ambient
            if (!isPlayingSpecialTrack)
            {
                AudioSource activeMusic = musicSourceAActive ? musicSourceA : musicSourceB;
                if (activeMusic.clip != null && !activeMusic.isPlaying && crossfadeCoroutine == null)
                {
                    PlayNextTrack();
                }
            }
        }

        public static void SetSfxSpeedModifier(float speed)
        {
            sfxSpeedModifier = speed;
        }

        // ==================== ONE-SHOT SOUNDS ====================

        /// <summary>
        /// Play a sound effect once. Supports overlapping.
        /// </summary>
        public static void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f, bool applySpeedModifier = false)
        {
            // Debug.Log($"PlaySound: {clip?.name} (vol={volume}, pitch={pitch})");
            if (Instance == null || clip == null || IsMuted) return;

            AudioSource source = Instance.GetNextOneShotSource();
            source.clip = clip;
            source.volume = volume * Instance.SfxVolume * Instance.MasterVolume;
            source.pitch = pitch * (applySpeedModifier ? sfxSpeedModifier : 1f);
            source.Play();
        }
        public static void PlayStandardClickSound(float pitch = 1f) => PlaySound(Instance.ButtonClick, 1f, pitch);
        public static void PlayStartGameSound() => PlaySound(Instance.StartGame, volume: 0.55f);

        private AudioSource GetNextOneShotSource()
        {
            // Find a free source first
            for (int i = 0; i < OneShotPoolSize; i++)
            {
                int index = (nextOneShotIndex + i) % OneShotPoolSize;
                if (!oneShotPool[index].isPlaying)
                {
                    nextOneShotIndex = (index + 1) % OneShotPoolSize;
                    return oneShotPool[index];
                }
            }

            // All busy steal the next one in rotation
            AudioSource stolen = oneShotPool[nextOneShotIndex];
            stolen.Stop();
            nextOneShotIndex = (nextOneShotIndex + 1) % OneShotPoolSize;
            return stolen;
        }

        #region Music

        private bool isPlayingSpecialTrack = false;

        /// <summary>
        /// Start playing ambient music, cycling through AmbientTracks.
        /// </summary>
        public static void StartMusic()
        {
            if (Instance == null || Instance.AmbientTracks.Length == 0) return;

            Instance.isPlayingSpecialTrack = false;
            Instance.PlayNextTrack();
        }

        /// <summary>
        /// Stop music with a fade out.
        /// </summary>
        public static void StopMusic(float fadeTime = 1f)
        {
            if (Instance == null) return;
            Instance.isPlayingSpecialTrack = false;
            if (Instance.crossfadeCoroutine != null) Instance.StopCoroutine(Instance.crossfadeCoroutine);
            Instance.crossfadeCoroutine = Instance.StartCoroutine(Instance.FadeOut(fadeTime));
        }

        /// <summary>
        /// Crossfade to a specific track. The standard ambient pauses and can be resumed later.
        /// The special track always starts from the beginning.
        /// </summary>
        public static void SwitchToTrack(AudioClip clip, float fadeDuration = -1f)
        {
            if (Instance == null || clip == null) return;
            if (fadeDuration < 0f) fadeDuration = Instance.MusicCrossfadeDuration;

            Instance.isPlayingSpecialTrack = true;

            AudioSource current = Instance.musicSourceAActive ? Instance.musicSourceA : Instance.musicSourceB;
            AudioSource next = Instance.musicSourceAActive ? Instance.musicSourceB : Instance.musicSourceA;
            Instance.musicSourceAActive = !Instance.musicSourceAActive;

            // Pause current instead of stopping so we can resume later
            next.clip = clip;
            next.time = 0f;
            next.loop = true;
            next.Play();

            if (Instance.crossfadeCoroutine != null) Instance.StopCoroutine(Instance.crossfadeCoroutine);
            Instance.crossfadeCoroutine = Instance.StartCoroutine(Instance.CrossfadeWithPause(current, next, fadeDuration));
        }

        /// <summary>
        /// Crossfade back to the standard ambient tracks, resuming where they left off.
        /// </summary>
        public static void ResumeAmbient(float fadeDuration = -1f)
        {
            if (Instance == null) return;
            if (fadeDuration < 0f) fadeDuration = Instance.MusicCrossfadeDuration;

            Instance.isPlayingSpecialTrack = false;

            AudioSource current = Instance.musicSourceAActive ? Instance.musicSourceA : Instance.musicSourceB;
            AudioSource next = Instance.musicSourceAActive ? Instance.musicSourceB : Instance.musicSourceA;
            Instance.musicSourceAActive = !Instance.musicSourceAActive;

            // If the ambient source still has a clip, unpause/resume it
            if (next.clip != null)
            {
                next.loop = false; // Ambient tracks don't loop individually
                next.UnPause();
            }
            else
            {
                // No previous ambient to resume: start fresh
                if (Instance.AmbientTracks.Length == 0) return;
                Instance.currentTrackIndex = (Instance.currentTrackIndex) % Instance.AmbientTracks.Length;
                next.clip = Instance.AmbientTracks[Instance.currentTrackIndex];
                next.loop = false;
                next.Play();
            }

            if (Instance.crossfadeCoroutine != null) Instance.StopCoroutine(Instance.crossfadeCoroutine);
            Instance.crossfadeCoroutine = Instance.StartCoroutine(Instance.CrossfadeAndStop(current, next, fadeDuration));
        }

        private void PlayNextTrack()
        {
            if (AmbientTracks.Length == 0) return;

            currentTrackIndex = (currentTrackIndex + 1) % AmbientTracks.Length;
            AudioClip nextClip = AmbientTracks[currentTrackIndex];

            AudioSource fadeIn = musicSourceAActive ? musicSourceB : musicSourceA;
            AudioSource fadeOut = musicSourceAActive ? musicSourceA : musicSourceB;
            musicSourceAActive = !musicSourceAActive;

            fadeIn.clip = nextClip;
            fadeIn.loop = false;
            fadeIn.Play();

            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = StartCoroutine(Crossfade(fadeOut, fadeIn, MusicCrossfadeDuration));
        }

        /// <summary>
        /// Standard crossfade: stops the outgoing source when done.
        /// </summary>
        private IEnumerator Crossfade(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float timer = 0f;
            float startVolumeOut = fadeOut.volume;
            float targetVolume = MusicVolume * MasterVolume;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);

                // Make sure it's muted if muted
                if (IsMuted)
                {
                    fadeOut.volume = 0;
                    fadeIn.volume = 0;
                }

                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = targetVolume;
            crossfadeCoroutine = null;

            RefreshMusicVolume();
        }

        /// <summary>
        /// Crossfade that pauses the outgoing source instead of stopping it.
        /// Used when switching TO a special track so ambient can be resumed.
        /// </summary>
        private IEnumerator CrossfadeWithPause(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float timer = 0f;
            float startVolumeOut = fadeOut.volume;
            float targetVolume = MusicVolume * MasterVolume;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            fadeOut.Pause(); // Pause, not stop: preserves playback position
            fadeOut.volume = 0f;
            fadeIn.volume = targetVolume;
            crossfadeCoroutine = null;
        }

        /// <summary>
        /// Crossfade that fully stops the outgoing source.
        /// Used when switching FROM a special track back to ambient.
        /// </summary>
        private IEnumerator CrossfadeAndStop(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float timer = 0f;
            float startVolumeOut = fadeOut.volume;
            float targetVolume = MusicVolume * MasterVolume;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = targetVolume;
            crossfadeCoroutine = null;
        }

        private IEnumerator FadeOut(float duration)
        {
            AudioSource active = musicSourceAActive ? musicSourceA : musicSourceB;
            float startVolume = active.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                active.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            active.Stop();
            active.volume = 0f;
            crossfadeCoroutine = null;
        }

        #endregion

        // ==================== CHARGING SOUNDS ====================

        /// <summary>
        /// Start a charging sound that ramps pitch over time.
        /// Use a unique id to control/stop it later.
        /// </summary>
        public static void StartChargingSound(string id, AudioClip clip, float basePitch = 0.8f, float maxPitch = 2f, float volume = 1f)
        {
            if (Instance == null || clip == null || IsMuted) return;

            // Stop existing one with same id
            StopChargingSound(id);

            AudioSource source = Instance.gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.pitch = basePitch;
            source.volume = volume * Instance.SfxVolume * Instance.MasterVolume;
            source.Play();

            Instance.chargingSounds[id] = new ChargingSound
            {
                Source = source,
                BasePitch = basePitch,
                MaxPitch = maxPitch
            };
        }

        /// <summary>
        /// Set the charging sound progress (0 = base pitch, 1 = max pitch).
        /// Call this each frame or whenever the value changes.
        /// </summary>
        public static void SetChargingProgress(string id, float progress)
        {
            if (Instance == null) return;
            if (!Instance.chargingSounds.ContainsKey(id)) return;

            ChargingSound cs = Instance.chargingSounds[id];
            progress = Mathf.Clamp01(progress);
            cs.Source.pitch = Mathf.Lerp(cs.BasePitch, cs.MaxPitch, progress);
        }

        /// <summary>
        /// Stop a charging sound immediately.
        /// </summary>
        public static void StopChargingSound(string id, bool destroy = true)
        {
            if (Instance == null) return;
            if (!Instance.chargingSounds.ContainsKey(id)) return;

            ChargingSound cs = Instance.chargingSounds[id];
            cs.Source.Stop();
            if (destroy)
            {
                Destroy(cs.Source);
                Instance.chargingSounds.Remove(id);
            }
        }

        // ==================== GLOBAL CONTROLS ====================

        /// <summary>
        /// Cycle through volume levels in the order: 3 → 0 (mute) → 1 → 2 → 3...
        /// Returns the new level.
        /// </summary>
        public static int CycleVolumeLevel()
        {
            if (Instance == null) return 0;

            switch (Instance.volumeLevel)
            {
                case 3: Instance.volumeLevel = 0; break;
                case 0: Instance.volumeLevel = 1; break;
                case 1: Instance.volumeLevel = 2; break;
                case 2: Instance.volumeLevel = 3; break;
            }

            Instance.ApplyVolumeLevel();
            return Instance.volumeLevel;
        }

        private void ApplyVolumeLevel()
        {
            SetMasterVolume(volumeLevelValues[volumeLevel]);
            RefreshMusicVolume();
        }

        public static bool IsMuted => Instance.MasterVolume <= 0f;

        public static void SetMasterVolume(float volume)
        {
            if (Instance == null) return;
            Instance.MasterVolume = Mathf.Clamp01(volume);
            Instance.RefreshMusicVolume();
        }

        public static void SetMusicVolume(float volume)
        {
            if (Instance == null) return;
            Instance.MusicVolume = Mathf.Clamp01(volume);
            Instance.RefreshMusicVolume();
        }

        public static void SetSfxVolume(float volume)
        {
            if (Instance == null) return;
            Instance.SfxVolume = Mathf.Clamp01(volume);
        }

        private void RefreshMusicVolume()
        {
            float targetVolume = MusicVolume * MasterVolume;
            AudioSource active = musicSourceAActive ? musicSourceA : musicSourceB;
            if (active.isPlaying && crossfadeCoroutine == null)
            {
                active.volume = targetVolume;
            }
        }
    }
}