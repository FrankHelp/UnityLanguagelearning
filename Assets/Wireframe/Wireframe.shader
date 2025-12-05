Shader "Custom/Wireframe"
{
    Properties
    {
        _WireColor ("Wire Color", Color) = (0, 1, 1, 1)
        _BaseColor ("Base Color", Color) = (0, 0.1, 0.2, 0.5)
        _WireThickness ("Wire Thickness", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2g
            {
                float4 vertex : POSITION;
            };
            
            struct g2f
            {
                float4 pos : SV_POSITION;
            };
            
            float4 _WireColor;
            float4 _BaseColor;
            float _WireThickness;
            
            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout LineStream<g2f> lineStream)
            {
                g2f o;
                for(int i = 0; i < 3; i++)
                {
                    o.pos = IN[i].vertex;
                    lineStream.Append(o);
                }
            }
            
            fixed4 frag (g2f i) : SV_Target
            {
                return _WireColor;
            }
            ENDCG
        }
    }
}