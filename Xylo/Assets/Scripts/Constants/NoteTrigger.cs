using System;

[Serializable]
public class NoteTrigger {
    public Note note;
    public double beatTriggered;
    
    public NoteTrigger(Note _note, double _beatTriggered) {
        note = _note;
        beatTriggered = _beatTriggered;
    }
}