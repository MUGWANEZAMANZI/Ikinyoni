Shader "Hidden/DoodlePaint"
{
    Properties
    {
        // Declare "_Color" as Property to support implicit color space conversions
        _Color("Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Lighting Off
        Blend One Zero
        ZWrite Off
        Cull Off
        
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float4 _Pos;
            float  _Radius;
            float4 _Color;

            float squareDist(float2 a, float2 b, float2 c)
            {
                float2 ab = b - a;
                float2 ac = c - a;
                float2 bc = c - b;

                float e = dot(ac, ab);
                if (e <= 0.0)
                    return dot(ac, ac);

                float f = dot(ab, ab);
                if (e >= f)
                    return dot(bc, bc);
                return dot(ac, ac) - e * e / f;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 coord = i.uv;
                float2 a = float2(_Pos.x, _Pos.y);
                float2 b = float2(_Pos.z, _Pos.w);
                float2 c = float2(coord.x, coord.y);
                float sqDist = squareDist(a, b, c);
                float sqRadius = _Radius * _Radius;
                if(sqDist < sqRadius)
                    return _Color;
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}