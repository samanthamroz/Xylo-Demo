using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioClipList {
	public List<AudioClip> list = new();

	public AudioClip this[int index] {
		get { return list[index]; }
		set { list[index] = value; }
	}
}