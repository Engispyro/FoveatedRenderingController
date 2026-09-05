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
private DepthOfField depthOfField;
public Transform cameraTransform;
private Thread receiveThread;
public Volume volume;
private Vignette vignette;
private Bloom bloom;
private MotionBlur motionBlur;
[SerializeField] private UniversalRendererData rendererData;
private ColorAdjustments colorAdjustments;
private UdpClient client;
[SerializeField]public Light mainLight; 
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
    EnsureColorAdjustmentsReference();
    if (volume.profile.TryGet<DepthOfField>(out depthOfField))
        {
            depthOfField.focusDistance.overrideState = true;
        }
        else
        {
            depthOfField = volume.profile.Add<DepthOfField>(true);
        }
    if (volume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.overrideState = true;
        }
        else
        {
            vignette = volume.profile.Add<Vignette>(true);
        }
    if (!volume.profile.TryGet<MotionBlur>(out motionBlur))
            {
                Debug.LogWarning("MotionBlur component not found on the Volume Profile!");
            }
    if (volume.profile.TryGet<Bloom>(out bloom))
        {
            bloom.intensity.overrideState = true;
        }
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
    private void EnsureColorAdjustmentsReference()
        {
            if (colorAdjustments != null) return; // Already initialized

        if (volume == null)
        {
            // Try to find a Volume on the same GameObject if unassigned
            volume = GetComponent<Volume>();
        }

        if (volume != null && volume.profile != null)
        {
            // Fetch or create ColorAdjustments override
            if (!volume.profile.TryGet(out colorAdjustments))
            {
                colorAdjustments = volume.profile.Add<ColorAdjustments>(true);
                Debug.Log("ColorAdjustments was missing, so it was added to the profile automatically.");
            }
        }
        else
        {
            Debug.LogError("Volume or Volume Profile is missing on " + gameObject.name, this);
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

    void updatecolorcorrection(float satur, float contra, float expo)
    {
        if (colorAdjustments != null){
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.saturation.Override(satur);
        colorAdjustments.contrast.Override(contra);
        colorAdjustments.postExposure.Override(expo);
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
    void AdjustShadows(string force, float distance)
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        urpAsset.shadowDistance = distance;
        Debug.Log(force);
        if(force == "hard")
        {
            Debug.Log("hard");
            QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
        }
        else if(force == "off")
        {
            Debug.Log("off");
            urpAsset.shadowDistance = 0;
        }
        else
        {
            Debug.Log("all");
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
        }
    }
    void AdjustBloom(float active, float bloompower)
    {
        if(active == 0)
        {
            Debug.Log("Bloom off");
            bloom.active = false;
        }
        else
        {
            Debug.Log("bloom on");
            Debug.Log(bloompower);
            bloom.active = true;
            bloom.intensity.value = bloompower;
        }
    }
    void AdjustSSAO(float active)
    {
        if (rendererData == null){ Debug.Log("no render data") ;return;}
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is ScreenSpaceAmbientOcclusion ssao)
            {
                if (active == 0)
                {
                    Debug.Log("SSao off");
                    ssao.SetActive(false);
                }
                else
                {
                    Debug.Log("SSao on");
                    ssao.SetActive(true);
                }
                break;
            }
        }
    }
    void AdjustFog(float active, float fogpower)
    {
        if(active == 0)
        {
            RenderSettings.fog = false;
        }
        else
        {
            RenderSettings.fog = true;
            fogpower = fogpower / 100;
            Debug.Log(fogpower);
            RenderSettings.fogDensity = fogpower;
        }
    }
    private void ProcessNetworkMessage(string message)
    {
        string command = message.Trim();
        Debug.Log($"Received controller command: {command}");
        string[] parts = command.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //parts[0] calls function. 0 = foveation. 1 = GazeTrack. 2=scalerender, 3=resolution, etc....
        if (parts[0] == "0" && (parts.Length >= 3)){
            if (float.TryParse(parts[2], out float foveateLevel) && float.TryParse(parts[1], out float foveateEnable))
            {
                AdjustFoveation(foveateEnable, foveateLevel);
            }
        }
        else if(parts[0] == "1" && (parts.Length >= 2)){
            if (float.TryParse(parts[1], out float gazeTrack))
            {
                gazetrack = gazeTrack;
                SetGazeAllowed(gazetrack);
            }
        }
        else if(parts[0] == "2" && (parts.Length >= 2)){
            if (float.TryParse(parts[1], out float scalerender))
            {
                AdjustRenderScale(scalerender);
            }
        }
        else if(parts[0] == "3" && (parts.Length >= 3)){
            if(float.TryParse(parts[1], out float resx) && float.TryParse(parts[2], out float resy))
            {
                AdjustResolution(resx, resy);
            }
        }
        else if(parts[0] == "4" && (parts.Length >= 3))
        {
            if(float.TryParse(parts[1], out float antialias) && int.TryParse(parts[2], out int aliasval))
            {
                if(antialias == 0)
                {
                    QualitySettings.antiAliasing = 0;
                }
                else
                {
                    QualitySettings.antiAliasing = aliasval;
                }
            }
        }
        else if(parts[0] == "5" && (parts.Length >= 2))
        {
            if (int.TryParse(parts[1], out int vsyncon))
            {
                QualitySettings.vSyncCount = vsyncon;
            }
        }
        else if(parts[0] == "6" && (parts.Length >= 2) && mainLight != null)
        {
            if (float.TryParse(parts[1], out float mainintensity))
            {
                mainLight.intensity = mainintensity;
            }
        }
        else if(parts[0] == "7")
        {
            if (float.TryParse(parts[1], out float envintensity))
            {
                RenderSettings.ambientIntensity = envintensity;
                DynamicGI.UpdateEnvironment();
            }
        }
        else if(parts[0] == "8")
        {
            if (float.TryParse(parts[1], out float saturationvalue) && float.TryParse(parts[2], out float contrastvalue) && float.TryParse(parts[3], out float expositionvalue))
            {
                updatecolorcorrection(saturationvalue, contrastvalue, expositionvalue);
            }
        }
        else if(parts[0] == "9")
        {
            if(float.TryParse(parts[1], out float farclip))
            {
                Debug.Log("fortnite battle pass");
                GetComponentInChildren<Camera>().farClipPlane = farclip;
            }
        }
        else if(parts[0] == "10")
        {
            if(float.TryParse(parts[1], out float shadistance))
            {
                AdjustShadows(parts[2], shadistance);
            }
        }
        else if(parts[0] == "11" && (parts.Length >= 3)){
            if (float.TryParse(parts[2], out float bloomLevel) && float.TryParse(parts[1], out float bloomEnable))
            {
                AdjustBloom(bloomEnable, bloomLevel);
            }
        }
        else if(parts[0] == "12")
        {
            if(int.TryParse(parts[1], out int motionon))
            {
                if(motionon == 1){
                    Debug.Log("blur on");
                    motionBlur.active = true;
                }
                else
                {
                    Debug.Log("blur off");
                    motionBlur.active = false;
                }
            }
        }
        else if(parts[0] == "13")
        {
            if (float.TryParse(parts[1], out float ssaoEnable))
            {
                AdjustSSAO(ssaoEnable);
            }            
        }
        else if(parts[0] == "14")
        {
            {   
            if (depthOfField == null || cameraTransform == null) 
            {
              Debug.LogWarning("DepthOfField or cameraTransform is missing!");
                return;
            }
         if (float.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float focusdist))
            {
                depthOfField.focusDistance.overrideState = true;
                depthOfField.focusDistance.value = focusdist;
            }
        }
        }
        else if(parts[0] == "15")
        {
            if(vignette != null && float.TryParse(parts[1], out float vignvalue))
            {
                vignvalue = vignvalue / 1000;
                Debug.Log(vignvalue);
                vignette.intensity.value = vignvalue;
            }
        }
        else if(parts[0] == "16")
        {
            if(float.TryParse(parts[1], out float fogactive) && float.TryParse(parts[2], out float fogpower))
            {
                AdjustFog(fogactive, fogpower);
            }
        }
        else
        {
            Debug.LogWarning($"Invalid message received");
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

