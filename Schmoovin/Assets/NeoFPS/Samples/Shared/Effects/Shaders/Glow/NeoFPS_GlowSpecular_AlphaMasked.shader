// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/GlowSpecular (Alpha Masked)"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Normals("Normals", 2D) = "bump" {}
		_Specular("Specular", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Occlusion("Occlusion", 2D) = "white" {}
		_GlowRamp("GlowRamp", 2D) = "white" {}
		_GlowMask("Glow Mask", 2D) = "white" {}
		_MaxGlow("Max Glow", Range( 0.1 , 2)) = 1.25
		[PerRendererData]_Glow("Glow", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normals;
		uniform sampler2D _Albedo;
		uniform sampler2D _GlowRamp;
		uniform sampler2D _GlowMask;
		uniform float _Glow;
		uniform float _MaxGlow;
		uniform sampler2D _Specular;
		uniform float _Smoothness;
		uniform sampler2D _Occlusion;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float3 normals38 = UnpackNormal( tex2D( _Normals, i.uv_texcoord ) );
			o.Normal = normals38;
			float4 albedo36 = tex2D( _Albedo, i.uv_texcoord );
			o.Albedo = albedo36.rgb;
			float glowMask37 = tex2D( _GlowMask, i.uv_texcoord ).r;
			float temp_output_34_0 = saturate( ( glowMask37 * _Glow * _MaxGlow ) );
			float2 appendResult63 = (float2(temp_output_34_0 , 0.5));
			float4 glowEmission46 = tex2D( _GlowRamp, appendResult63 );
			o.Emission = glowEmission46.rgb;
			float4 tex2DNode13 = tex2D( _Specular, i.uv_texcoord );
			float4 specular39 = tex2DNode13;
			o.Specular = specular39.rgb;
			float specularAlpha40 = tex2DNode13.a;
			float glowAmount29 = temp_output_34_0;
			float smoothness43 = ( _Smoothness * specularAlpha40 * ( 1.0 - glowAmount29 ) );
			o.Smoothness = smoothness43;
			float occlusion54 = tex2D( _Occlusion, i.uv_texcoord ).r;
			float glowOcclusion59 = saturate( ( occlusion54 + glowAmount29 ) );
			o.Occlusion = glowOcclusion59;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18200
66;270;1539;965;3072.058;654.322;1.450536;True;True
Node;AmplifyShaderEditor.CommentaryNode;35;-2581.095,-991.8542;Inherit;False;896.6844;1246.495;Comment;12;53;54;36;38;39;14;12;40;13;37;20;6;Textures;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-2562.275,-625.9635;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;20;-2260.034,-653.5404;Inherit;True;Property;_GlowMask;Glow Mask;6;0;Create;True;0;0;False;0;False;-1;None;9789d23040cb1fb45ad60392430c3c15;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;51;-1504.416,-640.4607;Inherit;False;1501.209;515.7075;Comment;9;46;62;63;29;34;24;25;52;33;Glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-1938.259,-630.1752;Inherit;False;glowMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1474.427,-375.8379;Inherit;False;Property;_MaxGlow;Max Glow;7;0;Create;True;0;0;False;0;False;1.25;1.295908;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1474.292,-466.2764;Inherit;False;Property;_Glow;Glow;8;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-1388.105,-560.0742;Inherit;False;37;glowMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1123.802,-483.8361;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;53;-2267.885,38.58816;Inherit;True;Property;_Occlusion;Occlusion;4;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;34;-964.801,-484.0402;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;29;-785.6693,-306.4248;Inherit;False;glowAmount;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;60;-2544.199,683.522;Inherit;False;866;260;Comment;5;58;59;57;55;56;Occlusion;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;13;-2264.089,-181.9612;Inherit;True;Property;_Specular;Specular;2;0;Create;True;0;0;False;0;False;-1;None;84d76c914224da14a8210ba4ba8a2992;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;54;-1932.757,60.59587;Inherit;False;occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;44;-2597.863,333.6167;Inherit;False;941.3788;302.1574;Comment;6;43;30;28;42;15;31;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;56;-2504.781,850.5159;Inherit;False;29;glowAmount;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-1930.764,-89.98881;Inherit;False;specularAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;55;-2507.081,747.8371;Inherit;False;54;occlusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-2502.674,551.1644;Inherit;False;29;glowAmount;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;63;-791.9719,-484.335;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-2508.058,475.0333;Inherit;False;40;specularAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-2277.118,555.4061;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2568.077,395.8182;Float;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;False;1;0.144;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;57;-2253.499,752.8101;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-2258.449,-907.362;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;False;-1;None;7130c16fd8005b546b111d341310a9a4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-2261.483,-419.8405;Inherit;True;Property;_Normals;Normals;1;0;Create;True;0;0;False;0;False;-1;None;11f03d9db1a617e40b7ece71f0a84f6f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;62;-630.0463,-512.9424;Inherit;True;Property;_GlowRamp;GlowRamp;5;0;Create;True;0;0;False;0;False;-1;38b47325704c0bf4aaa42915f812545e;38b47325704c0bf4aaa42915f812545e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-2060.674,400.1643;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;58;-2085.201,752.522;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-1904.2,747.522;Inherit;False;glowOcclusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;50;-1480.323,180.4646;Inherit;False;613.0461;611.6289;Comment;7;0;47;49;48;41;45;61;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;38;-1939.563,-420.6124;Inherit;False;normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-333.5374,-512.5349;Inherit;False;glowEmission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;-1929.764,-181.5387;Inherit;False;specular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;36;-1933.361,-907.3748;Inherit;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;43;-1881.988,394.8911;Inherit;False;smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-1390.891,342.1203;Inherit;False;38;normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-1392.912,262.3444;Inherit;False;36;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-1424.329,683.9854;Inherit;False;59;glowOcclusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-1388.15,507.4948;Inherit;False;39;specular;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-1411.495,594.2526;Inherit;False;43;smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;-1422.374,426.2137;Inherit;False;46;glowEmission;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-1108.87,354.2271;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;NeoFPS/Standard/GlowSpecular (Alpha Masked);False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;1;6;0
WireConnection;37;0;20;1
WireConnection;24;0;52;0
WireConnection;24;1;25;0
WireConnection;24;2;33;0
WireConnection;53;1;6;0
WireConnection;34;0;24;0
WireConnection;29;0;34;0
WireConnection;13;1;6;0
WireConnection;54;0;53;1
WireConnection;40;0;13;4
WireConnection;63;0;34;0
WireConnection;28;0;31;0
WireConnection;57;0;55;0
WireConnection;57;1;56;0
WireConnection;12;1;6;0
WireConnection;14;1;6;0
WireConnection;62;1;63;0
WireConnection;30;0;15;0
WireConnection;30;1;42;0
WireConnection;30;2;28;0
WireConnection;58;0;57;0
WireConnection;59;0;58;0
WireConnection;38;0;14;0
WireConnection;46;0;62;0
WireConnection;39;0;13;0
WireConnection;36;0;12;0
WireConnection;43;0;30;0
WireConnection;0;0;49;0
WireConnection;0;1;48;0
WireConnection;0;2;47;0
WireConnection;0;3;41;0
WireConnection;0;4;45;0
WireConnection;0;5;61;0
ASEEND*/
//CHKSM=4BB006DB6F18994E54D9FD4CADBF4C3BF5B206D9