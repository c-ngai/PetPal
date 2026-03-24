using UnityEngine;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading;

public class ArduinoReader : MonoBehaviour
{
    public string portName = "COM4"; // your Arduino port
    public int baudRate = 9600;

    private SerialPort serialPort;
    private Thread serialThread;
    private bool threadRunning = false;
    private Queue<string> serialQueue = new Queue<string>();

    [Header("Distance Smoothing")]
    public int distanceBufferSize = 5;
    private Queue<float> distanceBuffer = new Queue<float>();
    public float smoothedDistance = 0f;

    [Header("Sensor States")]
    public bool micPressed = false;
    public bool distPressed = false;
    public bool tiltBtnPressed = false;
    public bool isTilted = false;
    public bool soundDetected = false;

    [HideInInspector]
    private bool _soundTriggered = false;  // internal
    public bool soundTriggered { get { return _soundTriggered; } }

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 100;
            serialPort.Open();

            threadRunning = true;
            serialThread = new Thread(SerialReadThread);
            serialThread.Start();

            Debug.Log("Serial port opened.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not open serial port: " + e.Message);
        }
    }

    void SerialReadThread()
    {
        while (threadRunning)
        {
            try
            {
                string line = serialPort.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    lock (serialQueue)
                    {
                        serialQueue.Enqueue(line);
                    }
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception e)
            {
                Debug.LogWarning("Serial thread error: " + e.Message);
            }
        }
    }

    void Update()
    {
        while (true)
        {
            string line = null;
            lock (serialQueue)
            {
                if (serialQueue.Count > 0) line = serialQueue.Dequeue();
                else break;
            }

            if (string.IsNullOrEmpty(line)) continue;

            // ----- Buttons -----
            switch (line)
            {
                case "MIC_BTN_PRESS": micPressed = true; break;
                case "MIC_BTN_RELEASE": micPressed = false; break;
                case "DIST_BTN_PRESS": distPressed = true; break;
                case "DIST_BTN_RELEASE": distPressed = false; break;
                case "TILT_BTN_PRESS": tiltBtnPressed = true; break;
                case "TILT_BTN_RELEASE": tiltBtnPressed = false; break;
                case "TILT_ON": isTilted = true; break;
                case "TILT_OFF": isTilted = false; break;
                case "SOUND":
                    _soundTriggered = true; // trigger once
                    Debug.Log("Sound detected trigger set");
                    break;
            default:
                    if (line.StartsWith("DIST|"))
                    {
                        string valueStr = line.Substring(5);
                        if (float.TryParse(valueStr, out float rawDistance))
                        {
                            distanceBuffer.Enqueue(rawDistance);
                            if (distanceBuffer.Count > distanceBufferSize) distanceBuffer.Dequeue();

                            float sum = 0f;
                            foreach (float v in distanceBuffer) sum += v;
                            smoothedDistance = sum / distanceBuffer.Count;

                            Debug.Log($"Distance: {rawDistance} | Smoothed: {smoothedDistance}");
                        }
                        else if (valueStr == "OUT_OF_RANGE")
                        {
                            Debug.Log("Distance: OUT_OF_RANGE");
                        }
                    }
                    break;
            }
        }
    }

    // Method for PetBehavior to consume the sound trigger
    public void ConsumeSoundTrigger()
    {
        _soundTriggered = false;
    }

    void OnApplicationQuit()
    {
        threadRunning = false;
        if (serialThread != null && serialThread.IsAlive) serialThread.Join();

        if (serialPort != null && serialPort.IsOpen) serialPort.Close();
    }
}