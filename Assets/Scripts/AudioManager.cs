using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class AudioEntry
{
    [SerializeField] private string audioId;
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    public string AudioId => audioId;
    public AudioClip Clip => clip;
    public float Volume => volume;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private List<AudioEntry> bgmClips = new List<AudioEntry>();
    [SerializeField] private List<AudioEntry> sfxClips = new List<AudioEntry>();

    [Header("Startup")]
    [SerializeField] private string startupBgmId;

    [NonSerialized] private Dictionary<string, AudioEntry> bgmById;
    [NonSerialized] private Dictionary<string, AudioEntry> sfxById;

    // Skills without a clip yet would otherwise warn on every single cast.
    [NonSerialized] private readonly HashSet<string> warnedMissingIds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<AudioEntry> BgmClips => bgmClips;
    public IReadOnlyList<AudioEntry> SfxClips => sfxClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RebuildLookup();
    }

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(startupBgmId))
        {
            PlayBGM(startupBgmId);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool PlayBGM(string audioId)
    {
        EnsureLookup();
        if (!TryGetEntry(bgmById, audioId, out AudioEntry entry))
        {
            Debug.LogWarning($"[AudioManager] Unknown BGM id '{audioId}'.", this);
            return false;
        }

        if (bgmSource == null)
        {
            Debug.LogWarning("[AudioManager] BGM AudioSource is not assigned.", this);
            return false;
        }

        bgmSource.clip = entry.Clip;
        bgmSource.volume = entry.Volume;
        bgmSource.Play();
        return true;
    }

    public bool PlaySFX(string audioId)
    {
        EnsureLookup();
        if (!TryGetEntry(sfxById, audioId, out AudioEntry entry))
        {
            if (warnedMissingIds.Add(audioId ?? string.Empty))
            {
                Debug.LogWarning($"[AudioManager] No SFX registered for id '{audioId}'.", this);
            }

            return false;
        }

        if (sfxSource == null)
        {
            Debug.LogWarning("[AudioManager] SFX AudioSource is not assigned.", this);
            return false;
        }

        sfxSource.PlayOneShot(entry.Clip, entry.Volume);
        return true;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    // Callers must run EnsureLookup() first so the dictionary passed in is current.
    private static bool TryGetEntry(
        Dictionary<string, AudioEntry> lookup,
        string audioId,
        out AudioEntry entry)
    {
        entry = null;
        return !string.IsNullOrWhiteSpace(audioId) &&
               lookup.TryGetValue(audioId, out entry);
    }

    private void EnsureLookup()
    {
        if (bgmById == null || sfxById == null)
        {
            RebuildLookup();
        }
    }

    private void RebuildLookup()
    {
        bgmClips ??= new List<AudioEntry>();
        sfxClips ??= new List<AudioEntry>();
        bgmById = BuildLookup(bgmClips, "BGM");
        sfxById = BuildLookup(sfxClips, "SFX");
    }

    private Dictionary<string, AudioEntry> BuildLookup(
        List<AudioEntry> entries,
        string label)
    {
        var lookup = new Dictionary<string, AudioEntry>(
            entries.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (AudioEntry entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.AudioId))
            {
                continue;
            }

            if (entry.Clip == null)
            {
                Debug.LogWarning(
                    $"[AudioManager] {label} '{entry.AudioId}' has no clip assigned.",
                    this);
                continue;
            }

            if (!lookup.TryAdd(entry.AudioId, entry))
            {
                Debug.LogWarning(
                    $"[AudioManager] Duplicate {label} id '{entry.AudioId}' is ignored.",
                    this);
            }
        }

        return lookup;
    }
}
