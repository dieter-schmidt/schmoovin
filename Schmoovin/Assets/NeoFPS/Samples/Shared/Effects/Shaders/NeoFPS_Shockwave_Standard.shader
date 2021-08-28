// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/ShockWave"
{
	Properties
	{
		[PerRendererData]_Distortion("Distortion", Range( 0 , 1)) = 0
		_EdgeNoise("Edge Noise", 2D) = "white" {}
		_EdgeNoiseAmount("Edge Noise Amount", Range( 0 , 1)) = 1
		_EdgeNoiseTightness("Edge Noise Tightness", Range( 0 , 1)) = 0.5
		_DistortionEffect("Distortion Effect", Range( -1 , 1)) = 0.5
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
		
		
		GrabPass{ }

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
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 ase_normal : NORMAL;
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
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
			uniform float _Distortion;
			uniform float _DistortionEffect;
			uniform sampler2D _EdgeNoise;
			uniform float4 _EdgeNoise_ST;
			uniform float _EdgeNoiseAmount;
			uniform float _EdgeNoiseTightness;
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
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.xyz = ase_worldNormal;
				
				o.ase_color = v.color;
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.zw = 0;
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
				float3 ase_worldNormal = i.ase_texcoord2.xyz;
				float3 worldNormal83 = ase_worldNormal;
				float3 TangentDistortion93 = ( mul( unity_WorldToCamera, float4( -worldNormal83 , 0.0 ) ).xyz * ( _Distortion * _DistortionEffect * 0.075 * i.ase_color.a ) );
				float2 uv_EdgeNoise = i.ase_texcoord3.xy * _EdgeNoise_ST.xy + _EdgeNoise_ST.zw;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult51 = dot( ase_worldViewDir , worldNormal83 );
				float edgeNoiseMask89 = ( _EdgeNoiseAmount * saturate( ( 1.0 - ( (0.75 + (_EdgeNoiseTightness - 0.0) * (2.0 - 0.75) / (1.0 - 0.0)) * dotResult51 ) ) ) );
				float4 lerpResult72 = lerp( float4( float3(1,1,1) , 0.0 ) , tex2D( _EdgeNoise, uv_EdgeNoise ) , edgeNoiseMask89);
				float4 noiseDistortion96 = lerpResult72;
				float4 screenColor8 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( float3( (ase_grabScreenPosNorm).xy ,  0.0 ) + (( float4( TangentDistortion93 , 0.0 ) * noiseDistortion96 )).rga ).xy);
				
				
				finalColor = screenColor8;
				return finalColor;
			}
			ENDCG
		}
	}
	//CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18200
175;446;1409;873;3400.51;229.1247;1.43243;True;True
Node;AmplifyShaderEditor.CommentaryNode;88;-2873.166,-812.512;Inherit;False;481.0044;202.5781;Comment;2;83;48;World Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;48;-2840.754,-754.5335;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;91;-2874.487,-526.8915;Inherit;False;1507.314;473.7827;Noise amount as the surface slopes from the camera;11;89;75;74;79;66;61;51;68;87;64;92;Edge Noise Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-2610.681,-759.5274;Inherit;False;worldNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-2753.085,-432.6639;Inherit;False;Property;_EdgeNoiseTightness;Edge Noise Tightness;3;0;Create;True;0;0;True;0;False;0.5;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;64;-2835.417,-223.5162;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;87;-2643.184,-152.9323;Inherit;False;83;worldNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;92;-2457.867,-427.3553;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.75;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;51;-2395.354,-218.6022;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-2230.618,-299.0486;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;66;-2089.208,-298.6772;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-2083.086,-408.4452;Inherit;False;Property;_EdgeNoiseAmount;Edge Noise Amount;2;0;Create;True;0;0;True;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;79;-1928.847,-298.1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;95;-2866.941,46.4211;Inherit;False;1056.841;708.5602;The distortion offsets based purely on surface tangent;11;78;93;32;50;77;76;101;49;85;31;86;Tangent Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-1765.577,-403.9972;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-2833.733,208.3052;Inherit;False;83;worldNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-2819.796,396.2838;Inherit;False;Property;_DistortionEffect;Distortion Effect;4;0;Create;True;0;0;False;0;False;0.5;0.4;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToCameraMatrix;49;-2679.61,94.89052;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.CommentaryNode;98;-2331.083,-1144.051;Inherit;False;829.7225;526.2213;Comment;5;96;72;29;71;90;Noise Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;78;-2720.334,570.1047;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;101;-2743.29,485.6235;Inherit;False;Constant;_EffectMultiplier;Effect Multiplier;5;0;Create;True;0;0;False;0;False;0.075;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2817.868,304.4783;Float;False;Property;_Distortion;Distortion;0;1;[PerRendererData];Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;89;-1601.434,-408.756;Inherit;False;edgeNoiseMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;85;-2595.463,213.4026;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-2414.081,190.3464;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;29;-2290.657,-918.1948;Inherit;True;Property;_EdgeNoise;Edge Noise;1;0;Create;True;0;0;False;0;False;-1;None;50ca3e8355f126a46923ef1732b7529e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;71;-2157.366,-1070.941;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-2429.565,309.1612;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-2222.926,-721.6687;Inherit;False;89;edgeNoiseMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;72;-1908.766,-762.3398;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-2235.392,189.5644;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;99;-1656.173,153.3707;Inherit;False;1420.578;495.8277;Comment;9;8;30;36;39;73;40;94;97;102;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;-2060.516,185.0766;Inherit;False;TangentDistortion;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;96;-1735.014,-767.9794;Inherit;False;noiseDistortion;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;94;-1636.618,466.2102;Inherit;False;93;TangentDistortion;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;-1618.124,551.9926;Inherit;False;96;noiseDistortion;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GrabScreenPosition;40;-1461.912,261.014;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1347.711,470.4042;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;36;-1175.252,465.9467;Inherit;False;True;True;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;39;-1200.401,260.5023;Inherit;True;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-899.1528,265.2351;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenColorNode;8;-706.264,261.6171;Float;False;Global;_ScreenGrab0;Screen Grab 0;-1;0;Create;True;0;0;False;0;False;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;102;-474.3267,267.4556;Float;False;True;-1;2;ASEMaterialInspector;100;1;NeoFPS/Standard/ShockWave;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;False;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;83;0;48;0
WireConnection;92;0;68;0
WireConnection;51;0;64;0
WireConnection;51;1;87;0
WireConnection;61;0;92;0
WireConnection;61;1;51;0
WireConnection;66;0;61;0
WireConnection;79;0;66;0
WireConnection;75;0;74;0
WireConnection;75;1;79;0
WireConnection;89;0;75;0
WireConnection;85;0;86;0
WireConnection;50;0;49;0
WireConnection;50;1;85;0
WireConnection;77;0;31;0
WireConnection;77;1;76;0
WireConnection;77;2;101;0
WireConnection;77;3;78;4
WireConnection;72;0;71;0
WireConnection;72;1;29;0
WireConnection;72;2;90;0
WireConnection;32;0;50;0
WireConnection;32;1;77;0
WireConnection;93;0;32;0
WireConnection;96;0;72;0
WireConnection;73;0;94;0
WireConnection;73;1;97;0
WireConnection;36;0;73;0
WireConnection;39;0;40;0
WireConnection;30;0;39;0
WireConnection;30;1;36;0
WireConnection;8;0;30;0
WireConnection;102;0;8;0
ASEEND*/
//CHKSM=32AF5F3C6B6E72D37E72BFDE1FFC30810C185EB6