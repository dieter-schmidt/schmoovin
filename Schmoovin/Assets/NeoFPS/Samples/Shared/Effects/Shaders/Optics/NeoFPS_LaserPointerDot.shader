// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/LaserPointerDot"
{
	Properties
	{
		[ASEEnd]_MaxIntensity("Max Intensity", Range( 0.1 , 2)) = 1.2

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Overlay" "Queue"="Transparent" }
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
				float2 texCoord24 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float clampResult27 = clamp( ( distance( texCoord24 , float2( 0.5,0.5 ) ) * 2.0 ) , 0.0 , 1.0 );
				float falloff63 = ( 1.0 - clampResult27 );
				float a42 = i.ase_color.a;
				float temp_output_36_0 = ( falloff63 * a42 * _MaxIntensity );
				float clampResult46 = clamp( ( ( temp_output_36_0 * temp_output_36_0 ) - 1.0 ) , 0.0 , 1.0 );
				float rgbOffset66 = clampResult46;
				float lerpResult75 = lerp( 8.0 , 2.0 , a42);
				float brightnessMultiplier77 = pow( falloff63 , lerpResult75 );
				
				
				finalColor = float4( ( ( (i.ase_color).rgb + rgbOffset66 ) * brightnessMultiplier77 ) , 0.0 );
				return finalColor;
			}
			ENDCG
		}
	}
	
	
	
}
/*ASEBEGIN
Version=18909
0;3;1920;1015;3954.133;1550.007;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;81;-3559.708,-184.5689;Inherit;False;1406.37;249.7274;Comment;6;63;29;27;28;26;24;Falloff;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;24;-3474.986,-91.85834;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;26;-3178.245,-91.85834;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-2995.148,-91.85843;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;79;-3562.047,496.1636;Inherit;False;1816.559;442.4764;Comment;8;0;50;69;48;67;38;42;61;Composite;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;27;-2826.257,-91.85834;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;61;-3460.425,570.4446;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;29;-2628.951,-91.85834;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;82;-3557.565,-666.6401;Inherit;False;1686.572;372.5715;Comment;8;66;46;45;51;36;43;33;65;RGB Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;42;-3197.391,656.2663;Inherit;False;a;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;-2411.831,-97.58871;Inherit;False;falloff;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-3511.479,-408.9319;Inherit;False;Property;_MaxIntensity;Max Intensity;0;0;Create;True;0;0;0;False;0;False;1.2;1.4;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;43;-3418.377,-498.7996;Inherit;False;42;a;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-3419.579,-583.4387;Inherit;False;63;falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-2981.439,-449.5803;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-2765.802,-462.8725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3.27;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;72;-3552.989,-1355.542;Inherit;False;1377.161;571.7479;Comment;5;77;75;76;74;73;Brightness;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;45;-2549.413,-450.5335;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-3502.554,-1018.326;Inherit;False;42;a;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-3324.808,-1290.904;Inherit;True;63;falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;75;-3264.49,-1058.851;Inherit;False;3;0;FLOAT;8;False;1;FLOAT;2;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;46;-2315.371,-450.5337;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;76;-3033.296,-1143.779;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;66;-2109.85,-457.0301;Inherit;False;rgbOffset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;38;-3192.856,564.6829;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-2863.022,658.3239;Inherit;False;66;rgbOffset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-2587.727,-1144.563;Inherit;False;brightnessMultiplier;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-2651.094,800.8646;Inherit;False;77;brightnessMultiplier;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-2598.507,572.3963;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-2262.003,570.761;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;-1978.267,568.2348;Float;False;True;-1;2;;100;1;NeoFPS/Standard/LaserPointerDot;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;4;1;False;-1;1;False;-1;0;5;False;-1;10;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;5;False;-1;255;False;-1;255;False;-1;4;False;-1;0;False;-1;0;False;-1;0;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;7;False;-1;True;False;0;False;-1;0;False;-1;True;2;RenderType=Overlay=RenderType;Queue=Transparent=Queue=0;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;26;0;24;0
WireConnection;28;0;26;0
WireConnection;27;0;28;0
WireConnection;29;0;27;0
WireConnection;42;0;61;4
WireConnection;63;0;29;0
WireConnection;36;0;65;0
WireConnection;36;1;43;0
WireConnection;36;2;33;0
WireConnection;51;0;36;0
WireConnection;51;1;36;0
WireConnection;45;0;51;0
WireConnection;75;2;73;0
WireConnection;46;0;45;0
WireConnection;76;0;74;0
WireConnection;76;1;75;0
WireConnection;66;0;46;0
WireConnection;38;0;61;0
WireConnection;77;0;76;0
WireConnection;48;0;38;0
WireConnection;48;1;67;0
WireConnection;50;0;48;0
WireConnection;50;1;69;0
WireConnection;0;0;50;0
ASEEND*/
//CHKSM=D8BB73F510D91CE7B1580876017EC203D9041E8A