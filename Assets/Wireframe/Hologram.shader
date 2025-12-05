Shader "Custom/Hologram"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0, 0.5, 1, 0.8)
        _ScanSpeed ("Scan Speed", Float) = 5.0
        _ScanFrequency ("Scan Frequency", Float) = 10.0
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
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };
            
            float4 _MainColor;
            float _ScanSpeed;
            float _ScanFrequency;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Scanline Effekt
                float scan = sin(i.worldPos.y * _ScanFrequency + _Time.y * _ScanSpeed) * 0.5 + 0.5;
                float4 color = _MainColor;
                color.rgb += scan * 0.3;
                color.a *= scan * 0.5 + 0.5;
                
                // Ränder heller machen
                float edge = abs(dot(normalize(i.worldNormal), normalize(_WorldSpaceCameraPos - i.worldPos)));
                edge = pow(1.0 - edge, 2.0);
                color.rgb += edge * 0.5;
                
                return color;
            }
            ENDCG
        }
    }
}