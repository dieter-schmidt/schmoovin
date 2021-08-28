using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    public interface ICharacterAudioHandler
    {
        void PlayClip (AudioClip clip, float volume = 1f);        
        void PlayClip (AudioClip clip, FpsCharacterAudioSource source, float volume = 1f);
        void PlayAudio(FpsCharacterAudio id);
        void PlayAudio(FpsCharacterAudio id, FpsCharacterAudioSource source);
        void StartLoop (AudioClip clip, FpsCharacterAudioSource source, float volume = 1f, float pitch = 1f);
        void StopLoop (FpsCharacterAudioSource source);
        float GetLoopPitch (FpsCharacterAudioSource source);        
        void SetLoopPitch (FpsCharacterAudioSource source, float pitch);
        float GetLoopVolume(FpsCharacterAudioSource source);
        void SetLoopVolume(FpsCharacterAudioSource source, float volume);
    }
}