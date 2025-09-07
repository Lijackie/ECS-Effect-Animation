#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void MeshVertex_float(float3 In, float4 Down, float4 Up, out float3 Out)
{
    // 左下
    if (In.x == -0.5 && In.y == -0.5)
    {
        In.x = Down.x;
        In.y = Down.y;
    }

    //右下
    if (In.x == 0.5 && In.y == -0.5)
    {
        In.x = Down.z;
        In.y = Down.w;
    }
    //左上
    if (In.x == -0.5 && In.y == 0.5)
    {
        In.x = Up.x;
        In.y = Up.y;
    }
    // 右上
    if (In.x == 0.5 && In.y == 0.5)
    {
        In.x = Up.z;
        In.y = Up.w;
    }

    Out = In;
}
#endif