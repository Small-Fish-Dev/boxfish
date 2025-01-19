#ifndef VOXEL_UTILS
#define VOXEL_UTILS

static const float3 OffsetTable[8] = {
    float3( -0.5f, -0.5f, 0.5f ),
    float3( -0.5f, 0.5f, 0.5f ),
    float3( 0.5f, 0.5f, 0.5f ),
    float3( 0.5f, -0.5f, 0.5f ),
    float3( -0.5f, -0.5f, -0.5f ),
    float3( -0.5f, 0.5f, -0.5f ),
    float3( 0.5f, 0.5f, -0.5f ),
    float3( 0.5f, -0.5f, -0.5f )
};

static const float FaceMultipliers[6] = {
    1.0f, 1.0f,
    0.85f, 0.7f,
    0.85f, 0.7f
};

static const float3 FaceNormals[6] = {
    float3(0, 0, 1),
    float3(0, 0, -1),
    float3(-1, 0, 0),
    float3(0, 1, 0),
    float3(1, 0, 0),
    float3(0, -1, 0)
};

static const int2 UVTable[6][8] = {
    // +z, correct
    {
        int2( 1, 1 ),
        int2( 0, 1 ),
        int2( 0, 0 ),
        int2( 1, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 )
    },

    // -z, correct
    {
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 1 ),
        int2( 1, 1 ),
        int2( 1, 0 ),
        int2( 0, 0 )
    },

    // -x, correct
    {
        int2( 1, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 1, 1 ),
        int2( 0, 1 ),
        int2( 0, 0 ),
        int2( 0, 0 )
    },

    // +y, correct
    {
        int2( 0, 0 ),
        int2( 1, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 1, 1 ),
        int2( 0, 1 ),
        int2( 0, 0 )
    },

    // +x, correct
    {
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 1, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 1, 1 ),
        int2( 0, 1 )
    },

    // -y, correct
    {
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 1, 0 ),
        int2( 0, 1 ),
        int2( 0, 0 ),
        int2( 0, 0 ),
        int2( 1, 1 )
    },
};

class Voxel 
{
    static float3 GetOffset(uint vertexIndex) {
        return OffsetTable[vertexIndex];
    }

    static float3 GetFaceNormal(uint face) {
        return FaceNormals[face];
    }

    static float GetFaceMultiplier(uint face) {
        return FaceMultipliers[face];
    }

    static int2 GetUV(uint vertexIndex, uint face) {
        return UVTable[face][vertexIndex];
    }
};

#endif