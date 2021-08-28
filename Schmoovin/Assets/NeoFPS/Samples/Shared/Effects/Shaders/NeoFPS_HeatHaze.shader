// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/HeatHaze"
{
	Properties
	{
		_HazeMask("HazeMask", 2D) = "white" {}
		_DistortionMap("Distortion Map", 2D) = "white" {}
		[PerRendererData]_HazeIntensity("Haze Intensity", Range( 0 , 1)) = 1
		_HazeMaxIntensity("Haze Max Intensity", Range( 0 , 1)) = 0.05
		_HazeMaxStrength("Haze Max Strength", Vector) = (0.3,0.3,0,0)
		_VerticalSpeed("Vertical Speed", Range( 0 , 1)) = 0.1
		_HorizontalFrequency("Horizontal Frequency", Range( 0 , 2)) = 0.25
		_HorizontalAmplitude("Horizontal Amplitude", Range( 0 , 1)) = 0.05
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		
		
		GrabPass{ "_GrabScreen0" }

		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
			#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
			#else
			#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
			#endif


			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabScreen0 )
			uniform sampler2D _DistortionMap;
			uniform float _HorizontalAmplitude;
			uniform float _HorizontalFrequency;
			uniform float _VerticalSpeed;
			uniform float2 _HazeMaxStrength;
			uniform float _HazeMaxIntensity;
			uniform float _HazeIntensity;
			uniform sampler2D _HazeMask;
			uniform float4 _HazeMask_ST;
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
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord1 = screenPos;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
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
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( screenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				float mulTime101 = _Time.y * _HorizontalFrequency;
				float mulTime38 = _Time.y * _VerticalSpeed;
				float2 appendResult112 = (float2(( _HorizontalAmplitude * cos( mulTime101 ) ) , -mulTime38));
				float2 uvOffset121 = appendResult112;
				float2 uv091 = i.ase_texcoord2.xy * float2( 1,1 ) + uvOffset121;
				float2 uv_HazeMask = float4(i.ase_texcoord2.xy,0,0).xy * _HazeMask_ST.xy + _HazeMask_ST.zw;
				float mask141 = tex2D( _HazeMask, uv_HazeMask ).r;
				float2 hazeAmount124 = ( _HazeMaxStrength * _HazeMaxIntensity * _HazeIntensity * i.ase_color.a * mask141 );
				float2 hazeOffset127 = (-hazeAmount124 + ((tex2D( _DistortionMap, uv091 )).rg - float2( 0,0 )) * (hazeAmount124 - -hazeAmount124) / (float2( 1,1 ) - float2( 0,0 )));
				float4 screenColor27 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabScreen0,( (ase_grabScreenPosNorm).xy + hazeOffset127 ));
				float4 appendResult113 = (float4((screenColor27).rgb , mask141));
				
				
				finalColor = appendResult113;
				return finalColor;
			}
			ENDCG
		}
	}
	//CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18200
175;446;1409;873;1364.533;-875.6367;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;123;-1606.066,9.616364;Inherit;False;1255.142;344.259;Animate the distortion UVs;10;121;112;99;93;98;43;38;101;42;97;UV Scroll;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-1573.465,158.5806;Float;False;Property;_HorizontalFrequency;Horizontal Frequency;6;0;Create;True;0;0;False;0;False;0.25;0.3;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-1458.97,256.9747;Float;False;Property;_VerticalSpeed;Vertical Speed;5;0;Create;True;0;0;False;0;False;0.1;0.098;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;101;-1278.515,163.2307;Inherit;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-1231.865,70.38054;Float;False;Property;_HorizontalAmplitude;Horizontal Amplitude;7;0;Create;True;0;0;False;0;False;0.05;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;43;-1083.37,163.8741;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;38;-1129.27,262.2749;Inherit;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;144;-1601.532,-389.5562;Inherit;False;649.475;293.788;Comment;2;141;50;Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-912.7637,141.5806;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;50;-1549.772,-310.4084;Inherit;True;Property;_HazeMask;HazeMask;0;0;Create;True;0;0;False;0;False;-1;None;395fe69e8ad4b0245a89a619083952be;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NegateNode;93;-909.771,262.179;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;126;-874.6248,-820.1383;Inherit;False;772.6897;713.6778;The amount of haze based on properties, tint alpha and vertex alpha;7;124;120;143;119;135;73;148;Haze Amount;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;112;-732.4636,239.3797;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;141;-1205.168,-287.1568;Inherit;False;mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-833.9124,-612.2156;Float;False;Property;_HazeMaxIntensity;Haze Max Intensity;3;0;Create;True;0;0;False;0;False;0.05;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;135;-787.3585,-760.4084;Inherit;False;Property;_HazeMaxStrength;Haze Max Strength;4;0;Create;True;0;0;False;0;False;0.3,0.3;0.05,0.025;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;148;-833.23,-505.6638;Inherit;False;Property;_HazeIntensity;Haze Intensity;2;1;[PerRendererData];Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;119;-760.6423,-401.9232;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;-573.5596,234.4702;Inherit;False;uvOffset;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;143;-761.3792,-209.7184;Inherit;False;141;mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;129;-1584.357,1047.125;Inherit;False;1520.96;458.8563;Calculate the UV offset for haze;8;125;127;134;136;71;91;122;145;Haze Offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-1554.726,1234.293;Inherit;False;121;uvOffset;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-500.8163,-589.4403;Inherit;False;5;5;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;-336.1864,-593.8271;Inherit;False;hazeAmount;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;91;-1371.716,1192.826;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;71;-1104.438,1164.623;Inherit;True;Property;_DistortionMap;Distortion Map;1;0;Create;True;0;0;False;0;False;-1;None;0e91f4f669fef4848862675435f2b335;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;125;-1014.968,1364.773;Inherit;False;124;hazeAmount;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NegateNode;145;-763.6456,1239.482;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;136;-784.2491,1164.193;Inherit;False;FLOAT2;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;130;-1596.812,455.4058;Inherit;False;1555.194;493.2852;Comment;9;113;108;27;35;128;60;142;146;147;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;134;-556.3232,1169.4;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GrabScreenPosition;60;-1556.932,524.9414;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;127;-348.9325,1164.968;Inherit;False;hazeOffset;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;128;-1518.871,702.7345;Inherit;False;127;hazeOffset;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;146;-1263.235,525.1034;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;-1052.118,529.7314;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.01,0.01;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenColorNode;27;-857.0089,523.5072;Float;False;Global;_GrabScreen0;Grab Screen 0;0;0;Create;True;0;0;False;0;False;Object;-1;True;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;142;-637.145,771.8102;Inherit;False;141;mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;108;-648.2823,522.8515;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;113;-393.2163,753.8136;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;147;-242.1092,753.8164;Float;False;True;-1;2;ASEMaterialInspector;100;1;NeoFPS/Standard/HeatHaze;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;False;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;101;0;97;0
WireConnection;43;0;101;0
WireConnection;38;0;42;0
WireConnection;99;0;98;0
WireConnection;99;1;43;0
WireConnection;93;0;38;0
WireConnection;112;0;99;0
WireConnection;112;1;93;0
WireConnection;141;0;50;1
WireConnection;121;0;112;0
WireConnection;120;0;135;0
WireConnection;120;1;73;0
WireConnection;120;2;148;0
WireConnection;120;3;119;4
WireConnection;120;4;143;0
WireConnection;124;0;120;0
WireConnection;91;1;122;0
WireConnection;71;1;91;0
WireConnection;145;0;125;0
WireConnection;136;0;71;0
WireConnection;134;0;136;0
WireConnection;134;3;145;0
WireConnection;134;4;125;0
WireConnection;127;0;134;0
WireConnection;146;0;60;0
WireConnection;35;0;146;0
WireConnection;35;1;128;0
WireConnection;27;0;35;0
WireConnection;108;0;27;0
WireConnection;113;0;108;0
WireConnection;113;3;142;0
WireConnection;147;0;113;0
ASEEND*/
//CHKSM=4CDC1FAC12C8807009DC2495A83B6B7B6FDF9F27