// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/Water Caustics"
{
	Properties
	{
		_Colour("Colour", Color) = (0.1745283,0.7499093,1,0.4)
		_FadeDepth("Fade Depth", Range( 0.1 , 10)) = 2
		_CausticsRepeat("Caustics Repeat", Range( 0.1 , 10)) = 1.5
		_Exponent("Exponent", Range( 1 , 5)) = 2
		_TimeScale("Time Scale", Range( 0.1 , 10)) = 1
		_Brightness("Brightness", Range( 0 , 5)) = 1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend One OneMinusSrcAlpha
		AlphaToMask Off
		Cull Front
		ColorMask RGBA
		ZWrite Off
		ZTest Always
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			//This is a late directive
			
			uniform float4 _Colour;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _FadeDepth;
			uniform float _CausticsRepeat;
			uniform float _TimeScale;
			uniform float _Exponent;
			uniform sampler2D _CameraNormalsTexture;
			uniform float _Brightness;
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDir72_g1( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
					float2 voronoihash90( float2 p )
					{
						
						p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
						return frac( sin( p ) *43758.5453);
					}
			
					float voronoi90( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
					{
						float2 n = floor( v );
						float2 f = frac( v );
						float F1 = 8.0;
						float F2 = 8.0; float2 mg = 0;
						for ( int j = -1; j <= 1; j++ )
						{
							for ( int i = -1; i <= 1; i++ )
						 	{
						 		float2 g = float2( i, j );
						 		float2 o = voronoihash90( n + g );
								o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
								float d = 0.5 * dot( r, r );
						 		if( d<F1 ) {
						 			F2 = F1;
						 			F1 = d; mg = g; mr = r; id = o;
						 		} else if( d<F2 ) {
						 			F2 = d;
						 		}
						 	}
						}
						return (F2 + F1) * 0.5;
					}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord1 = screenPos;
				
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
#endif
				float4 screenPos = i.ase_texcoord1;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 UV22_g3 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g3 = UnStereo( UV22_g3 );
				float2 break64_g1 = localUnStereo22_g3;
				float clampDepth69_g1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g1 = ( 1.0 - clampDepth69_g1 );
				#else
				float staticSwitch38_g1 = clampDepth69_g1;
				#endif
				float3 appendResult39_g1 = (float3(break64_g1.x , break64_g1.y , staticSwitch38_g1));
				float4 appendResult42_g1 = (float4((appendResult39_g1*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g1 = mul( unity_CameraInvProjection, appendResult42_g1 );
				float3 In72_g1 = ( (temp_output_43_0_g1).xyz / (temp_output_43_0_g1).w );
				float3 localInvertDepthDir72_g1 = InvertDepthDir72_g1( In72_g1 );
				float4 appendResult49_g1 = (float4(localInvertDepthDir72_g1 , 1.0));
				float4 pixelWorldPos137 = mul( unity_CameraToWorld, appendResult49_g1 );
				float3 worldToObj78 = mul( unity_WorldToObject, float4( pixelWorldPos137.xyz, 1 ) ).xyz;
				float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
				float clampResult170 = clamp( ( ( (worldToObj78.y*-1.0 + 0.5) * ase_objectScale.y ) / _FadeDepth ) , 0.0 , 1.0 );
				float boxFade140 = ( ( abs( worldToObj78.x ) < 0.5 ? 1.0 : 0.0 ) * ( abs( worldToObj78.z ) < 0.5 ? 1.0 : 0.0 ) * clampResult170 );
				float mulTime92 = _Time.y * _TimeScale;
				float time90 = mulTime92;
				float3 worldSpaceLightDir = UnityWorldSpaceLightDir(WorldPosition);
				float3 temp_output_146_0 = cross( worldSpaceLightDir , float3( 0,1,0 ) );
				float3 normalizeResult167 = normalize( cross( worldSpaceLightDir , temp_output_146_0 ) );
				float dotResult149 = dot( float4( normalizeResult167 , 0.0 ) , pixelWorldPos137 );
				float3 normalizeResult168 = normalize( temp_output_146_0 );
				float dotResult150 = dot( float4( normalizeResult168 , 0.0 ) , pixelWorldPos137 );
				float2 appendResult151 = (float2(dotResult149 , dotResult150));
				float2 coords90 = appendResult151 * _CausticsRepeat;
				float2 id90 = 0;
				float2 uv90 = 0;
				float fade90 = 0.5;
				float voroi90 = 0;
				float rest90 = 0;
				for( int it90 = 0; it90 <2; it90++ ){
				voroi90 += fade90 * voronoi90( coords90, time90, id90, uv90, 0 );
				rest90 += fade90;
				coords90 *= 2;
				fade90 *= 0.5;
				}//Voronoi90
				voroi90 /= rest90;
				float voronoi153 = pow( voroi90 , _Exponent );
				float dotResult157 = dot( float4( worldSpaceLightDir , 0.0 ) , (tex2D( _CameraNormalsTexture, (ase_screenPosNorm).xy )*2.0 + -1.0) );
				float clampResult160 = clamp( (0.0 + (dotResult157 - 0.1) * (1.0 - 0.0) / (0.3 - 0.1)) , 0.0 , 1.0 );
				float lightFade158 = clampResult160;
				
				
				finalColor = ( _Colour * ( boxFade140 * voronoi153 * lightFade158 * _Brightness ) );
				return finalColor;
			}
			ENDCG
		}
	}
	
	
	
}
/*ASEBEGIN
Version=18200
2632;653;995;862;5855.032;-1801.898;1.561293;True;False
Node;AmplifyShaderEditor.CommentaryNode;164;-6632.16,831.8668;Inherit;False;683.519;141.8221;;2;77;137;World Position From Depth;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;162;-6629.917,2033.257;Inherit;False;2208.663;751.6851;;16;153;144;145;90;92;91;151;93;149;150;148;152;146;108;167;168;Voronoi;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;77;-6590.514,887.8788;Inherit;False;Reconstruct World Position From Depth;-1;;1;e7094bcbcc80eb140b2a3dbe6a861de8;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;108;-6565.054,2238.926;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;166;-6619.58,2898.376;Inherit;False;2209.239;596.9824;Comment;9;158;160;161;157;156;101;98;100;99;Light Direction;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;-6187.74,882.4653;Inherit;False;pixelWorldPos;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;165;-6622.954,1100.82;Inherit;False;1626.992;827.55;;14;140;124;123;121;130;129;127;122;117;126;128;78;139;170;Box Constraints & Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.CrossProductOpNode;146;-6182.234,2236.11;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,1,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;99;-6538.535,3227.977;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;139;-6588.536,1495.379;Inherit;False;137;pixelWorldPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TransformPositionNode;78;-6285.062,1496.84;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SwizzleNode;100;-6248.995,3226.753;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CrossProductOpNode;148;-6013.716,2096.653;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;126;-5954.821,1546.408;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;168;-5996.402,2236.032;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ObjectScaleNode;128;-5926.336,1659.075;Inherit;False;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;152;-6134.144,2365.372;Inherit;False;137;pixelWorldPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NormalizeNode;167;-5834.402,2097.032;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;98;-5999.114,3204.858;Inherit;True;Global;_CameraNormalsTexture;_CameraNormalsTexture;5;0;Create;True;0;0;False;0;False;-1;None;;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;150;-5617.602,2356.637;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;149;-5618.745,2169.735;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-5804.945,2483.385;Inherit;False;Property;_TimeScale;Time Scale;4;0;Create;True;0;0;False;0;False;1;1;0.1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;-5727.039,1611.533;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;101;-5645.192,3209.703;Inherit;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;156;-5665.434,3001.219;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;129;-6027.085,1811.195;Inherit;False;Property;_FadeDepth;Fade Depth;1;0;Create;True;0;0;False;0;False;2;2;0.1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;122;-5894.779,1352.336;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;117;-5898.188,1200.222;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;130;-5563.267,1611.074;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-5573.951,2596.703;Inherit;False;Property;_CausticsRepeat;Caustics Repeat;2;0;Create;True;0;0;True;0;False;1.5;3;0.1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;92;-5473.638,2488.22;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;151;-5387.785,2333.256;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;157;-5349.194,3000.775;Inherit;False;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;123;-5735.624,1347.264;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;161;-5166.456,2999.189;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0.3;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;90;-5133.872,2470.687;Inherit;False;0;0;1;3;2;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.Compare;121;-5730.804,1193.582;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;170;-5496.646,1732.46;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-5235.763,2663.484;Inherit;False;Property;_Exponent;Exponent;3;0;Create;True;0;0;True;0;False;2;3;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;144;-4847.936,2470.575;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;160;-4926.705,3002.46;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-5387.292,1564.176;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;153;-4648.054,2465.751;Inherit;False;voronoi;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;158;-4700.422,2996.854;Inherit;False;lightFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;140;-5232.531,1559.318;Inherit;False;boxFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;163;-4907.788,1240.527;Inherit;False;898.9177;671.813;;8;76;84;131;73;159;154;141;171;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;159;-4814.724,1689.564;Inherit;False;158;lightFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;154;-4818.839,1588.178;Inherit;False;153;voronoi;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;171;-4855.657,1784.537;Inherit;False;Property;_Brightness;Brightness;6;0;Create;True;0;0;True;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;141;-4821.042,1502.947;Inherit;False;140;boxFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;73;-4863.071,1316.587;Inherit;False;Property;_Colour;Colour;0;0;Create;True;0;0;True;0;False;0.1745283,0.7499093,1,0.4;0.9974865,1,0.9103774,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-4558.053,1413.796;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-4407.495,1323.372;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;76;-4199.625,1323.138;Float;False;True;-1;2;;100;1;NeoFPS/Standard/Water Caustics;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;3;1;False;-1;10;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;1;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;7;False;-1;True;False;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;137;0;77;0
WireConnection;146;0;108;0
WireConnection;78;0;139;0
WireConnection;100;0;99;0
WireConnection;148;0;108;0
WireConnection;148;1;146;0
WireConnection;126;0;78;2
WireConnection;168;0;146;0
WireConnection;167;0;148;0
WireConnection;98;1;100;0
WireConnection;150;0;168;0
WireConnection;150;1;152;0
WireConnection;149;0;167;0
WireConnection;149;1;152;0
WireConnection;127;0;126;0
WireConnection;127;1;128;2
WireConnection;101;0;98;0
WireConnection;122;0;78;3
WireConnection;117;0;78;1
WireConnection;130;0;127;0
WireConnection;130;1;129;0
WireConnection;92;0;93;0
WireConnection;151;0;149;0
WireConnection;151;1;150;0
WireConnection;157;0;156;0
WireConnection;157;1;101;0
WireConnection;123;0;122;0
WireConnection;161;0;157;0
WireConnection;90;0;151;0
WireConnection;90;1;92;0
WireConnection;90;2;91;0
WireConnection;121;0;117;0
WireConnection;170;0;130;0
WireConnection;144;0;90;0
WireConnection;144;1;145;0
WireConnection;160;0;161;0
WireConnection;124;0;121;0
WireConnection;124;1;123;0
WireConnection;124;2;170;0
WireConnection;153;0;144;0
WireConnection;158;0;160;0
WireConnection;140;0;124;0
WireConnection;131;0;141;0
WireConnection;131;1;154;0
WireConnection;131;2;159;0
WireConnection;131;3;171;0
WireConnection;84;0;73;0
WireConnection;84;1;131;0
WireConnection;76;0;84;0
ASEEND*/
//CHKSM=01EA3C77BDBFCD3E97B0C745E69094146B7C43B4