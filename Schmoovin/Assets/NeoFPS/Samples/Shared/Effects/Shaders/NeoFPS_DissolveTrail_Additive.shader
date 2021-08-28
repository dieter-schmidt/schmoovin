// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/DissolveTrail Additive"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_StartFade("Start Fade", Range( 0.001 , 1)) = 1
		[PerRendererData]_OffsetU("OffsetU", Range( 0 , 1)) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend One One, SrcAlpha OneMinusSrcAlpha
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
					float2 appendResult68 = (float2(_OffsetU , 0.0));
					float4 tex2DNode5 = tex2D( _MainTex, ( uv0_MainTex + appendResult68 ) );
					float3 tintColor27 = (( (_TintColor).rgb * (i.color).rgb )).xyz;
					float vertexAlpha25 = i.color.a;
					float tintAlpha53 = _TintColor.a;
					float textureAlpha44 = tex2DNode5.a;
					float dissolve46 = saturate( ( saturate( ( textureAlpha44 - ( 1.0 - vertexAlpha25 ) ) ) / vertexAlpha25 ) );
					float u63 = uv0_MainTex.x;
					float temp_output_43_0 = saturate( ( u63 / _StartFade ) );
					float startFade57 = ( temp_output_43_0 * temp_output_43_0 );
					float alpha61 = ( vertexAlpha25 * tintAlpha53 * dissolve46 * startFade57 );
					

					fixed4 col = float4( ( (tex2DNode5).rgb * tintColor27 * alpha61 ) , 0.0 );
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
119;436;1409;880;1772.769;954.4078;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;48;-1622.965,-770.0972;Inherit;False;2117.799;424.3467;Comment;13;1;10;60;29;9;63;44;5;65;68;70;33;66;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;33;-1592.932,-691.4738;Inherit;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;66;-1313.12,-452.3314;Inherit;False;Property;_OffsetU;OffsetU;1;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;68;-1037.12,-447.3314;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;70;-1347.489,-594.6494;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;50;-1621.399,-1289.874;Inherit;False;1025.717;435.4285;Tint colour from property & vertex colour;9;27;28;25;38;40;32;39;52;53;Tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;39;-1585.403,-1044.453;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-847.0773,-597.3411;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;5;-700.0008,-690.8567;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;True;0;False;-1;None;e28dc97a9541e3642a48c0e3886688c5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;51;-1608.932,-211.1073;Inherit;False;1730.826;473.4266;Dissolve the texture and blend out the start;8;46;42;17;41;11;45;16;49;Dissolve;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1200.364,-957.1357;Inherit;False;vertexAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-390.474,-598.112;Inherit;False;textureAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-1561.475,11.62657;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-1357.613,-136.5615;Inherit;False;44;textureAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-1310.579,-49.72057;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;-1099.501,-531.4724;Inherit;False;u;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;56;-1612.227,414.4096;Inherit;False;1393.568;263.2184;Comment;6;57;58;43;37;36;71;Start Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1276.964,575.8951;Inherit;False;Property;_StartFade;Start Fade;0;0;Create;True;0;0;True;0;False;1;1;0.001;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;-1573.811,504.8302;Inherit;False;63;u;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-1080.889,-71.63705;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-971.5527,507.7042;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;41;-910.8561,-71.4658;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;43;-803.9322,507.3069;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-691.3017,-7.295567;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-628.3971,507.6193;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;32;-1592.807,-1233.955;Inherit;False;0;0;_TintColor;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;42;-389.7667,-8.220707;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;38;-1358.307,-1234.456;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;52;-1360.489,-1049.918;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;-1401.164,-1141.535;Inherit;False;tintAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;59;-1607.314,806.7552;Inherit;False;814.0699;498.8478;Comment;6;61;15;47;62;54;26;Alpha;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-131.7994,-13.42565;Inherit;False;dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-447.3772,502.8148;Inherit;False;startFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-1503.685,999.0284;Inherit;False;53;tintAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-1503.811,1198.282;Inherit;False;57;startFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-1141.649,-1230.131;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-1525.147,904.6824;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-1498.931,1098.138;Inherit;False;46;dissolve;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-1241.221,1022.845;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;28;-971.8502,-1234.78;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-1035.837,1017.777;Inherit;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-808.3483,-1234.78;Inherit;False;tintColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-146.5204,-611.6626;Inherit;False;27;tintColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;9;-134.315,-689.7549;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-143.9983,-530.1125;Inherit;False;61;alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;82.52105,-683.6382;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;248.0718,-683.7857;Float;False;True;-1;2;ASEMaterialInspector;0;7;NeoFPS/Standard/DissolveTrail Additive;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;4;1;False;-1;1;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;68;0;66;0
WireConnection;70;2;33;0
WireConnection;65;0;70;0
WireConnection;65;1;68;0
WireConnection;5;0;33;0
WireConnection;5;1;65;0
WireConnection;25;0;39;4
WireConnection;44;0;5;4
WireConnection;16;0;49;0
WireConnection;63;0;70;1
WireConnection;11;0;45;0
WireConnection;11;1;16;0
WireConnection;37;0;71;0
WireConnection;37;1;36;0
WireConnection;41;0;11;0
WireConnection;43;0;37;0
WireConnection;17;0;41;0
WireConnection;17;1;49;0
WireConnection;58;0;43;0
WireConnection;58;1;43;0
WireConnection;42;0;17;0
WireConnection;38;0;32;0
WireConnection;52;0;39;0
WireConnection;53;0;32;4
WireConnection;46;0;42;0
WireConnection;57;0;58;0
WireConnection;40;0;38;0
WireConnection;40;1;52;0
WireConnection;15;0;26;0
WireConnection;15;1;54;0
WireConnection;15;2;47;0
WireConnection;15;3;62;0
WireConnection;28;0;40;0
WireConnection;61;0;15;0
WireConnection;27;0;28;0
WireConnection;9;0;5;0
WireConnection;10;0;9;0
WireConnection;10;1;29;0
WireConnection;10;2;60;0
WireConnection;1;0;10;0
ASEEND*/
//CHKSM=CB250E1F2A9EEC667AE4311E37036A1B52199FAB