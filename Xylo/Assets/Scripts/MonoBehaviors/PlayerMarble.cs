using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMarble : MonoBehaviour
{
    private List<Note> attemptNotesList;
    void Start()
    {
        attemptNotesList = new List<Note>();
    }
    void OnMouseDown() {
        GetComponent<Rigidbody>().isKinematic = false;
    }
    public void AddNote(Note note) {
        attemptNotesList.Add(note);
    }
    void OnCollisionEnter(Collision other)
    {
        GetComponent<AudioSource>().Play();
    }
}
