using UnityEngine;

[RequireComponent(typeof(Metronome))]
public class BeatManager : MonoBehaviour {
    public static BeatManager self;
    private Metronome metronome;

    private double dspStartTime, songPosInSec;
    [HideInInspector] public double songPosInBeats { get; private set; }
    [HideInInspector] public double secPerBeat { get; private set; }
    
    public float xDistancePerBeat = 2;
    public float beatsBetweenFirstTwoBeats = 1;

    private int currentBeat = 0, currentMeasure = 0, totalBeatCount = 0;
    [HideInInspector] public bool hasFirstNoteOccurred { get; private set; }
    public void SetFirstNoteOccurred(bool hasOccured) {
        hasFirstNoteOccurred = hasOccured;
    }

    void Awake() {
        if (self == null) {
            self = this;
        }
    }

    void Start() {
        metronome = GetComponent<Metronome>();
        secPerBeat = 60 / metronome.bpm;
        hasFirstNoteOccurred = false;
    }

    private void OnEnable() {
        Metronome.OnBeat += Beat;
        Metronome.OnDownBeat += DownBeat;
    }
    private void OnDisable() {
        Metronome.OnBeat -= Beat;
        Metronome.OnDownBeat -= DownBeat;
    }

    void Update() {
        songPosInSec = (float)(AudioSettings.dspTime - dspStartTime);
        songPosInBeats = songPosInSec / secPerBeat + 1;
    }

    //time = beat * spb
    //beat = time / spb
    //spb = beat / time
    public double GetTimestampForBeat(int beat) {
        return (beat - 1) * secPerBeat;
    }
    public float GetSecsToBeat(int beat) {
        return (float)(GetTimestampForBeat(beat) - songPosInSec);
    }
    public float GetBeatForTimestamp(double timestamp) {
        return (float)(timestamp / secPerBeat) + 1f;
    }
    public float GetBeatInNSeconds(float n) {
        return GetBeatForTimestamp(songPosInSec + n);
    }

    public double DeltaTimeForBeats(float numBeats) {
        double remainingSecondsInCurrentBeat = GetTimestampForBeat(totalBeatCount + +1) - songPosInSec;
        double secondsForBeats = numBeats * secPerBeat;
        return remainingSecondsInCurrentBeat + secondsForBeats;
    }

    public double GetSecsToNextBeat() {
        double secsToBeat = GetTimestampForBeat(totalBeatCount + 1) - songPosInSec;
        //print(secsToBeat);
        return secsToBeat;
    }

    public double GetCurrentBeatOffset() {
        return songPosInBeats - totalBeatCount;
    }

    public void StartAttempt() {
        // Reset counters
        currentBeat = 0;
        currentMeasure = 0;
        totalBeatCount = 0;

        // Reset time
        dspStartTime = AudioSettings.dspTime;
        Update();

        StartMetronome();
    }

    public void StartMetronome() {
        metronome.ResetTickSchedule();
        metronome.SetMetronomeRunning(true);
    }
    public void StopMetronome() {
        metronome.SetMetronomeRunning(false);
    }

    void Beat() {
        currentBeat++;
        totalBeatCount++;
        //print($"Measure {currentMeasure} | Beat {currentBeat}");
    }
    void DownBeat() {
        currentBeat = 0;
        currentMeasure++;
        totalBeatCount++;
        //print($"Measure {currentMeasure} | Beat {currentBeat}");
    }
}