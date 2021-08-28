// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/Water"
{
	Properties
	{
		_SurfaceColour("SurfaceColour", Color) = (0.1745283,0.7499093,1,0.4)
		_WaterNormals("Water Normals", 2D) = "white" {}
		_NormalLerp("Normal Lerp", Range( 0 , 1)) = 0.5
		_TurbulenceSpeed("Turbulence Speed", Range( 0 , 1)) = 0
		_Turbulence("Turbulence", Vector) = (1,1,0.5,-0.5)
		_SurfaceScale1("Surface Scale 1", Range( 0 , 1)) = 0
		_SurfaceScale2("Surface Scale 2", Range( 0 , 1)) = 0
		_OffsetStrength("Offset Strength", Range( 0.01 , 1)) = 0
		_Reflections("Reflections", Range( 0 , 1)) = 0.75
		_FresnelStrength("Fresnel Strength", Range( 0 , 1)) = 0
		_CameraFade("Camera Fade", Range( 0.1 , 1)) = 0.5
		_DepthFade("Depth Fade", Range( 0 , 2)) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPos;
			float3 worldPos;
			float eyeDepth;
			float3 worldNormal;
			INTERNAL_DATA
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _WaterNormals;
		uniform float _TurbulenceSpeed;
		uniform float4 _Turbulence;
		uniform float _SurfaceScale1;
		uniform float _SurfaceScale2;
		uniform float _NormalLerp;
		uniform float _OffsetStrength;
		uniform float _CameraFade;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DepthFade;
		uniform float4 _SurfaceColour;
		uniform float _Reflections;
		uniform float _FresnelStrength;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 appendResult81 = (float4(ase_grabScreenPosNorm.r , ase_grabScreenPosNorm.g , 0.0 , 0.0));
			float mulTime99 = _Time.y * ( _TurbulenceSpeed * 0.25 );
			float2 appendResult92 = (float2(_Turbulence.x , _Turbulence.y));
			float3 ase_worldPos = i.worldPos;
			float2 appendResult95 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner86 = ( mulTime99 * appendResult92 + appendResult95);
			float lerpResult104 = lerp( 1.0 , 0.005 , _SurfaceScale1);
			float2 appendResult93 = (float2(_Turbulence.z , _Turbulence.w));
			float2 panner85 = ( mulTime99 * appendResult93 + ( appendResult95 * float2( 1,1 ) ));
			float lerpResult106 = lerp( 1.0 , 0.005 , _SurfaceScale2);
			float4 lerpResult83 = lerp( tex2D( _WaterNormals, ( panner86 * lerpResult104 ) ) , tex2D( _WaterNormals, ( panner85 * lerpResult106 ) ) , _NormalLerp);
			float4 normalizeResult204 = normalize( lerpResult83 );
			float4 normals87 = normalizeResult204;
			float cameraDepthFade141 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / _CameraFade);
			float cameraDepthFade143 = saturate( cameraDepthFade141 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth146 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth146 = saturate( abs( ( screenDepth146 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) ) );
			float edgeFade147 = distanceDepth146;
			float4 screenColor77 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( appendResult81 - ( normals87 * ( _OffsetStrength * 0.05 * cameraDepthFade143 * edgeFade147 ) ) ).xy);
			float4 distortion163 = screenColor77;
			float3 indirectNormal76 = WorldNormalVector( i , normals87.rgb );
			Unity_GlossyEnvironmentData g76 = UnityGlossyEnvironmentSetup( 1.0, data.worldViewDir, indirectNormal76, float3(0,0,0));
			float3 indirectSpecular76 = UnityGI_IndirectSpecular( data, 0.5, indirectNormal76, g76 );
			float4 waterSurface136 = ( _SurfaceColour + float4( ( indirectSpecular76 * _Reflections ) , 0.0 ) );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float dotResult153 = dot( ase_worldViewDir , ase_worldNormal );
			float lerpResult128 = lerp( 0.5 , 2.0 , _FresnelStrength);
			float fresnel132 = (( ( 1.0 - _FresnelStrength ) * 0.75 ) + (pow( ( 1.0 - saturate( abs( dotResult153 ) ) ) , lerpResult128 ) - 0.0) * (1.0 - ( ( 1.0 - _FresnelStrength ) * 0.75 )) / (1.0 - 0.0));
			float4 lerpResult114 = lerp( distortion163 , waterSurface136 , ( fresnel132 * cameraDepthFade143 * edgeFade147 ));
			c.rgb = lerpResult114.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=18909
2636;731;995;862;4633.256;-1627.281;1.816679;True;False
Node;AmplifyShaderEditor.CommentaryNode;166;-7000.194,2868.278;Inherit;False;2333.021;858.3408;;23;87;204;83;78;79;84;100;107;86;104;85;106;99;92;101;93;105;103;110;95;90;94;98;Panned Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-6767.917,3452.769;Inherit;False;Property;_TurbulenceSpeed;Turbulence Speed;3;0;Create;True;0;0;0;False;0;False;0;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;94;-6958.394,2985.594;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector4Node;90;-6574.161,3256.417;Inherit;False;Property;_Turbulence;Turbulence;4;0;Create;True;0;0;0;False;0;False;1,1,0.5,-0.5;0.7,2,-0.8,-0.4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;95;-6744.12,3017.684;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-6471.208,3456.192;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;99;-6219.18,3325.042;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;92;-6235.161,3196.417;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-6540.852,3127.92;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-6348.385,3609.988;Inherit;False;Property;_SurfaceScale2;Surface Scale 2;6;0;Create;True;0;0;0;False;0;False;0;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-6357.476,3046.805;Inherit;False;Property;_SurfaceScale1;Surface Scale 1;5;0;Create;True;0;0;0;False;0;False;0;0.8;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;93;-6231.161,3427.417;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;106;-5998.898,3574.008;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.005;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;85;-5937.872,3404.975;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;104;-5975.808,3012.167;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.005;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;86;-5943.301,3173.652;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;168;-7009.752,1651.293;Inherit;False;1111.383;277.0428;;4;143;142;141;139;Camera Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;203;-5785.693,1654.707;Inherit;False;1013.445;231.1614;Comment;3;147;146;148;Edge Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-5760.371,3011.297;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;-5783.461,3573.139;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-6930.717,1800.077;Inherit;False;Property;_CameraFade;Camera Fade;10;0;Create;True;0;0;0;False;0;False;0.5;1;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;169;-7012.139,873.3366;Inherit;False;1789.319;644.3367;;13;152;153;151;154;132;160;159;126;158;157;156;128;155;Fresnel Effect;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;152;-6932.213,1147.714;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CameraDepthFade;141;-6590.15,1784.238;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;-5699.321,1763.823;Inherit;False;Property;_DepthFade;Depth Fade;11;0;Create;True;0;0;0;False;0;False;1;0.2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;151;-6923.682,988.8984;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;84;-5562.228,3595.565;Inherit;False;Property;_NormalLerp;Normal Lerp;2;0;Create;True;0;0;0;False;0;False;0.5;0.51;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;79;-5576.923,3377.129;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;78;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;78;-5571.829,3143.85;Inherit;True;Property;_WaterNormals;Water Normals;1;0;Create;True;0;0;0;False;0;False;-1;None;d6ec2faceac37fe418118f4a136fff23;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;153;-6673.063,1043.381;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;83;-5230.139,3259.648;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;146;-5340.813,1745.107;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;142;-6323.789,1783.217;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;143;-6164.624,1778.241;Inherit;False;cameraDepthFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;154;-6516.067,1042.887;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;-5034.961,1741.27;Inherit;False;edgeFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;167;-7004.262,2084.74;Inherit;False;1407.585;614.6875;;11;163;77;82;109;81;88;80;134;108;145;150;Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;204;-5058,3260;Inherit;False;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;126;-6589.615,1312.854;Inherit;False;Property;_FresnelStrength;Fresnel Strength;9;0;Create;True;0;0;0;False;0;False;0;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;145;-6929.654,2514.495;Inherit;False;143;cameraDepthFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;155;-6371.326,1043.532;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;-6888.726,2589.53;Inherit;False;147;edgeFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;87;-4889.716,3254.615;Inherit;False;normals;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;165;-5477.489,2133.063;Inherit;False;1153.817;567.0095;Comment;7;136;135;123;121;73;125;76;Water Surface;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-6961.14,2438.441;Inherit;False;Property;_OffsetStrength;Offset Strength;7;0;Create;True;0;0;0;False;0;False;0;0.5;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;156;-6192.326,1042.532;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;159;-6188.731,1417.985;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-6630.026,2442.548;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0.05;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;128;-6182.722,1270.598;Inherit;False;3;0;FLOAT;0.5;False;1;FLOAT;2;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;80;-6715.49,2175.869;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;88;-6879.602,2359.241;Inherit;False;87;normals;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-5436.744,2453.271;Inherit;False;87;normals;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-6396.295,2361.022;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.05;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;81;-6432.546,2203.53;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-5199.099,2589.088;Inherit;False;Property;_Reflections;Reflections;8;0;Create;True;0;0;0;False;0;False;0.75;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;160;-5954.453,1262.572;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.75;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectSpecularLight;76;-5176.815,2458.593;Inherit;False;Tangent;3;0;FLOAT3;0,0,1;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;157;-5949.326,1040.532;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;73;-5183.216,2190.872;Inherit;False;Property;_SurfaceColour;SurfaceColour;0;0;Create;True;0;0;0;True;0;False;0.1745283,0.7499093,1,0.4;0.0941176,0.2053906,0.3137255,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;82;-6231.542,2203.137;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCRemapNode;158;-5759.307,1039.992;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-4889.577,2458.557;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenColorNode;77;-6050.617,2197.42;Inherit;False;Global;_GrabScreen0;Grab Screen 0;1;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;162;-3857.079,1771.284;Inherit;False;1048.697;698.2766;Comment;8;75;114;164;140;137;149;144;133;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;132;-5460.647,1034.688;Inherit;False;fresnel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;135;-4714.301,2194.866;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;136;-4561.231,2189.178;Inherit;False;waterSurface;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-3745.927,2195.921;Inherit;False;132;fresnel;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;144;-3810.697,2274.533;Inherit;False;143;cameraDepthFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-3762.565,2357.183;Inherit;False;147;edgeFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;163;-5843.11,2198.64;Inherit;False;distortion;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;140;-3542.219,2255.254;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;137;-3774.88,2108.229;Inherit;False;136;waterSurface;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;164;-3750.487,2027.831;Inherit;False;163;distortion;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;114;-3332.062,2093.077;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;75;-3062.391,1860.992;Float;False;True;-1;2;;0;0;CustomLighting;NeoFPS/Standard/Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;95;0;94;1
WireConnection;95;1;94;3
WireConnection;110;0;98;0
WireConnection;99;0;110;0
WireConnection;92;0;90;1
WireConnection;92;1;90;2
WireConnection;101;0;95;0
WireConnection;93;0;90;3
WireConnection;93;1;90;4
WireConnection;106;2;105;0
WireConnection;85;0;101;0
WireConnection;85;2;93;0
WireConnection;85;1;99;0
WireConnection;104;2;103;0
WireConnection;86;0;95;0
WireConnection;86;2;92;0
WireConnection;86;1;99;0
WireConnection;100;0;86;0
WireConnection;100;1;104;0
WireConnection;107;0;85;0
WireConnection;107;1;106;0
WireConnection;141;0;139;0
WireConnection;79;1;107;0
WireConnection;78;1;100;0
WireConnection;153;0;151;0
WireConnection;153;1;152;0
WireConnection;83;0;78;0
WireConnection;83;1;79;0
WireConnection;83;2;84;0
WireConnection;146;0;148;0
WireConnection;142;0;141;0
WireConnection;143;0;142;0
WireConnection;154;0;153;0
WireConnection;147;0;146;0
WireConnection;204;0;83;0
WireConnection;155;0;154;0
WireConnection;87;0;204;0
WireConnection;156;0;155;0
WireConnection;159;0;126;0
WireConnection;134;0;108;0
WireConnection;134;2;145;0
WireConnection;134;3;150;0
WireConnection;128;2;126;0
WireConnection;109;0;88;0
WireConnection;109;1;134;0
WireConnection;81;0;80;1
WireConnection;81;1;80;2
WireConnection;160;0;159;0
WireConnection;76;0;121;0
WireConnection;157;0;156;0
WireConnection;157;1;128;0
WireConnection;82;0;81;0
WireConnection;82;1;109;0
WireConnection;158;0;157;0
WireConnection;158;3;160;0
WireConnection;123;0;76;0
WireConnection;123;1;125;0
WireConnection;77;0;82;0
WireConnection;132;0;158;0
WireConnection;135;0;73;0
WireConnection;135;1;123;0
WireConnection;136;0;135;0
WireConnection;163;0;77;0
WireConnection;140;0;133;0
WireConnection;140;1;144;0
WireConnection;140;2;149;0
WireConnection;114;0;164;0
WireConnection;114;1;137;0
WireConnection;114;2;140;0
WireConnection;75;13;114;0
ASEEND*/
//CHKSM=4861F295E0B8A4B14A823F030DEF12361E670426