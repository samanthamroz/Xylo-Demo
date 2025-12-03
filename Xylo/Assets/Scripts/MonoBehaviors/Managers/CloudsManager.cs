using UnityEngine;
using System.Linq;

public class CloudsManager : MonoBehaviour {
    [System.Serializable]
    public class SectionClouds {
        public string sectionName;
        public Cloud[] cloudsToMove;
    }

    [SerializeField] private SectionClouds[] sections;
	
	public static CloudsManager self;

    void Awake() {
		if (self == null) {
			self = this;
		}
	}

	public void MoveCloudsForSection(int sectionNum, bool goingBackToPrevSection = false) {
		var thisSection = sections[sectionNum];
		
		if (goingBackToPrevSection) {
			for (int i = sections.Length - 1; i >= 0; i--) {
				var section = sections[i];
				foreach (Cloud c in section.cloudsToMove) {
					if (thisSection.cloudsToMove.Contains(c)) {
						continue;
					}
					c.DoMoveToStart();
				}
				if (section == thisSection) {
					break;
				}
			}
        }
        

		foreach (Cloud c in thisSection.cloudsToMove) {
            c.DoMoveToEnd();
        }
    }
}