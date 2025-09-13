using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Renderer))]
public class NoteBlock : InteractableObject
{
    private Renderer objectRenderer;
    private AudioSource audioSource;
    
    [System.Serializable] public struct MaterialMapping
    {
        public Note type;
        public Material material;
    }
    [System.Serializable] public struct AudioMapping
    {
        public Note type;
        public List<AudioClip> audioClips;
    }
    
    [Header("Mappings")]
    public MaterialMapping[] materialMappings;
    public AudioMapping[] audioMappings;
    private Dictionary<Note, Material> materialDict;
    private Dictionary<Note, List<AudioClip>> audioDict;

    [Header("Current Settings")]
    [SerializeField] private Note note;
    public Note currentNote {
        get { return note; }
        set {
            note = value;
            UpdateNoteBlockProperties();
        }
    }
    
    [SerializeField] private int octave;
    public int currentOctave {
        get { return octave; }
        set {
            octave = value;
            UpdateNoteBlockProperties();
        }
    }

    void Awake()
    {
        // Get components first
        objectRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Convert arrays to dictionaries
        InitializeDictionaries();
    }
    void Start() {
        UpdateNoteBlockProperties();
    }
    void OnValidate()
    {
        // This will be handled by the custom editor in edit mode
        // Only update if we're in play mode
        if (Application.isPlaying)
        {
            InitializeDictionaries();
            UpdateNoteBlockProperties();
        }
    }
    
    private void InitializeDictionaries()
    {
        materialDict = new Dictionary<Note, Material>();
        if (materialMappings != null)
        {
            foreach (var mapping in materialMappings)
            {
                materialDict[mapping.type] = mapping.material;
            }
        }

        audioDict = new Dictionary<Note, List<AudioClip>>();
        if (audioMappings != null)
        {
            foreach (var mapping in audioMappings)
            {
                audioDict[mapping.type] = mapping.audioClips;
            }
        }
    }
    private void UpdateNoteBlockProperties() 
    {
        if (objectRenderer == null || audioSource == null)
        {
            // Try to get components if they're null
            objectRenderer = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();
        }
        
        if (objectRenderer == null || audioSource == null || materialDict == null || audioDict == null)
            return;
            
        // Update material
        if (materialDict.ContainsKey(note))
        {
            // Use sharedMaterial in edit mode, material in play mode
            if (Application.isPlaying)
                objectRenderer.material = materialDict[note];
            else
                objectRenderer.sharedMaterial = materialDict[note];
        }
        
        // Update audio clip
        if (audioDict.ContainsKey(note))
        {
            var clipList = audioDict[note];
            if (clipList != null && octave >= 0 && octave < clipList.Count)
            {
                audioSource.clip = clipList[octave];
            }
        }
    }
    
    public override void DoClick() {
        audioSource.Play();
    }
    
    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Marble")) {
            if (!BeatManager.self.hasFirstNoteOccurred) {
                BeatManager.self.StartAttempt();
                BeatManager.self.SetFirstNoteOccurred(true);
            }

            audioSource.PlayScheduled(AudioSettings.dspTime);
            LevelManager.self.TriggerNote(note);
        }
    }
}