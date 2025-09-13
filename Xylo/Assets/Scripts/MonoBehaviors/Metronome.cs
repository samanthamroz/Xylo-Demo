using System.Collections.Concurrent;
using UnityEngine;
using System;

//yeah ngl this entire class was written by chatGPT

[RequireComponent(typeof(AudioSource))]
public class Metronome : MonoBehaviour {
    public delegate void Beat();
    public static event Beat OnBeat;

    public delegate void DownBeat();
    public static event DownBeat OnDownBeat;

    private double sampleRate;
    private double nextTick;
    private int accent;

    [SerializeField] private double setBpm = 80;
    [HideInInspector] public double bpm => setBpm;

    [SerializeField] private int signatureHi = 4;
    [SerializeField] private int signatureLo = 4;

    private bool running = false;
    public void SetMetronomeRunning(bool isRunning) {
        running = isRunning;
    }

    // Thread-safe queue for beat events
    private ConcurrentQueue<Action> beatEvents = new();

    void Start() {
        ResetTickSchedule();
        running = false;
    }

    public void ResetTickSchedule() {
        sampleRate = AudioSettings.outputSampleRate;
        nextTick = AudioSettings.dspTime * sampleRate;
        accent = signatureHi;
    }

    void Update() {
        while (beatEvents.TryDequeue(out var e)) { //try to get the next event out of the queue
            e?.Invoke(); //as long as the event isn't null, do it
        }
    }

    void OnAudioFilterRead(float[] data, int channels) {
        if (!running) return;

        double samplesPerTick = sampleRate * 60.0f / bpm * 4.0f / signatureLo;
        double dspSample = AudioSettings.dspTime * sampleRate;
        int dataLen = data.Length / channels;

        for (int n = 0; n < dataLen; n++) {
            while (dspSample + n >= nextTick) {
                nextTick += samplesPerTick;

                // Queue events for main thread
                if (++accent > signatureHi) {
                    accent = 1;
                    Action downBeatAction = new Action(delegate {
                        OnDownBeat?.Invoke(); //Essentially, if OnDownBeat != null, invoke it
                    });
                    beatEvents.Enqueue(downBeatAction); //put that action in the queue
                }
                else {
                    Action beatAction = new Action(delegate {
                        OnBeat?.Invoke(); //Essentially, if OnDownBeat != null, invoke it
                    });
                    beatEvents.Enqueue(beatAction); //put that action in the queue
                }
            }
        }
    }
}