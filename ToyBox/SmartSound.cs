using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmartSound : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audio;
    public new AudioSource audio
    {
        get
        {
            if (_audio == null)
            {
                _audio = GetComponent<AudioSource>();
            }
            return _audio;
        }
    }

    public List<AudioClip> clips = new List<AudioClip>();

    public bool playRandomClip = true;

    public bool doNotInterrupt = false;

    private AudioClip currentClip;

    void Reset()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
        }
        _audio.playOnAwake = false;
        _audio.spatialBlend = 1f;
        _audio.dopplerLevel = 0.2f;

    }

    private void OnValidate()
    {
        if (audio != null)
        {
        }
    }

    private void Awake()
    {
        if (audio != null)
        {
        }
    }

    // USE THIS IN THE OBJECT THAT REFERENCES THIS SMARTSOUND:
    // to instantiate a clone of the sound prefab when assigning it to a script.
    // useful when assigning multiple sounds on multiple objects, and you don't wanna drag n drop and select all those objects.
    /* 
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (consumeSound != null)
        {
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(consumeSound))
            {
                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(consumeSound))
                {
                    // we should spawn this as a child and assign the instance.
                    var spawnedSound = (UnityEditor.PrefabUtility.InstantiatePrefab(consumeSound, transform) as SmartSound);
                    spawnedSound.transform.position = transform.position;
                    spawnedSound.transform.rotation = transform.rotation;
                    consumeSound = spawnedSound;
                }
            }
        }
#endif
    }
    */


    [DebugButton]
    public void Play()
    {
        if (doNotInterrupt)
        {
            if (_audio.isPlaying)
            {
                return;
            }
        }

        if (playRandomClip)
        {
            if (clips.Count > 0)
            {
                _audio.clip = clips[Random.Range(0, clips.Count)];
            }
        }

        _audio.Play();
    }

    [DebugButton]
    public void PlayOneShot()
    {

        if (playRandomClip)
        {
            if (clips.Count > 0)
            {
                currentClip = clips[Random.Range(0, clips.Count)];
            }
        }
        else
        {
            currentClip = clips[0];
        }

        _audio.PlayOneShot(currentClip);
    }

    public void Stop()
    {
        if (_audio.isPlaying)
        {
            _audio.Stop();
        }
    }
}