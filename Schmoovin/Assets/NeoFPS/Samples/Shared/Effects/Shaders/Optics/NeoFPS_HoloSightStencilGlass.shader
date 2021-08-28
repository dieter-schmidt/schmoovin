// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/HoloSightStencilGlass"
{
	Properties
	{
		_GlassColor("GlassColor", Color) = (0,0.3137255,0.4705882,0.07843138)
		_Smoothness("Smoothness", 2D) = "white" {}
		_SmoothnessMultiplier("SmoothnessMultiplier", Range( 0 , 1)) = 0.95
		_StencilMask1("StencilMask", Int) = 5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" }
		Cull Back
		Stencil
		{
			Ref [_StencilMask1]
			Comp Always
			Pass Replace
		}
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow nolightmap  nodirlightmap 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform int _StencilMask1;
		uniform float4 _GlassColor;
		uniform sampler2D _Smoothness;
		uniform float4 _Smoothness_ST;
		uniform float _SmoothnessMultiplier;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _GlassColor.rgb;
			o.Metallic = 0.0;
			float2 uv_Smoothness = i.uv_texcoord * _Smoothness_ST.xy + _Smoothness_ST.zw;
			o.Smoothness = ( tex2D( _Smoothness, uv_Smoothness ) * _SmoothnessMultiplier ).r;
			o.Alpha = _GlassColor.a;
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=18200
84;415;1722;833;1508.685;282.0734;1.3;True;True
Node;AmplifyShaderEditor.RangedFloatNode;6;-835.2851,379.6267;Inherit;False;Property;_SmoothnessMultiplier;SmoothnessMultiplier;2;0;Create;True;0;0;False;0;False;0.95;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-853.4541,181.4547;Inherit;True;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;True;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-811.8863,-103.9732;Inherit;False;Property;_GlassColor;GlassColor;0;0;Create;True;0;0;True;0;False;0,0.3137255,0.4705882,0.07843138;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-263.2856,62.42663;Inherit;False;Constant;_Metallic;Metallic;2;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-507.6852,185.9266;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.IntNode;8;60.41488,-113.0735;Inherit;False;Property;_StencilMask1;StencilMask;3;0;Create;True;0;0;True;0;False;5;0;0;1;INT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;1;1.3,0;Float;False;True;-1;2;;0;0;Standard;NeoFPS/Standard/HoloSightStencilGlass;False;False;False;False;False;False;True;False;True;False;False;False;False;False;True;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;True;5;True;8;255;False;-1;255;False;-1;7;False;-1;3;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;2;0
WireConnection;7;1;6;0
WireConnection;1;0;4;0
WireConnection;1;3;5;0
WireConnection;1;4;7;0
WireConnection;1;9;4;4
ASEEND*/
//CHKSM=6F622E7AA2BC8C31F33BAC78BEFDB1CA5B837C90