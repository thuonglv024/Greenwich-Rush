Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // Unity tự động điền thông số này

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Lấy pixel gốc
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Tính toán độ dày viền dựa trên kích thước pixel của ảnh
                float2 distance = _MainTex_TexelSize.xy * _OutlineSize;
                
                // Kiểm tra 4 hướng xung quanh (Trên, Dưới, Trái, Phải)
                fixed4 alpha = fixed4(0,0,0,0);
                alpha += tex2D(_MainTex, IN.texcoord + float2(distance.x, 0));
                alpha += tex2D(_MainTex, IN.texcoord + float2(-distance.x, 0));
                alpha += tex2D(_MainTex, IN.texcoord + float2(0, distance.y));
                alpha += tex2D(_MainTex, IN.texcoord + float2(0, -distance.y));

                // Nếu pixel hiện tại trong suốt (alpha thấp) 
                // nhưng các pixel lân cận có màu (alpha cao) -> Vẽ viền
                if (c.a < 0.1 && alpha.a > 0.1) {
                    c = _OutlineColor;
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}