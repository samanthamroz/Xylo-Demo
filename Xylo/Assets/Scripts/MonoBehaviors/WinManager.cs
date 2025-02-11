using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    public static WinManager self;
    private List<Note> attemptNotesList;

    void Awake() {
		if (self == null) {
			self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
    }

    public void TriggerNewAttempt() {
        attemptNotesList = new List<Note>();
    }

    public void TriggerNote(Note note) {
        attemptNotesList.Add(note);
    }
}
