#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void MeshVertex_float(float3 In, float4 Down, float4 Up, out float3 Out)
{
    // ���U
    if (In.x == -0.5 && In.y == -0.5)
    {
        In.x = Down.x;
        In.y = Down.y;
    }

    //�k�U
    if (In.x == 0.5 && In.y == -0.5)
    {
        In.x = Down.z;
        In.y = Down.w;
    }
    //���W
    if (In.x == -0.5 && In.y == 0.5)
    {
        In.x = Up.x;
        In.y = Up.y;
    }
    // �k�W
    if (In.x == 0.5 && In.y == 0.5)
    {
        In.x = Up.z;
        In.y = Up.w;
    }

    Out = In;
}
#endif