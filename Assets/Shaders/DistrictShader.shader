Shader "Custom/DistrictShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        _BorderColor("Border Color", Color) = (0,0,0,1)
        _BorderWidth("Border Width", Range(0,0.2)) = 0.02

        _Blink("Blink", Float) = 0
        _BlinkDuration("Blink Duration", Float) = 2
        _BlinkColor("Blink Color", Color) = (1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // 1. Pass: Paint black
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _BorderColor;

            // Build the object (goes through each vertex)
            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Draw in the object (goes through each pixel)
            fixed4 frag (v2f IN) : SV_Target
            {
                return _BorderColor;
            }
            ENDCG
        }

        
        // 2. Pass: Shrink in normal direction and paint normally
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 tangent: TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _BorderWidth;
            float4 _Color;
            float3 Offset = float3(0, 0, 0);
            float _Blink;
            float _BlinkDuration;
            float3 _BlinkColor;

            // Build the object (goes through each vertex)
            v2f vert(appdata IN)
            {
                v2f OUT;
                float3 ver = IN.vertex.xyz;
                ver = ver + (_BorderWidth * IN.tangent) + float3(0, 0.001, 0);
                OUT.vertex = UnityObjectToClipPos(ver);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Draw in the object (goes through each pixel)
            fixed4 frag(v2f IN) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, IN.uv);
                col = col * _Color;

                if (_Blink == 1.0f)
                {
                    float r = (0.5f + abs(sin(_Time.w * (1 / _BlinkDuration))));
                    col *= r;
                }
                return col;
            }
            ENDCG
        }
    }
}
