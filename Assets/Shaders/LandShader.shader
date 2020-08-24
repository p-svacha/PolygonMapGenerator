Shader "Custom/Splatmap/Lightmap-FirstPass" {

    Properties{
        _Control("Control (RGBA)", 2D) = "red" {}
        _Splat3("Layer 3 (A)", 2D) = "white" {}
        _Splat2("Layer 2 (B)", 2D) = "white" {}
        _Splat1("Layer 1 (G)", 2D) = "white" {}
        _Splat0("Layer 0 (R)", 2D) = "white" {}
        _OverlayMask("Overlay Mask", 2D) = "white" {}
        _OverlayTexture("Overlay Texture", 2D) = "white" {}
    }

    SubShader{

        Tags {
            "SplatCount" = "4"
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Lambert
        #pragma target 3.5

        struct Input {
            float2 uv_Control;
            float2 uv_Splat0;
            float2 uv_Splat1;
            float2 uv_Splat2;
            float2 uv_Splat3;
            float2 uv_OverlayMask;
            float2 uv_OverlayTexture;
        };

        sampler2D _Control;
        sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
        sampler2D _OverlayMask, _OverlayTexture;

        void surf(Input IN, inout SurfaceOutput o) {
            fixed4 splat_control = tex2D(_Control, IN.uv_Control);
            fixed4 mask = tex2D(_OverlayMask, IN.uv_OverlayMask);
            fixed4 overlayTex = tex2D(_OverlayTexture, IN.uv_OverlayTexture);
            fixed3 col;
            col =  splat_control.r * tex2D(_Splat0, IN.uv_Splat0).rgb;
            col += splat_control.g * tex2D(_Splat1, IN.uv_Splat1).rgb;
            col += splat_control.b * tex2D(_Splat2, IN.uv_Splat2).rgb;
            col += splat_control.a * tex2D(_Splat3, IN.uv_Splat3).rgb;
            col = (mask * overlayTex) + ((1 - mask) * col);
            o.Albedo = col;
            o.Alpha = 0.0;
        }
        ENDCG
    }

}