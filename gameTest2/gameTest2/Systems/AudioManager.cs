using Microsoft.Xna.Framework.Media;
using System;

namespace gameTest2.Systems
{
    /// <summary>
    /// Manages all audio playback including music and sound effects.
    /// Handles background music loops and sound effect playback with proper error handling.
    /// </summary>
    public class AudioManager
    {
        private Song _buttonClickSong;
        private Song _mainMenuMusic;
        private bool _audioEnabled;
        private bool _mainMenuMusicPlaying;

        /// <summary>
        /// Gets whether audio is currently enabled (successful initialization)
        /// </summary>
        public bool IsAudioEnabled => _audioEnabled;

        /// <summary>
        /// Gets whether the main menu music is currently playing
        /// </summary>
        public bool IsMainMenuMusicPlaying => _mainMenuMusicPlaying;

        /// <summary>
        /// Initializes the audio manager with provided audio assets.
        /// If initialization fails, audio will be disabled but the game can continue.
        /// </summary>
        /// <param name="buttonClickSong">Song to play for button clicks</param>
        /// <param name="mainMenuMusic">Song to loop in the main menu</param>
        public void Initialize(Song buttonClickSong, Song mainMenuMusic)
        {
            _buttonClickSong = buttonClickSong;
            _mainMenuMusic = mainMenuMusic;
            
            // Enable audio if both assets loaded successfully
            _audioEnabled = _buttonClickSong != null && _mainMenuMusic != null;
            _mainMenuMusicPlaying = false;

            if (!_audioEnabled)
            {
                System.Diagnostics.Debug.WriteLine("Audio manager initialized but audio is disabled (assets not loaded)");
            }
        }

        /// <summary>
        /// Starts playing the main menu music on loop.
        /// Safe to call multiple times - will not restart if already playing.
        /// </summary>
        public void PlayMainMenuMusic()
        {
            if (!_audioEnabled || _mainMenuMusic == null || _mainMenuMusicPlaying)
                return;

            try
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(_mainMenuMusic);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.4f;
                _mainMenuMusicPlaying = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play main menu music: {ex.Message}");
                _audioEnabled = false; // Disable audio if playback fails
            }
        }

        /// <summary>
        /// Stops the main menu music if it's currently playing.
        /// Safe to call even if music is not playing.
        /// </summary>
        public void StopMainMenuMusic()
        {
            if (!_audioEnabled || !_mainMenuMusicPlaying)
                return;

            try
            {
                MediaPlayer.Stop();
                _mainMenuMusicPlaying = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to stop main menu music: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays the button click sound effect.
        /// Will temporarily stop music to play the sound, but won't restart music.
        /// </summary>
        public void PlayButtonClickSound()
        {
            if (!_audioEnabled || _buttonClickSong == null)
                return;

            try
            {
                bool wasPlayingMenuMusic = _mainMenuMusicPlaying;

                // Stop any current playback and play the click sound
                MediaPlayer.Stop();
                MediaPlayer.Play(_buttonClickSong);
                MediaPlayer.Volume = 0.5f;

                // Mark that menu music is no longer playing
                // (Note: Menu music won't auto-resume, caller must restart if needed)
                if (wasPlayingMenuMusic)
                {
                    _mainMenuMusicPlaying = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play button click sound: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures the main menu music is playing (starts it if not already playing).
        /// Useful for state transitions where you want to guarantee menu music is active.
        /// </summary>
        public void EnsureMainMenuMusicPlaying()
        {
            if (!_mainMenuMusicPlaying)
            {
                PlayMainMenuMusic();
            }
        }

        /// <summary>
        /// Ensures the main menu music is stopped.
        /// Useful for transitioning to gameplay or other non-menu states.
        /// </summary>
        public void EnsureMainMenuMusicStopped()
        {
            if (_mainMenuMusicPlaying)
            {
                StopMainMenuMusic();
            }
        }

        /// <summary>
        /// Stops all audio playback and resets state.
        /// Useful for cleanup or full audio reset.
        /// </summary>
        public void StopAllAudio()
        {
            if (!_audioEnabled)
                return;

            try
            {
                MediaPlayer.Stop();
                _mainMenuMusicPlaying = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to stop all audio: {ex.Message}");
            }
        }
    }
}
