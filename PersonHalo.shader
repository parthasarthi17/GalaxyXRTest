// Camera image + a solid outline and a soft light glow around people (not over them).
// Put this on the screen's material. PersonHaloDetector fills _MaskTex:
//   R = person, G = glow (brightest at the body's edge, fading outward).
// The mask is read with the same image coordinates as the camera image,
// so the glow follows the fisheye warp, flips and screen size automatically.
Shader "Custom/PersonHalo"
{
    Properties
    {
        _MainTex ("Camera Image", 2D) = "white" {}
        _MaskTex ("Person Mask (set by script)", 2D) = "black" {}
        _GlowColor ("Glow Color", Color) = (0.6, 0.9, 1, 1)
        _GlowStrength ("Glow Strength", Range(0, 2)) = 0.8
        _LineColor ("Outline Color", Color) = (0, 1, 1, 1)
        _LineWidth ("Outline Width (0 = off)", Range(0, 0.03)) = 0.005
        _Aspect ("Image Width / Height", Float) = 1.78
        [Toggle] _FlipMaskY ("Flip Mask Up/Down (if glow is mirrored)", Float) = 0
        [Toggle] _ShowMask ("Debug: Show Mask", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MaskTex;

            fixed4 _GlowColor;
            float _GlowStrength;
            fixed4 _LineColor;
            float _LineWidth;
            float _Aspect;
            float _FlipMaskY;
            float _ShowMask;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // needed to render in both eyes
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 SampleMask(float2 uv)
            {
                if (_FlipMaskY > 0.5) uv.y = 1.0 - uv.y;
                float inImage = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);
                return tex2D(_MaskTex, uv) * inImage;
            }

            float Body(float2 uv)
            {
                return smoothstep(0.4, 0.6, SampleMask(uv).r);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                fixed4 cam = tex2D(_MainTex, i.uv);
                float4 mask = SampleMask(i.uv);
                float body = smoothstep(0.4, 0.6, mask.r);

                if (_ShowMask > 0.5)
                    return lerp(cam * 0.3, fixed4(1, 1, 1, 1), body);

                // 1. Soft glow outside the body, added like light.
                float glow = mask.g * (1.0 - body) * _GlowStrength;
                float3 col = saturate(cam.rgb + _GlowColor.rgb * glow);

                // 2. Solid outline just outside the body edge, drawn on top.
                //    (Width 0 gives no outline.)
                float near = body;
                [unroll]
                for (int k = 0; k < 16; k++)
                {
                    float ang = k * 0.3926991; // 22.5 degree steps
                    float2 dir = float2(cos(ang) / _Aspect, sin(ang)) * _LineWidth;
                    near = max(near, Body(i.uv + dir));
                    near = max(near, Body(i.uv + dir * 0.5));
                }
                float edge = saturate(near - body);
                col = lerp(col, _LineColor.rgb, edge);

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
