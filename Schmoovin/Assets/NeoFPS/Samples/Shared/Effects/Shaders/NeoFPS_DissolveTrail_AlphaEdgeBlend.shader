// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/DissolveTrail Alpha (Edge Blend)"
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
					float2 appendResult81 = (float2(_OffsetU , 0.0));
					float4 tex2DNode5 = tex2D( _MainTex, ( uv0_MainTex + appendResult81 ) );
					float3 tintColor27 = ( (_TintColor).rgb * (i.color).rgb );
					float2 uvs78 = uv0_MainTex;
					float2 break83 = uvs78;
					float clampResult35 = clamp( ( break83.x / _StartFade ) , 0.0 , 1.0 );
					float temp_output_43_0 = abs( (break83.y*2.0 + -1.0) );
					float clampResult52 = clamp( ( ( 1.0 - ( temp_output_43_0 * temp_output_43_0 ) ) / _EdgeFade ) , 0.0 , 1.0 );
					float edgeblend53 = ( clampResult35 * clampResult35 * clampResult52 );
					float tintAlpha75 = _TintColor.a;
					float vertexAlpha25 = i.color.a;
					float textureAlpha63 = tex2DNode5.a;
					float clampResult18 = clamp( ( textureAlpha63 - ( 1.0 - vertexAlpha25 ) ) , 0.0 , 1.0 );
					float clampResult12 = clamp( ( clampResult18 / vertexAlpha25 ) , 0.0 , 1.0 );
					float dissolve66 = clampResult12;
					float alpha73 = ( edgeblend53 * tintAlpha75 * vertexAlpha25 * dissolve66 );
					float4 appendResult14 = (float4(( (tex2DNode5).rgb * tintColor27 ) , alpha73));
					

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
175;446;1409;873;1899.322;1194.18;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;69;-1813.372,-505.0933;Inherit;False;2676.287;523.2009;Comment;13;1;14;67;10;9;29;63;5;80;81;82;78;34;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;33;-1655.33,-404.8346;Inherit;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-1371.384,-320.8115;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;82;-1114.45,-163.1573;Inherit;False;Property;_OffsetU;OffsetU;2;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;78;-1108.394,-245.2609;Inherit;False;uvs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;65;-1813.338,-1126.564;Inherit;False;2061.602;466.6054;Comment;14;53;57;35;52;37;46;45;36;49;71;43;83;79;84;Edge Blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-1756.621,-1036.188;Inherit;False;78;uvs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;62;-1806.55,137.0078;Inherit;False;999.6347;463.5333;Comment;8;27;28;25;58;59;32;60;75;Tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;81;-836.9384,-158.6619;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;83;-1498.577,-1030.262;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;80;-861.9803,-319.7855;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;60;-1679.475,410.5284;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-663.2046,-413.3282;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;True;0;False;-1;None;e28dc97a9541e3642a48c0e3886688c5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;84;-1208.322,-868.1804;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;68;-1821.302,-1724.279;Inherit;False;1663.848;444.1673;Comment;8;66;12;17;18;11;64;16;61;Dissolve;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1065.306,499.609;Inherit;False;vertexAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;-334.8504,-320.4243;Inherit;False;textureAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-1777.116,-1507.842;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;43;-983.105,-868.5677;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-837.8392,-869.3365;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-1616.135,-1662.934;Inherit;False;63;textureAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-1519.091,-1571.718;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;49;-694.3045,-868.6015;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-1296.349,-1593.635;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1185.444,-963.5645;Inherit;False;Property;_StartFade;Start Fade;0;0;Create;True;0;0;True;0;False;1;1;0.001;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-833.2932,-757.4983;Inherit;False;Property;_EdgeFade;Edge Fade;1;0;Create;True;0;0;True;0;False;1;1;0.001;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-499.9227,-868.0297;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-880.0332,-1031.755;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;18;-1114.734,-1594.412;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-906.761,-1529.293;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;52;-355.9224,-868.0297;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;35;-713.4191,-1032.124;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;32;-1759.848,206.0371;Inherit;False;0;0;_TintColor;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-143.1236,-1029.231;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;12;-612.6694,-1529.297;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;28;-1389.722,204.8533;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;66;-396.7238,-1533.346;Inherit;False;dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;58;-1391.297,405.1969;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;72;-664.7109,138.6029;Inherit;False;927.1226;565.682;Comment;6;73;74;77;54;76;26;Alpha;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;17.99262,-1034.667;Inherit;False;edgeblend;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;75;-1437.45,297.2332;Inherit;False;tintAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-568.7816,362.5939;Inherit;False;75;tintAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-565.2325,587.2867;Inherit;False;66;dissolve;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-583.0717,245.5695;Inherit;False;53;edgeblend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1179.858,208.7463;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-590.5108,481.038;Inherit;False;25;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1037.293,204.2324;Inherit;False;tintColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-275.0456,386.6406;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;9;-92.5247,-412.475;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-104.7301,-317.218;Inherit;False;27;tintColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-10.91061,381.416;Inherit;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;141.2944,-310.0771;Inherit;False;73;alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;156.6591,-408.5749;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;14;382.2679,-408.8965;Inherit;False;COLOR;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;587.0527,-408.8133;Float;False;True;-1;2;ASEMaterialInspector;0;7;NeoFPS/Standard/DissolveTrail Alpha (Edge Blend);0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;34;2;33;0
WireConnection;78;0;34;0
WireConnection;81;0;82;0
WireConnection;83;0;79;0
WireConnection;80;0;34;0
WireConnection;80;1;81;0
WireConnection;5;0;33;0
WireConnection;5;1;80;0
WireConnection;84;0;83;1
WireConnection;25;0;60;4
WireConnection;63;0;5;4
WireConnection;43;0;84;0
WireConnection;71;0;43;0
WireConnection;71;1;43;0
WireConnection;16;0;61;0
WireConnection;49;0;71;0
WireConnection;11;0;64;0
WireConnection;11;1;16;0
WireConnection;46;0;49;0
WireConnection;46;1;45;0
WireConnection;37;0;83;0
WireConnection;37;1;36;0
WireConnection;18;0;11;0
WireConnection;17;0;18;0
WireConnection;17;1;61;0
WireConnection;52;0;46;0
WireConnection;35;0;37;0
WireConnection;57;0;35;0
WireConnection;57;1;35;0
WireConnection;57;2;52;0
WireConnection;12;0;17;0
WireConnection;28;0;32;0
WireConnection;66;0;12;0
WireConnection;58;0;60;0
WireConnection;53;0;57;0
WireConnection;75;0;32;4
WireConnection;59;0;28;0
WireConnection;59;1;58;0
WireConnection;27;0;59;0
WireConnection;74;0;54;0
WireConnection;74;1;76;0
WireConnection;74;2;26;0
WireConnection;74;3;77;0
WireConnection;9;0;5;0
WireConnection;73;0;74;0
WireConnection;10;0;9;0
WireConnection;10;1;29;0
WireConnection;14;0;10;0
WireConnection;14;3;67;0
WireConnection;1;0;14;0
ASEEND*/
//CHKSM=F7AA15CDEEFE8D7309F8CB1DDE411EA26389ABF3