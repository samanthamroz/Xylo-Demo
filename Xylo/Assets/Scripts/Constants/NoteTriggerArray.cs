using System;

[Serializable]
public class NoteTriggerArray {
    public NoteTrigger[] triggers;
    public NoteTrigger this[int index] {
		get { return triggers[index]; }
		set { triggers[index] = value; }
	}
    public int Length => triggers.Length;
}