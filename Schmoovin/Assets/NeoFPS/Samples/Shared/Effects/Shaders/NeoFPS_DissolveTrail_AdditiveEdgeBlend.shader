// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/DissolveTrail Additive (Edge Blend)"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_StartFade("Start Fade", Range( 0.001 , 1)) = 1
		_EdgeFade("Edge Fade", Range( 0.001 , 1)) = 1
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
				uniform float _EdgeFade;


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
					float2 appendResult83 = (float2(_OffsetU , 0.0));
					float4 tex2DNode5 = tex2D( _MainTex, ( uv0_MainTex + appendResult83 ) );
					float3 tintColor27 = ( (_TintColor).rgb * (i.color).rgb );
					float vertexAlpha25 = i.color.a;
					float2 uvs87 = uv0_MainTex;
					float2 break89 = uvs87;
					float clampResult35 = clamp( ( break89.x / _StartFade ) , 0.0 , 1.0 );
					float temp_output_43_0 = abs( (break89.y*2.0 + -1.0) );
					float clampResult52 = clamp( ( ( 1.0 - ( temp_output_43_0 * temp_output_43_0 ) ) / _EdgeFade ) , 0.0 , 1.0 );
					float edgeblend53 = ( clampResult35 * clampResult35 * clampResult52 );
					float tintAlpha75 = _TintColor.a;
					float textureAlpha64 = tex2DNode5.a;
					float clampResult18 = clamp( ( textureAlpha64 - ( 1.0 - vertexAlpha25 ) ) , 0.0 , 1.0 );
					float clampResult12 = clamp( ( clampResult18 / vertexAlpha25 ) , 0.0 , 1.0 );
					float dissolve67 = clampResult12;
					float alpha80 = ( vertexAlpha25 * edgeblend53 * tintAlpha75 * dissolve67 );
					

					fixed4 col = float4( ( (tex2DNode5).rgb * tintColor27 * alpha80 ) , 0.0 );
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
175;446;1409;873;2369.75;1543.897;1.184536;True;True
Node;AmplifyShaderEditor.CommentaryNode;69;-1949.642,-692.3512;Inherit;False;2291.125;393.5956;Comment;13;1;10;66;9;29;64;5;85;83;87;84;86;33;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;33;-1932.054,-619.9286;Inherit;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;86;-1721.927,-544.9855;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;87;-1388.352,-481.2732;Inherit;False;uvs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1554.822,-407.401;Inherit;False;Property;_OffsetU;OffsetU;2;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;83;-1277.822,-402.401;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;63;-1956.183,-1286.843;Inherit;False;1946.197;444.9382;Comment;13;53;57;35;52;37;46;49;45;36;74;43;89;90;Edge Blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;62;-1960.701,-1826.55;Inherit;False;1078.483;424.0835;Comment;8;27;25;58;59;32;60;28;75;Tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;-1917.944,-1211.915;Inherit;False;87;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;89;-1723.612,-1207.418;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.VertexColorNode;60;-1893.181,-1576.688;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;85;-1094.822,-541.9926;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;5;-945.1801,-618.6232;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;True;0;False;-1;None;e28dc97a9541e3642a48c0e3886688c5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;90;-1487.271,-1042.839;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1199.634,-1489.576;Inherit;False;vertexAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;68;-1943.025,-172.9267;Inherit;False;1958.539;450.6002;Comment;8;67;12;17;18;11;16;65;61;Dissolve;1,1,1,1;0;0
Node;AmplifyShaderEditor.AbsOpNode;43;-1237.857,-1043.54;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-1897.513,48.55313;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;-592.0377,-526.9851;Inherit;False;textureAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1101.007,-1047.358;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-1701.451,-115.396;Inherit;False;64;textureAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-1606.955,-14.28957;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1092.045,-955.4696;Inherit;False;Property;_EdgeFade;Edge Fade;1;0;Create;True;0;0;True;0;False;1;1;0.001;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-1361.264,-36.20603;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;49;-945.0576,-1047.573;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1444.196,-1142.536;Inherit;False;Property;_StartFade;Start Fade;0;0;Create;True;0;0;True;0;False;1;1;0.001;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;18;-1179.65,-36.98291;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-758.6753,-1047.002;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-1138.785,-1210.728;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;52;-614.675,-1047.002;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;35;-972.1716,-1211.096;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-1009.377,28.1354;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;32;-1902.84,-1750.319;Inherit;False;0;0;_TintColor;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;12;-677.5857,28.1321;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-401.876,-1208.203;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;75;-1679.023,-1658.979;Inherit;False;tintAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;78;-1933.807,422.6546;Inherit;False;691.6631;426.3718;Comment;6;80;77;54;76;79;26;Alpha;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;-240.7597,-1213.64;Inherit;False;edgeblend;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;67;-208.911,22.13912;Inherit;False;dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-1883.255,498.5633;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;58;-1473.539,-1583.46;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-1862.117,748.4462;Inherit;False;67;dissolve;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-1872.249,583.1208;Inherit;False;53;edgeblend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;28;-1474.758,-1752.804;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-1863.813,663.4081;Inherit;False;75;tintAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1281.064,-1747.634;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-1651.688,596.2607;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1110.758,-1752.804;Inherit;False;tintColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;-1465.402,590.5649;Inherit;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;9;-357.8639,-619.7349;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-371.0694,-535.6428;Inherit;False;27;tintColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-369.4228,-449.251;Inherit;False;80;alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-91.36385,-614.8348;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;85.44912,-614.6897;Float;False;True;-1;2;ASEMaterialInspector;0;7;NeoFPS/Standard/DissolveTrail Additive (Edge Blend);0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;4;1;False;-1;1;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;86;2;33;0
WireConnection;87;0;86;0
WireConnection;83;0;84;0
WireConnection;89;0;88;0
WireConnection;85;0;86;0
WireConnection;85;1;83;0
WireConnection;5;0;33;0
WireConnection;5;1;85;0
WireConnection;90;0;89;1
WireConnection;25;0;60;4
WireConnection;43;0;90;0
WireConnection;64;0;5;4
WireConnection;74;0;43;0
WireConnection;74;1;43;0
WireConnection;16;0;61;0
WireConnection;11;0;65;0
WireConnection;11;1;16;0
WireConnection;49;0;74;0
WireConnection;18;0;11;0
WireConnection;46;0;49;0
WireConnection;46;1;45;0
WireConnection;37;0;89;0
WireConnection;37;1;36;0
WireConnection;52;0;46;0
WireConnection;35;0;37;0
WireConnection;17;0;18;0
WireConnection;17;1;61;0
WireConnection;12;0;17;0
WireConnection;57;0;35;0
WireConnection;57;1;35;0
WireConnection;57;2;52;0
WireConnection;75;0;32;4
WireConnection;53;0;57;0
WireConnection;67;0;12;0
WireConnection;58;0;60;0
WireConnection;28;0;32;0
WireConnection;59;0;28;0
WireConnection;59;1;58;0
WireConnection;77;0;26;0
WireConnection;77;1;54;0
WireConnection;77;2;76;0
WireConnection;77;3;79;0
WireConnection;27;0;59;0
WireConnection;80;0;77;0
WireConnection;9;0;5;0
WireConnection;10;0;9;0
WireConnection;10;1;29;0
WireConnection;10;2;66;0
WireConnection;1;0;10;0
ASEEND*/
//CHKSM=D205EA2A10A2FC6FA8E15DD15C74A6D59574F089