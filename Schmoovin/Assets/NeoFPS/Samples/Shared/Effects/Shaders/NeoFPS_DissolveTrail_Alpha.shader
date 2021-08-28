// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/DissolveTrail Alpha"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_StartFade("Start Fade", Range( 0.001 , 1)) = 1
		[PerRendererData]_OffsetU("OffsetU", Float) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ColorMask RGBA
			Cull Back
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float _OffsetU;
				uniform float _StartFade;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 uv0_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float2 appendResult61 = (float2(_OffsetU , 0.0));
					float4 tex2DNode5 = tex2D( _MainTex, ( uv0_MainTex + appendResult61 ) );
					float3 tintColor27 = ( (_TintColor).rgb * (i.color).rgb );
					float u65 = uv0_MainTex.x;
					float clampResult35 = clamp( ( u65 / _StartFade ) , 0.0 , 1.0 );
					float startFade43 = ( clampResult35 * clampResult35 );
					float textureAlpha56 = tex2DNode5.a;
					float vertexAlpha25 = i.color.a;
					float clampResult18 = clamp( ( ( startFade43 * textureAlpha56 ) - ( 1.0 - vertexAlpha25 ) ) , 0.0 , 1.0 );
					float clampResult12 = clamp( ( clampResult18 / vertexAlpha25 ) , 0.0 , 1.0 );
					float dissolve45 = clampResult12;
					float tintAlpha49 = _TintColor.a;
					float alpha53 = ( dissolve45 * tintAlpha49 * vertexAlpha25 );
					float4 appendResult14 = (float4(( (tex2DNode5).rgb * tintColor27 ) , alpha53));
					

					fixed4 col = appendResult14;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	//CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18200
144;498;1409;880;2171.16;1168.096;1.569779;True;True
Node;AmplifyShaderEditor.CommentaryNode;48;-1898.249,-570.0938;Inherit;False;2804.107;563.1154;Comment;14;1;14;10;54;29;9;56;5;62;61;63;65;34;33;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;33;-1804.094,-459.485;Inherit;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-1543.167,-393.8284;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;55;-1894.467,-1727.259;Inherit;False;1171.302;305.8405;Comment;6;64;43;21;35;37;36;startFade;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;65;-1271.585,-143.9202;Inherit;False;u;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1841.424,-1531.098;Inherit;False;Property;_StartFade;Start Fade;0;0;Create;True;0;0;True;0;False;1;0.782;0.001;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-1747.462,-1638.457;Inherit;False;65;u;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-1255.031,-257.312;Inherit;False;Property;_OffsetU;OffsetU;1;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-1536.013,-1599.289;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;61;-1074.031,-252.312;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;62;-885.7407,-387.5568;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;42;-1883.507,218.474;Inherit;False;1020.271;443.7669;Comment;8;49;27;39;28;38;32;25;40;Tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;35;-1369.399,-1599.658;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1157.992,-1623.778;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-642.7983,-469.7117;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;True;0;False;-1;None;e28dc97a9541e3642a48c0e3886688c5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;40;-1832.754,475.0858;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;47;-1896.655,-1253.218;Inherit;False;1955.629;461.2496;Comment;10;45;12;17;18;11;16;57;44;41;58;Dissolve;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-267.3544,-376.6181;Inherit;False;textureAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;43;-964.8666,-1628.611;Inherit;False;startFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1480.337,560.7296;Inherit;False;vertexAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-1848.537,-1101.129;Inherit;False;56;textureAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;44;-1826.243,-1181.991;Inherit;False;43;startFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-1842.929,-1018.058;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-1567.76,-1176.798;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-1582.172,-1077.024;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-1345.482,-1099.941;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;18;-1163.868,-1100.718;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-955.8948,-1035.6;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;12;-661.8031,-1035.603;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;32;-1833.789,287.674;Inherit;False;0;0;_TintColor;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;28;-1479.111,287.0171;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;38;-1480.243,469.3842;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;52;-1890.261,871.1145;Inherit;False;711.052;375.9418;Comment;5;53;50;51;26;46;Alpha;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-1527.406,378.8569;Inherit;False;tintAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;-181.5787,-1041.13;Inherit;False;dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-1821.297,1050.318;Inherit;False;49;tintAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-1841.817,1152.238;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-1260.169,292.4273;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-1819.198,949.958;Inherit;False;45;dissolve;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-1584.297,1032.319;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1086.011,286.0171;Inherit;False;tintColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;-1401.246,1026.558;Inherit;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;9;21.34188,-470.5959;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;9.136483,-392.5037;Inherit;False;27;tintColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;195.6895,-279.109;Inherit;False;53;alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;269.6329,-466.6957;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;14;443.7695,-466.4334;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;634.6365,-466.3502;Float;False;True;-1;2;ASEMaterialInspector;0;7;NeoFPS/Standard/DissolveTrail Alpha;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;34;2;33;0
WireConnection;65;0;34;1
WireConnection;37;0;64;0
WireConnection;37;1;36;0
WireConnection;61;0;63;0
WireConnection;62;0;34;0
WireConnection;62;1;61;0
WireConnection;35;0;37;0
WireConnection;21;0;35;0
WireConnection;21;1;35;0
WireConnection;5;0;33;0
WireConnection;5;1;62;0
WireConnection;56;0;5;4
WireConnection;43;0;21;0
WireConnection;25;0;40;4
WireConnection;57;0;44;0
WireConnection;57;1;58;0
WireConnection;16;0;41;0
WireConnection;11;0;57;0
WireConnection;11;1;16;0
WireConnection;18;0;11;0
WireConnection;17;0;18;0
WireConnection;17;1;41;0
WireConnection;12;0;17;0
WireConnection;28;0;32;0
WireConnection;38;0;40;0
WireConnection;49;0;32;4
WireConnection;45;0;12;0
WireConnection;39;0;28;0
WireConnection;39;1;38;0
WireConnection;50;0;46;0
WireConnection;50;1;51;0
WireConnection;50;2;26;0
WireConnection;27;0;39;0
WireConnection;53;0;50;0
WireConnection;9;0;5;0
WireConnection;10;0;9;0
WireConnection;10;1;29;0
WireConnection;14;0;10;0
WireConnection;14;3;54;0
WireConnection;1;0;14;0
ASEEND*/
//CHKSM=85FA713CA48D8FE44D837A23C8958443BA55BDAC