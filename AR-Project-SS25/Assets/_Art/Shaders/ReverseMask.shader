Shader "UI/ReverseMask"
{
    Properties
    {
        _Color("Overlay Color", Color) = (0,0,0,1)
        _MaskTex("Mask Texture", 2D) = "white" {}
        _MaskRect("Mask Rect", Vector) = (0.3, 0.3, 0.7, 0.7)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _Color;
            float4 _MaskRect; // xMin, yMin, xMax, yMax

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Check if UV is inside the mask rect
                bool inside = i.uv.x > _MaskRect.x && i.uv.x < _MaskRect.z &&
                              i.uv.y > _MaskRect.y && i.uv.y < _MaskRect.w;

                float alpha = inside ? 0 : _Color.a; // transparent if inside

                return float4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}
