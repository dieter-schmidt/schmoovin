// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/RenderTextureScope"
{
	Properties
	{
		[PerRendererData]_RenderTexture("RenderTexture", 2D) = "white" {}
		[PerRendererData]_ScopeTransparency("ScopeTransparency", Range( 0 , 1)) = 0
		_OpaqueColour("OpaqueColour", Color) = (0.1912846,0.2086938,0.2132353,1)
		_OpaqueReflectivity("OpaqueReflectivity", Range( 0 , 1)) = 0.5
		_InternalReflections("InternalReflections", Range( 0 , 1)) = 0.85
		_ScopeRingColour("ScopeRingColour", Color) = (0,0,0,1)
		_ScopeRingNormalised("ScopeRingNormalised", Range( 0.5 , 2)) = 1.2
		_ScopeRingFocus("ScopeRingFocus", Range( 0 , 1)) = 0.1
		[PerRendererData]_ScopeParallax("ScopeParallax", Vector) = (0,0,0,0)
		_ReflectionMask("ReflectionMask", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows noshadow nolightmap  
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _OpaqueColour;
		uniform sampler2D _RenderTexture;
		uniform float4 _RenderTexture_ST;
		uniform float4 _ScopeRingColour;
		uniform float2 _ScopeParallax;
		uniform float _ScopeRingNormalised;
		uniform float _ScopeRingFocus;
		uniform float _ScopeTransparency;
		uniform sampler2D _ReflectionMask;
		uniform float4 _ReflectionMask_ST;
		uniform float _OpaqueReflectivity;
		uniform float _InternalReflections;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_RenderTexture = i.uv_texcoord * _RenderTexture_ST.xy + _RenderTexture_ST.zw;
			float temp_output_22_0 = abs( ( ( ( i.uv_texcoord.x - 0.5 ) - _ScopeParallax.x ) * 2.0 ) );
			float temp_output_20_0 = abs( ( ( ( i.uv_texcoord.y - 0.5 ) - _ScopeParallax.y ) * 2.0 ) );
			float clampResult36 = clamp( ( ( sqrt( ( ( temp_output_22_0 * temp_output_22_0 ) + ( temp_output_20_0 * temp_output_20_0 ) ) ) - _ScopeRingNormalised ) * ( ( _ScopeRingFocus * 10.0 ) + 1.0 ) ) , 0.0 , 1.0 );
			float temp_output_40_0 = ( clampResult36 * clampResult36 );
			float4 lerpResult58 = lerp( tex2D( _RenderTexture, uv_RenderTexture ) , _ScopeRingColour , temp_output_40_0);
			float4 lerpResult13 = lerp( _OpaqueColour , lerpResult58 , _ScopeTransparency);
			o.Albedo = lerpResult13.rgb;
			o.Emission = ( lerpResult13 * _ScopeTransparency ).rgb;
			float2 uv_ReflectionMask = i.uv_texcoord * _ReflectionMask_ST.xy + _ReflectionMask_ST.zw;
			o.Smoothness = ( ( tex2D( _ReflectionMask, uv_ReflectionMask ).r * _OpaqueReflectivity ) * ( 1.0 - ( _ScopeTransparency * ( 1.0 - ( temp_output_40_0 * _InternalReflections ) ) ) ) );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18200
84;415;1722;833;2003.493;1175.349;1.546617;True;True
Node;AmplifyShaderEditor.RangedFloatNode;15;-3395.76,82.56621;Inherit;False;Constant;_rescaleHalf;rescaleHalf;7;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-3493.029,-121.8721;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;17;-3165.843,-74.60886;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;6;-3165.566,61.90887;Inherit;False;Property;_ScopeParallax;ScopeParallax;8;1;[PerRendererData];Create;True;0;0;True;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;24;-3165.268,-173.0509;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-2877.173,79.43991;Inherit;False;Constant;_rescale2x;rescale2x;7;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;61;-2908.054,-72.55829;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;60;-2907.054,-171.5583;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-2649.217,-70.98225;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-2648.642,-169.4241;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;20;-2486.879,-69.25529;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;22;-2486.305,-167.6972;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-2288.274,-65.8035;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-2290,-190.1467;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2082.371,424.9818;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-2081.035,-138.3377;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-2201.621,347.0938;Inherit;False;Property;_ScopeRingFocus;ScopeRingFocus;7;0;Create;True;0;0;True;0;False;0.1;0.189;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1906.371,515.9819;Inherit;False;Constant;_Float2;Float 2;7;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-1875.371,377.9818;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;27;-1937.693,-138.3378;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-2203.221,260.7983;Inherit;False;Property;_ScopeRingNormalised;ScopeRingNormalised;6;0;Create;True;0;0;True;0;False;1.2;0.708;0.5;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;-1711.371,377.9818;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;31;-1745.857,6.726437;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1527.455,8.026722;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;36;-1348.055,9.32671;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-1313.696,-694.3519;Inherit;False;Property;_InternalReflections;InternalReflections;4;0;Create;True;0;0;False;0;False;0.85;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-1164.246,-0.3811455;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1019.84,-703.6323;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;59;-1229.535,-207.3608;Inherit;False;Property;_ScopeRingColour;ScopeRingColour;5;0;Create;True;0;0;True;0;False;0,0,0,1;0.05579581,0.05757067,0.0588235,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-1313.538,-424.155;Inherit;True;Property;_RenderTexture;RenderTexture;0;1;[PerRendererData];Create;True;0;0;True;0;False;-1;None;c68296334e691ed45b62266cbc716628;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-1156.296,-790.4099;Inherit;False;Property;_ScopeTransparency;ScopeTransparency;1;1;[PerRendererData];Create;True;0;0;True;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;70;-752.1537,-760.4968;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;58;-840.523,-225.4593;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-565.7764,-783.1682;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;78;-1081.709,-1040.794;Inherit;True;Property;_ReflectionMask;ReflectionMask;9;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-1057.381,-1130.722;Inherit;False;Property;_OpaqueReflectivity;OpaqueReflectivity;3;0;Create;True;0;0;True;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-1267.64,-611.1835;Inherit;False;Property;_OpaqueColour;OpaqueColour;2;0;Create;True;0;0;True;0;False;0.1912846,0.2086938,0.2132353,1;0.4578287,0.5302321,0.5367647,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;13;-546.3655,-475.2574;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;75;-397.9883,-782.465;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-640.9235,-1084.1;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-120.6333,-392.6794;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-135.1638,-839.5616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;215.7439,-466.7347;Float;False;True;-1;2;;0;0;Standard;NeoFPS/Standard/RenderTextureScope;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0.05;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;14;2
WireConnection;17;1;15;0
WireConnection;24;0;14;1
WireConnection;24;1;15;0
WireConnection;61;0;17;0
WireConnection;61;1;6;2
WireConnection;60;0;24;0
WireConnection;60;1;6;1
WireConnection;18;0;61;0
WireConnection;18;1;19;0
WireConnection;23;0;60;0
WireConnection;23;1;19;0
WireConnection;20;0;18;0
WireConnection;22;0;23;0
WireConnection;25;0;20;0
WireConnection;25;1;20;0
WireConnection;21;0;22;0
WireConnection;21;1;22;0
WireConnection;26;0;21;0
WireConnection;26;1;25;0
WireConnection;42;0;3;0
WireConnection;42;1;41;0
WireConnection;27;0;26;0
WireConnection;43;0;42;0
WireConnection;43;1;44;0
WireConnection;31;0;27;0
WireConnection;31;1;2;0
WireConnection;32;0;31;0
WireConnection;32;1;43;0
WireConnection;36;0;32;0
WireConnection;40;0;36;0
WireConnection;40;1;36;0
WireConnection;76;0;40;0
WireConnection;76;1;77;0
WireConnection;70;0;76;0
WireConnection;58;0;4;0
WireConnection;58;1;59;0
WireConnection;58;2;40;0
WireConnection;74;0;7;0
WireConnection;74;1;70;0
WireConnection;13;0;12;0
WireConnection;13;1;58;0
WireConnection;13;2;7;0
WireConnection;75;0;74;0
WireConnection;79;0;78;1
WireConnection;79;1;8;0
WireConnection;65;0;13;0
WireConnection;65;1;7;0
WireConnection;69;0;79;0
WireConnection;69;1;75;0
WireConnection;0;0;13;0
WireConnection;0;2;65;0
WireConnection;0;4;69;0
ASEEND*/
//CHKSM=253748024D0D14CCBE071270A33151536EB73EFF