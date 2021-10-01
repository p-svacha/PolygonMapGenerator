Shader "Unlit/MarchingAnts"
{
    Properties
    {
        _Color1("Color", Color) = (0,0,0,1)
        _Color2("Color", Color) = (1,1,1,0)
        _AntSpeed("Ant Speed", Range(0.001, 0.1)) = 0.05
        _AntSize("Ant Size", Range(0.02, 0.2)) = 0.1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 worldPosition : TEXCOORD1;
            };

            float4 _Color1;
            float4 _Color2;
            float _AntSpeed;
            float _AntSize;

            // Build the object (goes through each vertex)
            v2f vert(appdata IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.worldPosition = IN.vertex;
                OUT.uv = IN.uv;
                return OUT;
            }

            // Draw in the object (goes through each pixel)
            fixed4 frag(v2f IN) : SV_Target
            {
                float timeMod = (_Time.w % (1 / _AntSpeed)) / (1 / _AntSpeed);
                float posValue = IN.worldPosition.x + IN.worldPosition.z;
                float colVal = (posValue + timeMod) % _AntSize;
                if (colVal > (_AntSize / 4) && colVal < (_AntSize /4 * 3)) return _Color1;
                else 
                {
                    half4 color = fixed4(_Color2.rgb, 0);
                    return color;
                }
            }
            ENDCG
        }
    }
}
