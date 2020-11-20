Shader "Custom/DistrictShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
    }
        SubShader
    {
        Pass
        {
            ZWrite Off
            ColorMask 0 // do not render any color
            Stencil {
            Ref 1
            Comp Always
            Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
            };
            struct v2f
            {
                float4 clipPos:SV_POSITION;
            };
            v2f vert(appdata v)
            {
                v2f o;
                v.vertex.x += 0.01 * v.vertex.x;
                v.vertex.z += 0.01 * v.vertex.z;
                o.clipPos = UnityObjectToClipPos(v.vertex * 0.98);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0,0,0,0);
            }
            ENDCG
        }


        Pass
        {
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _BaseColor;

            struct appdata
            {
                float4 vertex:POSITION;
            };
            struct v2f
            {
                float4 clipPos:SV_POSITION;
            };
            v2f vert(appdata v)
            {
                v2f o;
                o.clipPos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                return _BaseColor;
            }
            ENDCG
        }

        Pass
        {
            ZWrite Off
            Stencil {
            Ref 1
            Comp NotEqual
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _OutlineColor;

            struct appdata
            {
                float4 vertex:POSITION;
            };
            struct v2f
            {
                float4 clipPos:SV_POSITION;
            };
            v2f vert(appdata v)
            {
                v2f o;
                o.clipPos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
