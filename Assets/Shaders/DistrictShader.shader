Shader "Custom/DistrictShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        _BorderColor("Border Color", Color) = (0,0,0,1)
        _BorderWidth("Border Width", Range(0,0.2)) = 0.02

        [Toggle] _Blink("Blink", Float) = 0
        _BlinkDuration("Blink Duration", Float) = 2

        [Toggle] _AnimatedHighlight("Animated Highlight", Float) = 0
        _AnimatedHighlightColor("Animated Highlight Color", Color) = (1, 0.5, 0, 0.5)
        _AnimatedHighlightSpeed("Animated Highlight Speed", Range(0.001, 0.1)) = 0.05
        _AnimatedHighlightSize("Animated Highlight Size", Range(0.02, 0.2)) = 0.1
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        // 1. Pass: Paint normally in border color
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

        
        // 2. Pass: Shrink in tangent direction and paint normally
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
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _BorderWidth;
            float4 _Color;
            float3 Offset = float3(0, 0, 0);
            float _Blink;
            float _BlinkDuration;
            float _AnimatedHighlight;
            float4 _AnimatedHighlightColor;
            float _AnimatedHighlightSpeed;
            float _AnimatedHighlightSize;

            // Build the object (goes through each vertex)
            v2f vert(appdata IN)
            {
                v2f OUT;
                float3 ver = IN.vertex.xyz;
                ver = ver + (_BorderWidth * IN.tangent) + float3(0, 0.001, 0);
                OUT.vertex = UnityObjectToClipPos(ver);
                OUT.worldPosition = IN.vertex;
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

                if (_AnimatedHighlight == 1.0f)
                {
                    float timeMod = (_Time.w % (1 / _AnimatedHighlightSpeed)) / (1 / _AnimatedHighlightSpeed);
                    float posValue = IN.worldPosition.x + IN.worldPosition.z;
                    float colVal = (posValue + timeMod) % _AnimatedHighlightSize;
                    if (colVal > (_AnimatedHighlightSize / 4) && colVal < (_AnimatedHighlightSize / 4 * 3))
                    {
                        col = _AnimatedHighlightColor.a * (_AnimatedHighlightColor) + (1- _AnimatedHighlightColor.a) * col;
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}
