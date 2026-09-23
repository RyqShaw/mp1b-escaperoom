Shader "PassengerRoom/Outline"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color", Color) = (1,0.82,0.02,1)
        _OutlineWidth ("Width in metres", Float) = 0.004
        [HideInInspector] _BoundsCenter ("Mesh Center", Vector) = (0,0,0,0)
        [HideInInspector] _BoundsExtents ("Mesh Extents", Vector) = (1,1,1,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float4 _BoundsCenter;
                float4 _BoundsExtents;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                // Expand the hull on each local axis. Works with unreadable imported FBX meshes
                // and hard-edged, non-uniformly scaled lock plates without normal seams.
                float3 axisScale = float3(length(TransformObjectToWorldDir(float3(1,0,0), false)),
                    length(TransformObjectToWorldDir(float3(0,1,0), false)),
                    length(TransformObjectToWorldDir(float3(0,0,1), false)));
                float3 outward = (input.positionOS.xyz - _BoundsCenter.xyz) / max(_BoundsExtents.xyz, 0.00001);
                float3 expanded = input.positionOS.xyz + outward * _OutlineWidth / max(axisScale, 0.00001);
                output.positionCS = TransformObjectToHClip(expanded);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }
    }
}
