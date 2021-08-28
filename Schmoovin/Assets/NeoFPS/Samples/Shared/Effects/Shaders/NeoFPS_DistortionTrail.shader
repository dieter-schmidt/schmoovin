// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/DistortionTrail (EdgeBlend)"
{
	Properties
	{
		_Noise("Noise", 2D) = "bump" {}
		_Distortion("Distortion", Range( 0 , 1)) = 0
		_USpeed("U Speed", Float) = 0
		_VSpeed("V Speed", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _Noise;
		uniform float _USpeed;
		uniform float _VSpeed;
		uniform float _Distortion;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float mulTime104 = _Time.y * -_USpeed;
			float mulTime108 = _Time.y * _VSpeed;
			float2 appendResult106 = (float2(mulTime104 , mulTime108));
			float2 uv_TexCoord105 = i.uv_texcoord + appendResult106;
			float temp_output_88_0 = abs( (-1.0 + (i.uv_texcoord.y - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) );
			float temp_output_87_0 = ( 1.0 - ( temp_output_88_0 * temp_output_88_0 ) );
			float edgeBlend98 = ( temp_output_87_0 * temp_output_87_0 );
			float4 screenColor8 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( (ase_grabScreenPosNorm).xy + ( UnpackNormal( tex2D( _Noise, uv_TexCoord105 ) ).r * _Distortion * 0.25 * edgeBlend98 * i.vertexColor.a ) ));
			o.Emission = screenColor8.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18200
133;367;1409;886;1924.908;354.5405;1.813707;True;True
Node;AmplifyShaderEditor.CommentaryNode;112;-1208.751,660.4702;Inherit;False;1381.478;315.7155;Blend out the start and edges of the trail;7;98;97;87;88;80;114;115;Edge Blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;-1158.197,718.5361;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;113;-1905.242,-389.4117;Inherit;False;2734.35;839.1811;Comment;18;0;8;30;93;39;102;99;96;79;29;40;105;106;108;104;111;110;109;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;114;-906.8568,764.6192;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;109;-1872.05,-89.93896;Inherit;False;Property;_USpeed;U Speed;2;0;Create;True;0;0;False;0;False;0;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;88;-692.1489,765.0021;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;111;-1672.05,-84.93896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-539.8138,765.6957;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-1684.05,-10.939;Inherit;False;Property;_VSpeed;V Speed;3;0;Create;True;0;0;False;0;False;0;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;104;-1458.543,-84.64295;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;108;-1461.05,-5.938997;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;87;-385.744,765.356;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;106;-1239.543,-84.64295;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-213.1477,765.6715;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;98;-50.35512,760.5797;Inherit;False;edgeBlend;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;105;-1095.18,-130.6347;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GrabScreenPosition;40;-394.0278,-298.6098;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;79;-792.6123,31.53586;Inherit;False;Property;_Distortion;Distortion;1;0;Create;True;0;0;True;0;False;0;0.962;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-723.0407,111.2083;Inherit;False;Constant;_DistortionFactor;Distortion Factor;2;0;Create;True;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;-807.8062,-159.3714;Inherit;True;Property;_Noise;Noise;0;0;Create;True;0;0;True;0;False;-1;f7f322ea849ea7d41adb6fa8a7d8a3e6;dd2fd2df93418444c8e280f1d34deeb5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;102;-695.5435,264.3571;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;99;-713.3373,188.2795;Inherit;False;98;edgeBlend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;39;-144.2148,-299.0008;Inherit;True;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-357.2188,71.46494;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;154.3052,49.56962;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenColorNode;8;347.1941,45.95161;Float;False;Global;_ScreenGrab0;Screen Grab 0;-1;0;Create;True;0;0;False;0;False;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;579.1319,5.275653;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;NeoFPS/Standard/DistortionTrail (EdgeBlend);False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;False;0;False;Opaque;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;114;0;80;2
WireConnection;88;0;114;0
WireConnection;111;0;109;0
WireConnection;115;0;88;0
WireConnection;115;1;88;0
WireConnection;104;0;111;0
WireConnection;108;0;110;0
WireConnection;87;0;115;0
WireConnection;106;0;104;0
WireConnection;106;1;108;0
WireConnection;97;0;87;0
WireConnection;97;1;87;0
WireConnection;98;0;97;0
WireConnection;105;1;106;0
WireConnection;29;1;105;0
WireConnection;39;0;40;0
WireConnection;93;0;29;1
WireConnection;93;1;79;0
WireConnection;93;2;96;0
WireConnection;93;3;99;0
WireConnection;93;4;102;4
WireConnection;30;0;39;0
WireConnection;30;1;93;0
WireConnection;8;0;30;0
WireConnection;0;2;8;0
ASEEND*/
//CHKSM=8EBF5256138BE1BABD422088B9699609A0484973