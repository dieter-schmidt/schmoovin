// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/FirstPersonWeapon_StencilMetallic"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Normals("Normals", 2D) = "bump" {}
		_Metallic("Metallic", 2D) = "white" {}
		_MetallicMultiplier("MetallicMultiplier", Range( 0 , 1)) = 1
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_StencilMask("StencilMask", Int) = 6
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		Stencil
		{
			Ref [_StencilMask]
			ReadMask [_StencilMask]
			Comp NotEqual
		}
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform int _StencilMask;
		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;
		uniform float _MetallicMultiplier;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normals, uv_Normals ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * _Color ).rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			float4 tex2DNode37 = tex2D( _Metallic, uv_Metallic );
			o.Metallic = ( tex2DNode37.r * _MetallicMultiplier );
			o.Smoothness = ( _Smoothness * tex2DNode37.a );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18200
180;354;1608;790;4252.548;1909.846;1.712314;True;True
Node;AmplifyShaderEditor.CommentaryNode;25;-3872,-1808;Inherit;False;1874.499;1310.455;Comment;12;24;70;74;54;48;77;53;73;37;78;80;81;Textures;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;37;-3494.403,-1212.805;Inherit;True;Property;_Metallic;Metallic;3;0;Create;True;0;0;True;0;False;-1;None;84d76c914224da14a8210ba4ba8a2992;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;77;-3180.318,-908.9162;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-3134.633,-999.7856;Float;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;True;0;False;1;0.144;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;73;-3158.403,-1580.805;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;True;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;53;-3494.403,-1682.958;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;True;0;False;-1;None;7130c16fd8005b546b111d341310a9a4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;80;-3134.825,-1091.312;Inherit;False;Property;_MetallicMultiplier;MetallicMultiplier;4;0;Create;True;0;0;True;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-2886.403,-1676.805;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-2827.901,-898.7553;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-2828.973,-1253.292;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;54;-3494.403,-1436.805;Inherit;True;Property;_Normals;Normals;2;0;Create;True;0;0;True;0;False;-1;None;11f03d9db1a617e40b7ece71f0a84f6f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IntNode;78;-2241.547,-1441.536;Inherit;False;Property;_StencilMask;StencilMask;6;0;Create;True;0;0;True;0;False;6;0;0;1;INT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;24;-2294.403,-1324.805;Float;False;True;-1;2;;0;0;Standard;NeoFPS/Standard/FirstPersonWeapon_StencilMetallic;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;True;6;True;78;255;True;78;255;False;-1;6;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;77;0;37;4
WireConnection;74;0;53;0
WireConnection;74;1;73;0
WireConnection;70;0;48;0
WireConnection;70;1;77;0
WireConnection;81;0;37;1
WireConnection;81;1;80;0
WireConnection;24;0;74;0
WireConnection;24;1;54;0
WireConnection;24;3;81;0
WireConnection;24;4;70;0
ASEEND*/
//CHKSM=6B4DB4052994355A5B8440A9F8D05334F56114A9