// Toony Colors Pro+Mobile 2
// (c) 2014-2021 Jean Moreno

Shader "Coda Platform/Toon/Grass (Animated)"
{
	Properties
	{
		[Header(Base)]
		[Space] _Color ("Color", Color) = (1,1,1,1)
		_ColorBack ("Color Backfaces", Color) = (1,1,1,1)
		_HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		_SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		[Space(15)] _MainTex ("Albedo", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5

		[Header(Ramp Shading)]
		[Space] _RampThreshold ("Threshold", Range(0.01,1)) = 0.15
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.35
		
		[Header(Wind)]
		[Space][Toggle(TCP2_WIND)] _UseWind ("Enable Wind", Float) = 0
		[Space] _WindDirection ("Direction", Vector) = (1,0,0,0)
		_WindStrength ("Strength", Range(0,0.2)) = 0.025
		_WindTimeOffset ("Wind Time Offset Range", Range(0,1)) = 1
		_WindSpeed ("Speed", Range(0,10)) = 2.5
		_WindFrequency ("Frequency", Range(0,5)) = 0.5
	}

	SubShader
	{
		Tags
		{
			"RenderType"="TransparentCutout"
			"Queue"="Transparent"
		}
		
		CGINCLUDE

		#include "UnityCG.cginc"
		#include "UnityLightingCommon.cginc"	// needed for LightColor

		// Shader Properties
		sampler2D _MainTex;
		
		// Shader Properties
		float _WindTimeOffset;
		float _WindSpeed;
		float _WindFrequency;
		float4 _WindDirection;
		float _WindStrength;
		float4 _MainTex_ST;
		float _Cutoff;
		fixed4 _Color;
		fixed4 _ColorBack;
		float _RampThreshold;
		float _RampSmoothing;
		fixed4 _HColor;
		fixed4 _SColor;
		
		ENDCG

		// Main Surface Shader
		AlphaToMask On
		Cull Off

		CGPROGRAM

		#pragma surface surf ToonyColorsCustom vertex:vertex_surface exclude_path:deferred exclude_path:prepass keepalpha noforwardadd addshadow nolightmap nolppv
		#pragma target 3.0

		//================================================================
		// SHADER KEYWORDS

		#pragma shader_feature TCP2_WIND

		//================================================================
		// STRUCTS

		//Vertex input
		struct appdata_tcp2
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord0 : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
		#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
			half4 tangent : TANGENT;
		#endif
			fixed4 vertexColor : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input
		{
			float vFace : VFACE;
			float2 texcoord0;
		};

		//================================================================
		// VERTEX FUNCTION

		void vertex_surface(inout appdata_tcp2 v, out Input output)
		{
			UNITY_INITIALIZE_OUTPUT(Input, output);

			// Texture Coordinates
			output.texcoord0.xy = v.texcoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			// Shader Properties Sampling
			float __windTimeOffset = ( v.vertexColor.g * _WindTimeOffset );
			float __windSpeed = ( _WindSpeed );
			float __windFrequency = ( _WindFrequency );
			float4 __windSineScale2 = ( float4(2.3,1.7,1.4,1.2) );
			float __windSineStrength2 = ( .6 );
			float4 __windSineScale3 = ( float4(1.3,2.9,2.1,0.8) );
			float __windSineStrength3 = ( .5 );
			float4 __windSineScale4 = ( float4(3.4,2.6,3.1,1.5) );
			float __windSineStrength4 = ( .2 );
			float3 __windDirection = ( _WindDirection.xyz );
			float3 __windMask = ( v.vertexColor.rrr );
			float __windStrength = ( _WindStrength );

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			#if defined(TCP2_WIND)
			// Wind Animation
			float windTimeOffset = __windTimeOffset;
			float windSpeed = __windSpeed;
			float3 windFrequency = worldPos.xyz * __windFrequency;
			float windPhase = (_Time.y + windTimeOffset) * windSpeed;
			float3 windFactor = sin(windPhase + windFrequency);
			float4 windSin2scale = __windSineScale2;
			float windSin2strength = __windSineStrength2;
			windFactor += sin(windPhase.xxx * windSin2scale.www + windFrequency * windSin2scale.xyz) * windSin2strength;
			float4 windSin3scale = __windSineScale3;
			float windSin3strength = __windSineStrength3;
			windFactor += sin(windPhase.xxx * windSin3scale.www + windFrequency * windSin3scale.xyz) * windSin3strength;
			float4 windSin4scale = __windSineScale4;
			float windSin4strength = __windSineStrength4;
			windFactor += sin(windPhase.xxx * windSin4scale.www + windFrequency * windSin4scale.xyz) * windSin4strength;
			float3 windDir = normalize(__windDirection);
			float3 windMask = __windMask;
			float windStrength = __windStrength;
			worldPos.xyz += windDir * windFactor * windMask * windStrength;
			#endif
			v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;

		}

		//================================================================

		//Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			half atten;
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Specular;
			half Gloss;
			half Alpha;

			Input input;
			
			// Shader Properties
			float __rampThreshold;
			float __rampSmoothing;
			float3 __highlightColor;
			float3 __shadowColor;
			float __ambientIntensity;
		};

		//================================================================
		// SURFACE FUNCTION

		void surf(Input input, inout SurfaceOutputCustom output)
		{

			// Shader Properties Sampling
			float4 __albedo = ( tex2D(_MainTex, input.texcoord0.xy).rgba );
			float4 __mainColor = (  lerp(_Color, _ColorBack, step(input.vFace,0.5)) );
			float __alpha = ( __albedo.a * __mainColor.a );
			float __cutoff = ( _Cutoff );
			output.__rampThreshold = ( _RampThreshold );
			output.__rampSmoothing = ( _RampSmoothing );
			output.__highlightColor = ( _HColor.rgb );
			output.__shadowColor = ( _SColor.rgb );
			output.__ambientIntensity = ( 1.0 );

			output.input = input;

			output.Albedo = __albedo.rgb;
			output.Alpha = __alpha;

			//Sharpen Alpha-to-Coverage
			output.Alpha = (output.Alpha - __cutoff) / max(fwidth(output.Alpha), 0.0001) + 0.5;
			
			output.Albedo *= __mainColor.rgb;

		}

		//================================================================
		// LIGHTING FUNCTION

		inline half4 LightingToonyColorsCustom(inout SurfaceOutputCustom surface, UnityGI gi)
		{
			half3 lightDir = gi.light.dir;
			#if defined(UNITY_PASS_FORWARDBASE)
				half3 lightColor = _LightColor0.rgb;
				half atten = surface.atten;
			#else
				//extract attenuation from point/spot lights
				half3 lightColor = _LightColor0.rgb;
				half atten = max(gi.light.color.r, max(gi.light.color.g, gi.light.color.b)) / max(_LightColor0.r, max(_LightColor0.g, _LightColor0.b));
			#endif

			half3 normal = normalize(surface.Normal);
			normal.xyz *= (surface.input.vFace < 0) ? -1.0 : 1.0;
			half ndl = dot(normal, lightDir);
			half3 ramp;
			
			// Wrapped Lighting
			ndl = ndl * 0.5 + 0.5;
			
			#define		RAMP_THRESHOLD	surface.__rampThreshold
			#define		RAMP_SMOOTH		surface.__rampSmoothing
			ndl = saturate(ndl);
			ramp = smoothstep(RAMP_THRESHOLD - RAMP_SMOOTH*0.5, RAMP_THRESHOLD + RAMP_SMOOTH*0.5, ndl);
			half3 rampGrayscale = ramp;

			//Apply attenuation (shadowmaps & point/spot lights attenuation)
			ramp *= atten;

			//Highlight/Shadow Colors
			#if !defined(UNITY_PASS_FORWARDBASE)
				ramp = lerp(half3(0,0,0), surface.__highlightColor, ramp);
			#else
				ramp = lerp(surface.__shadowColor, surface.__highlightColor, ramp);
			#endif

			//Output color
			half4 color;
			color.rgb = surface.Albedo * lightColor.rgb * ramp;
			color.a = surface.Alpha;

			// Apply indirect lighting (ambient)
			half occlusion = 1;
			#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
				half3 ambient = gi.indirect.diffuse;
				ambient *= surface.Albedo * occlusion * surface.__ambientIntensity;

				color.rgb += ambient;
			#endif

			return color;
		}

		void LightingToonyColorsCustom_GI(inout SurfaceOutputCustom surface, UnityGIInput data, inout UnityGI gi)
		{
			half3 normal = surface.Normal;

			//GI without reflection probes
			gi = UnityGlobalIllumination(data, 1.0, normal); // occlusion is applied in the lighting function, if necessary

			surface.atten = data.atten; // transfer attenuation to lighting function
			gi.light.color = _LightColor0.rgb; // remove attenuation

		}

		ENDCG

	}

	Fallback "Diffuse"
}