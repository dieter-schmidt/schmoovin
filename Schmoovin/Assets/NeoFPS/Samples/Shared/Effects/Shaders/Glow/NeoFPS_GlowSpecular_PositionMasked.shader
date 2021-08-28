// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/GlowSpecular (Position Masked)"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Normals("Normals", 2D) = "bump" {}
		_Specular("Specular", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Occlusion("Occlusion", 2D) = "white" {}
		_GlowRamp("Glow Ramp", 2D) = "white" {}
		_StartPositionZ("Start Position Z", Float) = 0
		_EndPositionZ("End Position Z", Float) = 0
		_MaxGlow("Max Glow", Range( 0.1 , 2)) = 1
		[PerRendererData]_Glow("Glow", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _Normals;
		uniform sampler2D _Albedo;
		uniform sampler2D _GlowRamp;
		uniform float _StartPositionZ;
		uniform float _EndPositionZ;
		uniform float _Glow;
		uniform float _MaxGlow;
		uniform sampler2D _Specular;
		uniform float _Smoothness;
		uniform sampler2D _Occlusion;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float3 normals78 = UnpackNormal( tex2D( _Normals, i.uv_texcoord ) );
			o.Normal = normals78;
			float4 albedo79 = tex2D( _Albedo, i.uv_texcoord );
			o.Albedo = albedo79.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 worldToObj55 = mul( unity_WorldToObject, float4( ase_worldPos, 1 ) ).xyz;
			float temp_output_66_0 = abs( (-1.0 + (worldToObj55.z - _StartPositionZ) * (1.0 - -1.0) / (_EndPositionZ - _StartPositionZ)) );
			float positionMask34 = ( 1.0 - ( temp_output_66_0 * temp_output_66_0 ) );
			float temp_output_70_0 = saturate( ( positionMask34 * _Glow * _MaxGlow ) );
			float2 appendResult72 = (float2(temp_output_70_0 , 0.5));
			float4 glowEmission74 = tex2D( _GlowRamp, appendResult72 );
			o.Emission = glowEmission74.rgb;
			float4 tex2DNode13 = tex2D( _Specular, i.uv_texcoord );
			float4 specular76 = tex2DNode13;
			o.Specular = specular76.rgb;
			float glowAmount29 = temp_output_70_0;
			float smoothness80 = ( _Smoothness * ( 1.0 - glowAmount29 ) );
			o.Smoothness = smoothness80;
			float occlusion81 = tex2D( _Occlusion, i.uv_texcoord ).r;
			float glowOcclusion82 = saturate( ( occlusion81 + glowAmount29 ) );
			o.Occlusion = glowOcclusion82;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18200
66;270;1539;965;406.4274;-288.1298;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;99;-1701.773,-502.7741;Inherit;False;1692.214;416.6712;Comment;9;34;67;68;66;62;65;55;64;38;Position Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;38;-1662.588,-428.9485;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;64;-1358.978,-270.3699;Inherit;False;Property;_StartPositionZ;Start Position Z;6;0;Create;True;0;0;False;0;False;0;-0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;55;-1430.102,-434.4836;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;65;-1354.307,-184.7331;Inherit;False;Property;_EndPositionZ;End Position Z;7;0;Create;True;0;0;False;0;False;0;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;62;-1017.987,-359.1205;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;66;-807.7864,-359.1202;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-627.1707,-359.1207;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;67;-448.1121,-359.1202;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;97;-1425.861,49.42542;Inherit;False;1424.783;431.1957;Comment;9;74;73;72;29;70;24;33;35;25;Glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-240.8486,-363.715;Inherit;False;positionMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1393.155,217.971;Inherit;False;Property;_Glow;Glow;9;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-1331.91,131.7937;Inherit;False;34;positionMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;98;-2045.537,591.8673;Inherit;False;959.2197;910.5012;Comment;10;77;76;79;78;13;12;14;81;83;6;Textures;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1393.29,308.4096;Inherit;False;Property;_MaxGlow;Max Glow;8;0;Create;True;0;0;False;0;False;1;1;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-2015.794,997.9007;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1056.617,200.6305;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;83;-1644.91,1284.577;Inherit;True;Property;_Occlusion;Occlusion;4;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;70;-909.7043,200.183;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;29;-701.7662,373.7725;Inherit;False;glowAmount;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-1345.886,1307.024;Inherit;False;occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;96;-846.1328,676.0159;Inherit;False;834.5988;281.4047;Comment;5;80;30;28;15;31;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;88;-846.9122,1114.815;Inherit;False;837.5574;253.5325;Comment;5;86;84;82;85;87;Occlusion;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;-797.8053,1182.927;Inherit;False;81;occlusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-813.6607,844.4399;Inherit;False;29;glowAmount;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-818.8784,1278.604;Inherit;False;29;glowAmount;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;86;-566.9417,1188.571;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-704.9884,759.0748;Float;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;False;1;0.144;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-590.4893,849.1017;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;72;-716.6155,201.4099;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;14;-1641.83,880.5125;Inherit;True;Property;_Normals;Normals;1;0;Create;True;0;0;False;0;False;-1;None;11f03d9db1a617e40b7ece71f0a84f6f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-1640.291,670.9813;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;False;-1;None;7130c16fd8005b546b111d341310a9a4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;-1644.121,1085.479;Inherit;True;Property;_Specular;Specular;2;0;Create;True;0;0;False;0;False;-1;None;84d76c914224da14a8210ba4ba8a2992;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;87;-410.3984,1188.571;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;73;-556.2947,173.5385;Inherit;True;Property;_GlowRamp;Glow Ramp;5;0;Create;True;0;0;False;0;False;-1;None;38b47325704c0bf4aaa42915f812545e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-400.5851,764.421;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;78;-1333.563,881.166;Inherit;False;normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;79;-1322.563,671.166;Inherit;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;74;-237.2947,173.5385;Inherit;False;glowEmission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;76;-1324.121,1085.479;Inherit;False;specular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;95;173.2631,221.6057;Inherit;False;572.6389;599.7328;Comment;7;0;90;93;94;92;89;91;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;-240.7545,759.1904;Inherit;False;smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;82;-228.5157,1184.503;Inherit;False;glowOcclusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;200.4825,704.7813;Inherit;False;82;glowOcclusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;210.8937,623.4994;Inherit;False;80;smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-1326.121,1177.479;Inherit;False;specularAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;91;230.4614,540.7123;Inherit;False;76;specular;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;94;228.9562,286.3301;Inherit;False;79;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;230.4613,372.1277;Inherit;False;78;normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;197.3467,457.9252;Inherit;False;74;glowEmission;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;506.8159,390.9518;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;NeoFPS/Standard/GlowSpecular (Position Masked);False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;55;0;38;0
WireConnection;62;0;55;3
WireConnection;62;1;64;0
WireConnection;62;2;65;0
WireConnection;66;0;62;0
WireConnection;68;0;66;0
WireConnection;68;1;66;0
WireConnection;67;0;68;0
WireConnection;34;0;67;0
WireConnection;24;0;35;0
WireConnection;24;1;25;0
WireConnection;24;2;33;0
WireConnection;83;1;6;0
WireConnection;70;0;24;0
WireConnection;29;0;70;0
WireConnection;81;0;83;1
WireConnection;86;0;84;0
WireConnection;86;1;85;0
WireConnection;28;0;31;0
WireConnection;72;0;70;0
WireConnection;14;1;6;0
WireConnection;12;1;6;0
WireConnection;13;1;6;0
WireConnection;87;0;86;0
WireConnection;73;1;72;0
WireConnection;30;0;15;0
WireConnection;30;1;28;0
WireConnection;78;0;14;0
WireConnection;79;0;12;0
WireConnection;74;0;73;0
WireConnection;76;0;13;0
WireConnection;80;0;30;0
WireConnection;82;0;87;0
WireConnection;77;0;13;4
WireConnection;0;0;94;0
WireConnection;0;1;93;0
WireConnection;0;2;92;0
WireConnection;0;3;91;0
WireConnection;0;4;90;0
WireConnection;0;5;89;0
ASEEND*/
//CHKSM=31E8168FF728BB657AE9102329C588443118419F