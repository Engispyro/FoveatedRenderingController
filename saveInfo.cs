
using UnityEngine;
using TMPro;
using System.IO;
using System.Text;
using UnityEngine.Profiling;
using Unity.Profiling;
using System.Collections.Generic;
using UnityEngine.XR;

public class saveInfo : MonoBehaviour
{
    private List<int> triangleIndices = new List<int>();
    private float fps;
    private float deltaTime = 0.0f;
    double cpuTimeMs;
    double gpuTimeMs;
    public long totalAllocated;
    public long allocatedBytes;
    public float allocatedMB;
    public double allocatedVRAM;
    private ProfilerRecorder setPassRecorder;
    private ProfilerRecorder batchesRecorder;
    int totalTriangles;
    MeshFilter meshFilter;
    private XRDisplaySubsystem displaySubsystem;
    int droppedFrames;
    private FrameTiming[] m_Timings = new FrameTiming[1];
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        meshFilter = GetComponent<MeshFilter>();
        List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displays);
        if (displays.Count > 0)
        {
            displaySubsystem = displays[0];
        }
        InvokeRepeating("Inforead", 0.5f, 0.5f);
    }
    void OnDisable()
    {
        // Dispose of the recorders to avoid memory leaks
        setPassRecorder.Dispose();
        batchesRecorder.Dispose();
    }
    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;
        FrameTimingManager.CaptureFrameTimings();
        uint retrievedCount = FrameTimingManager.GetLatestTimings(1, m_Timings);
        setPassRecorder= ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        batchesRecorder= ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        totalAllocated = Profiler.GetTotalAllocatedMemoryLong();
        allocatedBytes = Profiler.GetAllocatedMemoryForGraphicsDriver();
        allocatedMB = totalAllocated / (1024f * 1024f);
        allocatedVRAM = allocatedBytes / (1024.0 * 1024.0);   
        List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displays);
        displaySubsystem.TryGetDroppedFrameCount(out int droppedFrames);
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            totalTriangles = 0;
            Mesh mesh = meshFilter.sharedMesh;

            // Loop through all sub-meshes (if the object uses multiple materials)
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                mesh.GetTriangles(triangleIndices, i);
                totalTriangles += triangleIndices.Count / 3;
            }    
        if (retrievedCount > 0)
        {
            // Convert seconds to milliseconds
            double cpuTimeMs = m_Timings[0].cpuFrameTime * 1000.0;
            double gpuTimeMs = m_Timings[0].gpuFrameTime * 1000.0;
        }
        }
        }
        void Inforead()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "GameData.csv");
        StringBuilder sb = new StringBuilder();
        bool fileExists = File.Exists(filePath);
        if (!fileExists)
        {
            sb.AppendLine("sep=;");
            sb.AppendLine("FPS;CPU Time;GPU Time;RAM;VRAM;Passes;Batches;Triangles;Dropped Frames");
        }
        sb.AppendLine($"{fps:F1};{cpuTimeMs:F2};{gpuTimeMs:F2};{totalAllocated:F2} MB;{allocatedBytes} MB;{setPassRecorder.LastValue};{batchesRecorder.LastValue};{totalTriangles};{droppedFrames}");

        using (StreamWriter writer = new StreamWriter(filePath, append: true, Encoding.UTF8))
        {
            writer.Write(sb.ToString());
        }
    }
}
