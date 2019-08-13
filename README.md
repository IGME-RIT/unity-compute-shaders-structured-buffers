Documentation Author: Alexander Amling

# Unity Compute Shaders Structured Buffers

[part 3](https://github.com/IGME-RIT/unity-compute-shaders)

*Unity Compute Shaders Part 4*

As mentioned in part 3, compute buffers are very flexible, and compute shaders can accept structs to format the data passed in. For our code, we will use the following:

```
struct transformData 
{
    float4x4 transformationMatrix;
    float4 color;
    float3 velocity;
    float mass;
};
```
Which is the same as
```
public struct transformData {
    public Matrix4x4 transformMatrix;
    public Vector4 color;
    public Vector3 velocity;
    public float mass;
}; 
```
Both are 24 float values that can simply be passed into a buffer and read into the GPU to be put into the transformData format again
```
dataBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 24);
```
This allows us to easily keep track of a lot more data, and easily do more complex calculations. As an example, we're simulating gravity-like forces, but there is plenty more that can be done.
