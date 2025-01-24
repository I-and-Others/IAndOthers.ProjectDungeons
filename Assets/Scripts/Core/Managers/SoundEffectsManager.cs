using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Core;
using Scripts.Entities.Class;
using UnityEngine;

public class SoundEffectsManager : Singleton<SoundEffectsManager>
{
    [Header("Settings")]
    [Range(0, 1)] public float audioVolume = 0.5f;
    [SerializeField] private List<AudioClip> soundEffects;
    void Start()
    {
        EventManager.Instance.AddListener<OnSoundEffectsPlayEventArgs>(GameEvents.ON_PLAY_SFX, PlaySoundEffects);
    }

    private void PlaySoundEffects(object sender, OnSoundEffectsPlayEventArgs e)
    {
        var audioClip = soundEffects.Where(x => x.name == e.SoundEffectsType.GetDescription()).FirstOrDefault();
        if (audioClip == null) return;
        AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position, audioVolume);
    }
}