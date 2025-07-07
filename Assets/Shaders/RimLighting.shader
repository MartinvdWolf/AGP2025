Shader"Custom/RimLighting"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _NormalMap ( "Normal Map", 2D) = "bump" {}
        _HeightMap ("Height Map", 2D) = "black" {}

        _Color ("Main Color", Color) = (1,1,1,1)

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Parallax ("Height Map strength", Range(0.0, 0.1)) = 0.02

        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimIntensity ("Rim Intensity", Range(0.1, 8.0)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        // Physically based Standard lighting model, to enable normal shader functionality
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

sampler2D _MainTex;
sampler2D _NormalMap;
sampler2D _HeightMap;

fixed4 _Color;
fixed4 _RimColor;
float _RimIntensity;
half _Glossiness;
half _Metallic;
float _Parralax;


struct Input
{
	float2 uv_MainTex;
	float2 uv_NormalMap;
	float2 uv_HeightMap;
	float3 viewDir;
};


void surf(Input IN, inout SurfaceOutputStandard o)
{
    //regular shader setup
	fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = texColor.rgb;
    
	o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
    
	float height = tex2D(_HeightMap, IN.uv_HeightMap).r;
	float2 parallaxOffset = height * _Parralax * normalize(IN.viewDir).xy;
	IN.uv_MainTex += parallaxOffset;
	IN.uv_NormalMap += parallaxOffset;
    
    
    //Rimlighting: Does not play well with regular emission. Turned off for the Sun
	float rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
	rim = pow(rim, _RimIntensity);
	fixed3 rimEm = _RimColor.rgb * rim;
    
	o.Emission = rimEm;
    //end rimlighting
    
	o.Metallic = _Metallic;
	o.Smoothness = _Glossiness;
}

        ENDCG
    }
FallBack"Standard"
}
