// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/PowerupGlow"
{
	Properties
	{
		_GlowColour("GlowColour", Color) = (0,1,0,0)
		_RimGlow("Rim Glow", Range( 0 , 1)) = 0.5
		_BodyGlow("Body Glow", Range( 0 , 1)) = 0.25
		[NoScaleOffset]_BodyNoise("Body Noise", 2D) = "white" {}
		_NoiseTiling("Noise Tiling", Range( 0.1 , 10)) = 1
		_NoiseSpeed("Noise Speed", Range( 0 , 1)) = 0.05
		_Albedo("Albedo", 2D) = "white" {}
		_Normals("Normals", 2D) = "bump" {}
		_Metallic("Metallic", 2D) = "white" {}
		_Occlusion("Occlusion", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		ZTest LEqual
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 viewDir;
			INTERNAL_DATA
		};

		uniform sampler2D _Normals;
		uniform half4 _Normals_ST;
		uniform sampler2D _Albedo;
		uniform half4 _Albedo_ST;
		uniform float4 _GlowColour;
		uniform float _RimGlow;
		uniform sampler2D _BodyNoise;
		uniform half _NoiseTiling;
		uniform half _NoiseSpeed;
		uniform half _BodyGlow;
		uniform sampler2D _Metallic;
		uniform half4 _Metallic_ST;
		uniform sampler2D _Occlusion;
		uniform half4 _Occlusion_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			half3 normals30 = UnpackNormal( tex2D( _Normals, uv_Normals ) );
			o.Normal = normals30;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			half4 tex2DNode1 = tex2D( _Albedo, uv_Albedo );
			half4 albedo61 = tex2DNode1;
			o.Albedo = albedo61.rgb;
			half dotResult21 = dot( normals30 , i.viewDir );
			half glowRim47 = pow( ( 1.0 - saturate( dotResult21 ) ) , ( ( 1.0 - _RimGlow ) * 5.0 ) );
			half2 temp_cast_1 = (_NoiseTiling).xx;
			half mulTime44 = _Time.y * _NoiseSpeed;
			half2 temp_cast_2 = (mulTime44).xx;
			float2 uv_TexCoord42 = i.uv_texcoord * temp_cast_1 + temp_cast_2;
			half4 glowBody49 = ( tex2D( _BodyNoise, uv_TexCoord42 ) * _BodyGlow );
			half textureAlpha55 = tex2DNode1.a;
			half4 glow32 = ( _GlowColour * ( glowRim47 + glowBody49 ) * textureAlpha55 );
			o.Emission = glow32.rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			half4 metallic62 = tex2D( _Metallic, uv_Metallic );
			o.Metallic = metallic62.r;
			float2 uv_Occlusion = i.uv_texcoord * _Occlusion_ST.xy + _Occlusion_ST.zw;
			half4 occlusion63 = tex2D( _Occlusion, uv_Occlusion );
			o.Occlusion = occlusion63.r;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18200
500;285;1409;873;7594.447;2179.967;6.745283;True;True
Node;AmplifyShaderEditor.CommentaryNode;68;-3240.786,-1171.28;Inherit;False;737.2456;1014.635;;9;63;61;62;2;4;55;1;30;3;Standard Textures;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;3;-3156.999,-826.0605;Inherit;True;Property;_Normals;Normals;7;0;Create;True;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-2760.247,-825.3514;Inherit;False;normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;58;-3237.644,-56.68888;Inherit;False;1345.325;485.9087;;10;47;26;53;5;20;54;28;21;36;22;Rim Glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;59;-3228.314,529.7694;Inherit;False;1621.448;487.5085;;8;49;43;51;42;44;46;71;72;Body Glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-3182.434,27.0191;Inherit;False;30;normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;22;-3196.029,108.7136;Float;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;46;-3186.153,882.6778;Inherit;False;Property;_NoiseSpeed;Noise Speed;5;0;Create;True;0;0;False;0;False;0.05;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-2987.388,305.9409;Float;False;Property;_RimGlow;Rim Glow;1;0;Create;True;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;21;-2840.739,89.42926;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;44;-2904.355,887.0737;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-3186.155,671.1653;Inherit;False;Property;_NoiseTiling;Noise Tiling;4;0;Create;True;0;0;False;0;False;1;1;0.1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;54;-2681.006,311.3772;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;42;-2703.695,652.7721;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;20;-2685.128,88.84735;Inherit;False;1;0;FLOAT;1.23;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;5;-2520.062,88.1496;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;71;-2428.803,624.9428;Inherit;True;Property;_BodyNoise;Body Noise;3;1;[NoScaleOffset];Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;51;-2417.244,901.5637;Inherit;False;Property;_BodyGlow;Body Glow;2;0;Create;True;0;0;False;0;False;0.25;0.25;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-2504.006,311.3772;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;26;-2306.219,199.0951;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-2039.548,766.2974;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.25;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;60;-3218.321,1129.945;Inherit;False;953.229;519.3838;;7;32;27;35;25;56;48;50;Combined Glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;-2120.808,193.5071;Inherit;False;glowRim;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-1847.026,761.2488;Inherit;False;glowBody;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-3150.334,-1050.584;Inherit;True;Property;_Albedo;Albedo;6;0;Create;True;0;0;False;0;False;-1;None;84508b93f15f2b64386ec07486afc7a3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;50;-3156.265,1432.079;Inherit;False;49;glowBody;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-3149.743,1342.939;Inherit;False;47;glowRim;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-2762.013,-958.8411;Inherit;False;textureAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;56;-2973.164,1528.63;Inherit;False;55;textureAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;-2887.194,1381.631;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;25;-2965.484,1207.144;Float;False;Property;_GlowColour;GlowColour;0;0;Create;True;0;0;False;0;False;0,1,0,0;0,1,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-3152.555,-608.8218;Inherit;True;Property;_Metallic;Metallic;8;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-2692.546,1287.847;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;-3150.413,-385.2427;Inherit;True;Property;_Occlusion;Occlusion;9;0;Create;True;0;0;False;0;False;-1;None;a8de9c9c15d9c7e4eaa883c727391bee;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;-2757.969,-384.0434;Inherit;False;occlusion;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;32;-2508.433,1282.724;Inherit;False;glow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-2758.924,-1050.971;Inherit;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;67;-3210.452,1752.301;Inherit;False;684.4301;690.518;;6;0;64;65;66;31;33;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;62;-2755.502,-608.3331;Inherit;False;metallic;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-3139.428,2099.306;Inherit;False;62;metallic;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;33;-3139.368,2014.45;Inherit;False;32;glow;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-3138.868,1938.178;Inherit;False;30;normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-3142.104,1857.587;Inherit;False;61;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-3145.516,2184.535;Inherit;False;63;occlusion;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-2790.065,1959.509;Half;False;True;-1;2;;0;0;Standard;NeoFPS/Standard/PowerupGlow;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;3;0
WireConnection;21;0;36;0
WireConnection;21;1;22;0
WireConnection;44;0;46;0
WireConnection;54;0;28;0
WireConnection;42;0;72;0
WireConnection;42;1;44;0
WireConnection;20;0;21;0
WireConnection;5;0;20;0
WireConnection;71;1;42;0
WireConnection;53;0;54;0
WireConnection;26;0;5;0
WireConnection;26;1;53;0
WireConnection;43;0;71;0
WireConnection;43;1;51;0
WireConnection;47;0;26;0
WireConnection;49;0;43;0
WireConnection;55;0;1;4
WireConnection;35;0;48;0
WireConnection;35;1;50;0
WireConnection;27;0;25;0
WireConnection;27;1;35;0
WireConnection;27;2;56;0
WireConnection;63;0;4;0
WireConnection;32;0;27;0
WireConnection;61;0;1;0
WireConnection;62;0;2;0
WireConnection;0;0;64;0
WireConnection;0;1;31;0
WireConnection;0;2;33;0
WireConnection;0;3;65;0
WireConnection;0;5;66;0
ASEEND*/
//CHKSM=222CAB2D59813C8303015D851990E43D2A932852