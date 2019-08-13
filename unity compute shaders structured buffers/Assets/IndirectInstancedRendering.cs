using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct transformData
{
    public Matrix4x4 transformMatrix;
    public Vector4 color;
    public Vector3 velocity;
    public float mass;
}; // 24 float values

public class IndirectInstancedRendering : MonoBehaviour
{
    public int instanceCount = 1000;

    public Mesh instanceMesh;

    // this time, we will have to use a custom shader to deal with instance rending on the GPU
    public Material instanceMaterial;
    int subMeshIndex = 0;

    public ComputeShader transformationShader;
    ComputeBuffer dataBuffer;
    transformData[] dataArray;
    int translationKernel;

    ComputeBuffer transformBuffer;
    
    ComputeBuffer argsBuffer;
    uint[] args;

    bool loaded;

    void Start()
    {
        loaded = false;
        translationKernel = transformationShader.FindKernel("Translate");

        transformBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16);

        dataBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 24);
        dataArray = new transformData[instanceCount];
        
        GenerateTransforms();

        args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        GenerateArguments();
    }
    
    void Update()
    {
        if (loaded)
        {
            transformationShader.SetFloat("deltaTime", Time.deltaTime);
            transformationShader.Dispatch(translationKernel, instanceCount / 64, 1, 1);
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
        }
    }

    /// <summary>
    /// This method is used to generate random transforms for each of the instances
    /// </summary>
    private void GenerateTransforms()
    {
        Vector3 pos = new Vector3();
        Quaternion rot = new Quaternion();
        Vector3 scale = new Vector3();
        for (int i = 0; i < instanceCount; i++)
        {
            pos = Random.insideUnitSphere * 50;
            rot = Random.rotation;
            scale = new Vector3(Random.Range(0, 2.0f), Random.Range(0, 2.0f), Random.Range(0, 2.0f));
            dataArray[i].transformMatrix = Matrix4x4.TRS(pos, rot, scale);
        }

        // set starting velocity
        for (int i = 0; i < instanceCount; i++)
        {
            dataArray[i].velocity = new Vector3(Random.Range(0, 2.0f), Random.Range(0, 2.0f), Random.Range(0, 2.0f));
        }

        // set starting color
        for (int i = 0; i < instanceCount; i++)
        {
            dataArray[i].color = new Vector4(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
        }

        // set starting mass
        for (int i = 0; i < instanceCount; i++)
        {
            dataArray[i].mass = Random.Range(1, 5f);
        }

        dataBuffer.SetData(dataArray);

        instanceMaterial.SetBuffer("transformBuffer", transformBuffer);
        transformationShader.SetBuffer(translationKernel, "transformBuffer", transformBuffer);

        instanceMaterial.SetBuffer("dataBuffer", dataBuffer);
        transformationShader.SetBuffer(translationKernel, "dataBuffer", dataBuffer);
    }

    private void GenerateArguments()
    {
        if (instanceMesh != null)
        {
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1); // make sure submesh index is valid
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);
        loaded = true;
    }

    /// <summary>
    /// This is being added because buffers are not automatically released by the garbage collector
    /// </summary>
    private void OnDestroy()
    {
        if(dataBuffer != null)
        {
            dataBuffer.Release();
            dataBuffer = null;
        }

        if(argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }

        if (transformBuffer != null)
        {
            transformBuffer.Release();
            transformBuffer = null;
        }
    }
}
