// Put this script in a folder named "Editor" in your Assets folder
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(NoteBlock))]
public class NoteBlockEditor : Editor
{
    private Dictionary<Note, Material> materialDict;
    private Dictionary<Note, List<AudioClip>> audioDict;
    
    public override void OnInspectorGUI()
    {
        NoteBlock noteBlock = (NoteBlock)target;
        
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Check if any values changed
        if (GUI.changed)
        {
            // Update the visual in edit mode
            UpdateNoteBlockInEditMode(noteBlock);
        }
    }
    
    private void UpdateNoteBlockInEditMode(NoteBlock noteBlock)
    {
        // Get components
        var objectRenderer = noteBlock.GetComponent<Renderer>();
        var audioSource = noteBlock.GetComponent<AudioSource>();
        
        if (objectRenderer == null || audioSource == null)
            return;
            
        // Build dictionaries from the serialized arrays
        BuildDictionaries(noteBlock);
        
        // Get current values using reflection since fields are private
        var noteField = typeof(NoteBlock).GetField("note", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var octaveField = typeof(NoteBlock).GetField("octave", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (noteField != null && octaveField != null)
        {
            Note currentNote = (Note)noteField.GetValue(noteBlock);
            int currentOctave = (int)octaveField.GetValue(noteBlock);
            
            // Update material
            if (materialDict != null && materialDict.ContainsKey(currentNote))
            {
                objectRenderer.sharedMaterial = materialDict[currentNote];
                // Mark the scene as dirty so changes are saved
                EditorUtility.SetDirty(noteBlock);
            }
            
            // Update audio clip
            if (audioDict != null && audioDict.ContainsKey(currentNote))
            {
                var clipList = audioDict[currentNote];
                if (clipList != null && currentOctave >= 0 && currentOctave < clipList.Count)
                {
                    audioSource.clip = clipList[currentOctave];
                    EditorUtility.SetDirty(noteBlock);
                }
            }
        }
    }
    
    private void BuildDictionaries(NoteBlock noteBlock)
    {
        // Access the public arrays
        var materialMappings = noteBlock.materialMappings;
        var audioMappings = noteBlock.audioMappings;
        
        // Build material dictionary
        materialDict = new Dictionary<Note, Material>();
        if (materialMappings != null)
        {
            foreach (var mapping in materialMappings)
            {
                materialDict[mapping.type] = mapping.material;
            }
        }
        
        // Build audio dictionary
        audioDict = new Dictionary<Note, List<AudioClip>>();
        if (audioMappings != null)
        {
            foreach (var mapping in audioMappings)
            {
                audioDict[mapping.type] = mapping.audioClips;
            }
        }
    }
}