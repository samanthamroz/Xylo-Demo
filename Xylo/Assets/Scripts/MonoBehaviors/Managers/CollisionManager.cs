using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour {
	[SerializeField] private GameObject[] turnOffForPuzzle1;
    [SerializeField] private GameObject[] turnOffForPuzzle2;
    [SerializeField] private GameObject[] turnOffForPuzzle3;
    [SerializeField] private GameObject[] turnOffForPuzzle4;
    [SerializeField] private GameObject[] turnOffForPuzzle5;
    [SerializeField] private GameObject[] turnOffForPuzzle6;
    [SerializeField] private GameObject[] turnOffForPuzzle7; //cinematic
    [SerializeField] private GameObject[] turnOffForPuzzle8;
    [SerializeField] private GameObject[] turnOffForPuzzle9;
    [SerializeField] private GameObject[] turnOffForPuzzle10;
    [SerializeField] private GameObject[] turnOffForPuzzle11;
    [SerializeField] private GameObject[] turnOffForPuzzle12;
    [SerializeField] private GameObject[] turnOffForPuzzle13;
    private List<GameObject[]> turnOffs;
	public static CollisionManager self;
	void Awake() {
		if (self == null) {
			self = this;
		}
        turnOffs = new List<GameObject[]>{turnOffForPuzzle1, turnOffForPuzzle2, turnOffForPuzzle3,
            turnOffForPuzzle4, turnOffForPuzzle5, turnOffForPuzzle6, turnOffForPuzzle7, turnOffForPuzzle8,
            turnOffForPuzzle9, turnOffForPuzzle10, turnOffForPuzzle11, turnOffForPuzzle12, turnOffForPuzzle13};
	}
    private void TurnOnAllCollisions() {
        foreach(GameObject[] garray in turnOffs) {
            foreach(GameObject g in garray) {
                g.SetActive(true);
            }
        }
    }

    public void TurnOffCollisionForPuzzle(int sectionNum) {
        TurnOnAllCollisions();
        foreach(GameObject g in turnOffs[sectionNum]) {
            g.SetActive(false);
        }
    }

    public void TurnOffCollisionForCinematic() {
        TurnOnAllCollisions();
        foreach(GameObject g in turnOffForPuzzle7) {
            g.SetActive(false);
        }
    }

    public void TurnOffAllCollision() {
        foreach(GameObject[] garray in turnOffs) {
            foreach(GameObject g in garray) {
                g.SetActive(false);
            }
        }
    }
}