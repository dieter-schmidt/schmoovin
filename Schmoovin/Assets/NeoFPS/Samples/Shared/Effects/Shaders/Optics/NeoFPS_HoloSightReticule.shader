// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/HoloSightReticule"
{
	Properties
	{
		_MaxIntensity("Max Intensity", Range( 0.1 , 2)) = 1.2
		_ReticuleAlpha("ReticuleAlpha", 2D) = "black" {}
		[ASEEnd]_StencilBuffer("StencilBuffer", Int) = 5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Overlay" "Queue"="Transparent+2" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend One One
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest Always
		Stencil
		{
			Ref [_StencilBuffer]
			ReadMask [_StencilBuffer]
			Comp Equal
		}
		
		
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
			#define ASE_NEEDS_FRAG_COLOR


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
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform int _StencilBuffer;
			uniform sampler2D _ReticuleAlpha;
			uniform float4 _ReticuleAlpha_ST;
			uniform float _MaxIntensity;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_color = v.color;
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
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
				float2 uv_ReticuleAlpha = i.ase_texcoord1.xy * _ReticuleAlpha_ST.xy + _ReticuleAlpha_ST.zw;
				float reticuleAlpha46 = tex2D( _ReticuleAlpha, uv_ReticuleAlpha ).a;
				float vertexAlpha26 = i.ase_color.a;
				float temp_output_41_0 = ( reticuleAlpha46 * vertexAlpha26 * _MaxIntensity );
				float clampResult44 = clamp( ( ( temp_output_41_0 * temp_output_41_0 ) - 1.0 ) , 0.0 , 1.0 );
				float rgbOffset45 = clampResult44;
				float lerpResult56 = lerp( 4.0 , 1.0 , vertexAlpha26);
				float brightnessMultiplier58 = pow( reticuleAlpha46 , lerpResult56 );
				
				
				finalColor = float4( ( ( (i.ase_color).rgb + rgbOffset45 ) * brightnessMultiplier58 ) , 0.0 );
				return finalColor;
			}
			ENDCG
		}
	}
	
	
	
}
/*ASEBEGIN
Version=18909
2636;731;995;862;2081.011;1884.717;2.993661;True;False
Node;AmplifyShaderEditor.CommentaryNode;49;-1181.812,-1745.039;Inherit;False;768.145;391.6212;Comment;2;46;2;Reticule Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;51;-1189.096,-27.09824;Inherit;False;1655.683;547.5409;Comment;8;0;31;48;29;47;28;26;25;Composite;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;25;-1074.636,72.19389;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-1104.818,-1635.801;Inherit;True;Property;_ReticuleAlpha;ReticuleAlpha;1;0;Create;True;0;0;0;True;0;False;-1;None;dbaf5935e5109ec4f838dffd462f1872;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;50;-1196.083,-644.2266;Inherit;False;1661.466;432.2268;Comment;8;45;44;43;42;41;38;40;39;RGB Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-687.2101,-1543.795;Inherit;False;reticuleAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-788.6031,158.0157;Inherit;False;vertexAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1131.428,-372.8408;Inherit;False;Property;_MaxIntensity;Max Intensity;0;0;Create;True;0;0;0;False;0;False;1.2;1.2;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-1061.6,-467.5718;Inherit;False;26;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-1067.22,-554.988;Inherit;False;46;reticuleAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-673.6154,-411.9377;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-443.6769,-412.2294;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;53;-1193.865,-1240.165;Inherit;False;1086.569;465.2689;Comment;5;58;57;56;55;54;Brightness;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;43;-241.5885,-412.8909;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-1143.431,-902.9503;Inherit;False;26;vertexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;55;-965.6846,-1175.528;Inherit;True;46;reticuleAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;44;-7.547145,-412.8911;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;56;-905.3671,-943.4753;Inherit;False;3;0;FLOAT;4;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;197.9744,-419.3874;Inherit;False;rgbOffset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;57;-674.1727,-1028.403;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;28;-784.0681,66.43224;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-384.5348,-1032.825;Inherit;False;brightnessMultiplier;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-761.7683,264.9721;Inherit;False;45;rgbOffset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-408.3094,72.04625;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-467.6327,264.0251;Inherit;False;58;brightnessMultiplier;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-71.80289,70.41093;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.IntNode;24;240.8366,209.9673;Inherit;False;Property;_StencilBuffer;StencilBuffer;2;0;Create;True;0;0;0;True;0;False;5;5;False;0;1;INT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;217.2953,69.94852;Float;False;True;-1;2;;100;1;NeoFPS/Standard/HoloSightReticule;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;4;1;False;-1;1;False;-1;0;5;False;-1;10;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;True;True;True;5;True;24;255;True;24;255;False;-1;5;False;-1;0;False;-1;0;False;-1;0;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;7;False;-1;True;False;0;False;-1;0;False;-1;True;2;RenderType=Overlay=RenderType;Queue=Transparent=Queue=2;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;46;0;2;4
WireConnection;26;0;25;4
WireConnection;41;0;40;0
WireConnection;41;1;39;0
WireConnection;41;2;38;0
WireConnection;42;0;41;0
WireConnection;42;1;41;0
WireConnection;43;0;42;0
WireConnection;44;0;43;0
WireConnection;56;2;54;0
WireConnection;45;0;44;0
WireConnection;57;0;55;0
WireConnection;57;1;56;0
WireConnection;28;0;25;0
WireConnection;58;0;57;0
WireConnection;29;0;28;0
WireConnection;29;1;47;0
WireConnection;31;0;29;0
WireConnection;31;1;48;0
WireConnection;0;0;31;0
ASEEND*/
//CHKSM=4C54F8186D9B22C7756B2E3AD553C2EA87A3505B