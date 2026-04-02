using UnityEngine;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading;

public class ArduinoReader : MonoBehaviour
{
    [Header("Hardware Settings")]
    public bool enableArduino = false; // <-- UNCHECK THIS IN UNITY TO USE KEYBOARD ONLY
    public string portName = "COM4";
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
    // left button
    public bool micPressed = false;

    // right button 
    public bool distPressed = false;

    // middle button
    public bool tiltBtnPressed = false;
    public bool isTilted = false;
    public bool soundDetected = false;

    [HideInInspector]
    private bool _soundTriggered = false;
    public bool soundTriggered { get { return _soundTriggered; } }

    void Start()
    {
        // 🛑 If we are testing with just the keyboard, ignore the Arduino entirely
        if (!enableArduino)
        {
            Debug.Log("🔌 Arduino Disabled: Running in Keyboard-Only Mode.");
            return;
        }

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
        if (!enableArduino) return; // Skip updating if Arduino is disabled

        while (true)
        {
            string line = null;
            lock (serialQueue)
            {
                if (serialQueue.Count > 0) line = serialQueue.Dequeue();
                else break;
            }

            if (string.IsNullOrEmpty(line)) continue;

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
                    _soundTriggered = true;
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
                        }
                    }
                    break;
            }
        }
    }

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