// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/GlowMetallic (Distance Masked)"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Normals("Normals", 2D) = "bump" {}
		_Metallic("Metallic", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Occlusion("Occlusion", 2D) = "white" {}
		_GlowRamp("Glow Ramp", 2D) = "white" {}
		_GlowCenter("Glow Center", Vector) = (0,0,0,0)
		_MinDistance("Min Distance", Float) = 0
		_MaxDistance("Max Distance", Float) = 1
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
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _Normals;
		uniform sampler2D _Albedo;
		uniform sampler2D _GlowRamp;
		uniform float3 _GlowCenter;
		uniform float _MinDistance;
		uniform float _MaxDistance;
		uniform float _Glow;
		uniform float _MaxGlow;
		uniform sampler2D _Metallic;
		uniform float _Smoothness;
		uniform sampler2D _Occlusion;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 normals81 = UnpackNormal( tex2D( _Normals, i.uv_texcoord ) );
			o.Normal = normals81;
			float4 albedo79 = tex2D( _Albedo, i.uv_texcoord );
			o.Albedo = albedo79.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 worldToObj55 = mul( unity_WorldToObject, float4( ase_worldPos, 1 ) ).xyz;
			float clampResult73 = clamp( (0.0 + (distance( worldToObj55 , _GlowCenter ) - _MinDistance) * (1.0 - 0.0) / (_MaxDistance - _MinDistance)) , 0.0 , 1.0 );
			float distanceMask34 = ( 1.0 - ( clampResult73 * clampResult73 ) );
			float temp_output_74_0 = saturate( ( distanceMask34 * _Glow * _MaxGlow ) );
			float2 appendResult76 = (float2(temp_output_74_0 , 0.5));
			float4 glowEmission77 = tex2D( _GlowRamp, appendResult76 );
			o.Emission = glowEmission77.rgb;
			float4 tex2DNode13 = tex2D( _Metallic, i.uv_texcoord );
			float metallic83 = tex2DNode13.r;
			o.Metallic = metallic83;
			float glowAmount29 = temp_output_74_0;
			float metallicAlpha84 = tex2DNode13.a;
			float smoothness90 = ( _Smoothness * ( 1.0 - glowAmount29 ) * metallicAlpha84 );
			o.Smoothness = smoothness90;
			float occlusion88 = tex2D( _Occlusion, i.uv_texcoord ).r;
			float glowOcclusion97 = saturate( ( occlusion88 + glowAmount29 ) );
			o.Occlusion = glowOcclusion97;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18200
66;270;1539;965;1835.728;1313.123;1.835363;True;True
Node;AmplifyShaderEditor.CommentaryNode;102;-1457.802,-671.5609;Inherit;False;1685.647;537.7411;Comment;11;34;67;68;73;62;64;70;65;69;55;38;Distance Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;38;-1415.34,-604.4011;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;55;-1179.854,-609.9362;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;69;-1171.313,-462.3744;Inherit;False;Property;_GlowCenter;Glow Center;6;0;Create;True;0;0;False;0;False;0,0,0;0,2.5,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;65;-1129.816,-227.0052;Inherit;False;Property;_MaxDistance;Max Distance;8;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;70;-924.2118,-535.8891;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-1126.83,-311.1099;Inherit;False;Property;_MinDistance;Min Distance;7;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;62;-750.6134,-534.6364;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;73;-555.1134,-534.3568;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-365.9231,-531.5734;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;67;-186.8641,-531.5731;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;5.542019,-536.1677;Inherit;False;distanceMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;100;-1175.11,45.70592;Inherit;False;1418.184;425.3746;Comment;9;77;75;76;29;74;24;35;25;33;Glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;101;-1980.659,611.789;Inherit;False;983.3735;953.8519;Comment;10;83;79;81;14;12;84;13;88;87;6;Textures;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1122.552,299.6358;Inherit;False;Property;_MaxGlow;Max Glow;9;0;Create;True;0;0;False;0;False;1;1;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1122.417,209.1973;Inherit;False;Property;_Glow;Glow;10;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-1061.172,123.02;Inherit;False;34;distanceMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-772.8783,191.8568;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-1952.009,1138.421;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;87;-1551.349,1142.021;Inherit;True;Property;_Occlusion;Occlusion;4;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;74;-625.1402,192.1089;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;29;-157.4983,374.5457;Inherit;False;glowAmount;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;93;-853.1874,730.4073;Inherit;False;878.0858;316.9209;Comment;6;90;30;15;86;28;31;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;13;-1553.564,1351.402;Inherit;True;Property;_Metallic;Metallic;2;0;Create;True;0;0;False;0;False;-1;None;84d76c914224da14a8210ba4ba8a2992;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;88;-1259.744,1165.139;Inherit;False;occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;99;-848.9575,1124.154;Inherit;False;842;259;Comment;5;97;96;95;89;94;Occlusion;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-806.7225,1191.341;Inherit;False;88;occlusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;94;-825.7396,1300.067;Inherit;False;29;glowAmount;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-814.6878,872.7962;Inherit;False;29;glowAmount;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-1240.079,1443.658;Inherit;False;metallicAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-679.2263,797.5887;Float;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;False;1;0.144;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;76;-432.7296,191.1592;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-620.0483,955.6809;Inherit;False;84;metallicAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;95;-577.2725,1196.489;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-568.3315,876.9726;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-374.823,802.9349;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-1555.263,727.6141;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;False;-1;None;7130c16fd8005b546b111d341310a9a4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;96;-407.4726,1194.889;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;75;-274.2105,162.4423;Inherit;True;Property;_GlowRamp;Glow Ramp;5;0;Create;True;0;0;False;0;False;-1;38b47325704c0bf4aaa42915f812545e;38b47325704c0bf4aaa42915f812545e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-1556.802,937.1453;Inherit;True;Property;_Normals;Normals;1;0;Create;True;0;0;False;0;False;-1;None;11f03d9db1a617e40b7ece71f0a84f6f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;29.96173,161.8123;Inherit;False;glowEmission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-1242.465,1374.538;Inherit;False;metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-1231.463,937.2979;Inherit;False;normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;79;-1225.879,728.032;Inherit;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;92;405.3732,-209.0416;Inherit;False;613;621;Comment;7;0;78;82;80;85;91;98;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-199.33,798.6907;Inherit;False;smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;97;-224.0724,1191.089;Inherit;False;glowOcclusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;466.6764,-148.5222;Inherit;False;79;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;466.7906,-63.21297;Inherit;False;81;normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;91;442.8962,191.7218;Inherit;False;90;smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;435.238,22.91466;Inherit;False;77;glowEmission;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;468.6174,105.0437;Inherit;False;83;metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;432.1576,282.4453;Inherit;False;97;glowOcclusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;777.6655,-18.63726;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;NeoFPS/Standard/GlowMetallic (Distance Masked);False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;55;0;38;0
WireConnection;70;0;55;0
WireConnection;70;1;69;0
WireConnection;62;0;70;0
WireConnection;62;1;64;0
WireConnection;62;2;65;0
WireConnection;73;0;62;0
WireConnection;68;0;73;0
WireConnection;68;1;73;0
WireConnection;67;0;68;0
WireConnection;34;0;67;0
WireConnection;24;0;35;0
WireConnection;24;1;25;0
WireConnection;24;2;33;0
WireConnection;87;1;6;0
WireConnection;74;0;24;0
WireConnection;29;0;74;0
WireConnection;13;1;6;0
WireConnection;88;0;87;1
WireConnection;84;0;13;4
WireConnection;76;0;74;0
WireConnection;95;0;89;0
WireConnection;95;1;94;0
WireConnection;28;0;31;0
WireConnection;30;0;15;0
WireConnection;30;1;28;0
WireConnection;30;2;86;0
WireConnection;12;1;6;0
WireConnection;96;0;95;0
WireConnection;75;1;76;0
WireConnection;14;1;6;0
WireConnection;77;0;75;0
WireConnection;83;0;13;1
WireConnection;81;0;14;0
WireConnection;79;0;12;0
WireConnection;90;0;30;0
WireConnection;97;0;96;0
WireConnection;0;0;80;0
WireConnection;0;1;82;0
WireConnection;0;2;78;0
WireConnection;0;3;85;0
WireConnection;0;4;91;0
WireConnection;0;5;98;0
ASEEND*/
//CHKSM=B2A021901AED50A4A1968E7F13CEA5D5D8D0B5D9