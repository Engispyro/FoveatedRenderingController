using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 


public class FoveationSettings : MonoBehaviour
{
  public float FoveateLevel = 1.0f;
[SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRGazeInteractor gazeInteractor;

private Thread receiveThread;
private UdpClient client;
public int port = 8005;
private string lastReceivedMessage = "";
private bool hasNewMessage = false;

private volatile bool isRunning = false;

float gazetrack = 1;


  private List<XRDisplaySubsystem> xrDisplays = new List<XRDisplaySubsystem>();

  void Start()
  {
    StartListener();
    SubsystemManager.GetSubsystems(xrDisplays);
    if (xrDisplays.Count == 1)
    {
        xrDisplays[0].foveatedRenderingLevel = FoveateLevel; 
        xrDisplays[0].foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
        Debug.Log("Foveated Ativo");
    }
    receiveThread = new Thread(new ThreadStart(ReceiveData));
    receiveThread.IsBackground = true;
    receiveThread.Start();
    Debug.Log($"Server started, listening on port {port}...");
  }
    void Update()
    {
        if (gazeInteractor != null)
        {
            // Position and gaze direction in world space
            Vector3 gazeOrigin = gazeInteractor.transform.position;
            Vector3 gazeDirection = gazeInteractor.transform.forward;

            //Debug.Log($"Gaze Origin: {gazeOrigin} | Direction: {gazeDirection}");
        }
        if (hasNewMessage)
        {
            lock (this)
            {
                hasNewMessage = false;
                ProcessNetworkMessage(lastReceivedMessage);
            }
        }
    }

    private void StartListener()
    {
        try
        {
            // Set socket reuse options BEFORE binding to prevent lockups during domain reloads
            client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));

            isRunning = true;
            receiveThread = new Thread(ReceiveData)
            {
                IsBackground = true
            };
            receiveThread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UDP] Failed to bind to port {port}: {ex.Message}");
        }
    }

private void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                
                // Blocking call until data arrives or socket closes
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);

                lock (this)
                {
                    lastReceivedMessage = text;
                    hasNewMessage = true;
                }
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception err)
            {
                if (isRunning)
                {
                    Debug.LogError($"[UDP Error] {err}");
                }
            }
        }
    }

    void AdjustFoveation(float active, float amount)
    {
        if(active == 0)
        {
            xrDisplays[0].foveatedRenderingLevel = 0;
        }
        else
        {
            xrDisplays[0].foveatedRenderingLevel = amount;
        }
    }

    void AdjustRenderScale(float renderscale)
    {
        renderscale = Mathf.Clamp(renderscale, 0.1f, 2.0f);

        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset)
        {
            urpAsset.renderScale = renderscale;
        }
    }

    void AdjustResolution(float resolutionx, float resolutiony)
    {
        int resox = (int)resolutionx;
        int resoy = (int)resolutiony;
        Screen.SetResolution(resox, resoy, true);
    }

    private void ProcessNetworkMessage(string message)
    {
        string command = message.Trim();
        Debug.Log($"Received controller command: {command}");
        string[] parts = command.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
            {
            if (float.TryParse(parts[0], out float foveateLevel) && float.TryParse(parts[2], out float foveateEnable))
            {
                AdjustFoveation(foveateEnable, foveateLevel);
            }

            if (float.TryParse(parts[1], out float gazeTrack))
            {
                gazetrack = gazeTrack;
                SetGazeAllowed(gazetrack);
            }
            if (float.TryParse(parts[3], out float scalerender))
            {
                AdjustRenderScale(scalerender);
            }
            if(float.TryParse(parts[4], out float resx) && float.TryParse(parts[5], out float resy))
            {
                AdjustResolution(resx, resy);
            }
        }
        else
        {
            Debug.LogWarning($"Malformed network message. Expected 3 values, got: {parts.Length}");
        }
        }  
    public void SetGazeAllowed(float state)
    {
        SubsystemManager.GetSubsystems(xrDisplays);
        if (xrDisplays.Count == 0) return;

        XRDisplaySubsystem display = xrDisplays[0];

        if (state > 0.5f)
        {
            display.foveatedRenderingFlags |= XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
        }
        else
        {
            display.foveatedRenderingFlags &= ~XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
        }
    }
    private void StopListener()
    {
        isRunning = false;

        // Closing the client breaks client.Receive() instantly and frees the port
        if (client != null)
        {
            client.Close();
            client = null;
        }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(100); // Give thread up to 100ms to clean up
            receiveThread = null;
        }
    }
    void OnDestroy()
{
    StopListener();
}
}

