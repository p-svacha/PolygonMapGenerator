Shader "Custom/DistrictShader"
{
    // District Shader that supports blink and animated highlight but no borders. Mesh borders should be used with this shader
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Toggle]_Blink("Blink", Float) = 0
        _BlinkDuration("Blink Duration", Float) = 2
        _BlinkColor("Blink Color", Color) = (1,1,1)

        [Toggle] _AnimatedHighlight("Animated Highlight", Float) = 0
        _AnimatedHighlightColor("Animated Highlight Color", Color) = (0, 0, 0, 0.5)
        _AnimatedHighlightSpeed("Animated Highlight Speed", Range(0.001, 0.1)) = 0.05
        _AnimatedHighlightSize("Animated Highlight Size", Range(0.02, 0.2)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Blink;
        float _BlinkDuration;
        float3 _BlinkColor;
        float _AnimatedHighlight;
        float4 _AnimatedHighlightColor;
        float _AnimatedHighlightSpeed;
        float _AnimatedHighlightSize;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            if (_Blink == 1.0f)
            {
                float r = (0.5f + abs(sin(_Time.w * (1 / _BlinkDuration))));
                c.rgb *= r;
            }
            if (_AnimatedHighlight == 1.0f)
            {
                float timeMod = (_Time.w % (1 / _AnimatedHighlightSpeed)) / (1 / _AnimatedHighlightSpeed);
                float posValue = IN.worldPos.x + IN.worldPos.z;
                float colVal = (posValue + timeMod) % _AnimatedHighlightSize;
                if (colVal > (_AnimatedHighlightSize / 4) && colVal < (_AnimatedHighlightSize / 4 * 3))
                {
                    c = _AnimatedHighlightColor.a * (_AnimatedHighlightColor)+(1 - _AnimatedHighlightColor.a) * c;
                }
            }
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
