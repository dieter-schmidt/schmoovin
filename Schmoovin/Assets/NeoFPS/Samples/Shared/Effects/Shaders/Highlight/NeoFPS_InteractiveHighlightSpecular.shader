// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/InteractiveHighlightSpecular"
{
	Properties
	{
		_Color("Color", Color) = (0.9044118,0.6640914,0.03325041,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Emission("Emission", 2D) = "black" {}
		_Occlusion("Occlusion", 2D) = "white" {}
		_Specular("Specular", 2D) = "white" {}
		_SpecularLevel("Specular Level", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_HighlightColor("Highlight Color", Color) = (0.7065311,0.9705882,0.9596617,1)
		_HighlightSpeed("Highlight Speed", Range( 0.1 , 2)) = 1
		[PerRendererData]_Highlight("Highlight", Range( 0 , 1)) = 0
		_BodyMaxHighlight("Body Max Highlight", Range( 0 , 1)) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _Normal;
		uniform float4 _Color;
		uniform sampler2D _Albedo;
		uniform float _HighlightSpeed;
		uniform float _BodyMaxHighlight;
		uniform float4 _HighlightColor;
		uniform float _Highlight;
		uniform sampler2D _Emission;
		uniform sampler2D _Specular;
		uniform float _SpecularLevel;
		uniform float _Smoothness;
		uniform sampler2D _Occlusion;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float3 Normal67 = UnpackNormal( tex2D( _Normal, i.uv_texcoord ) );
			o.Normal = Normal67;
			float4 Albedo65 = ( _Color * tex2D( _Albedo, i.uv_texcoord ) );
			o.Albedo = Albedo65.rgb;
			float mulTime138 = _Time.y * ( _HighlightSpeed * UNITY_PI );
			float scaledTime202 = mulTime138;
			float3 ase_worldPos = i.worldPos;
			float3 break199 = ( ase_worldPos * float3( 0.4,1,0.5 ) );
			float bodyHighlight206 = (0.0 + (cos( ( ( scaledTime202 * 0.66 ) + break199.x + break199.y + break199.z ) ) - -1.0) * (_BodyMaxHighlight - 0.0) / (1.0 - -1.0));
			float4 Emision75 = tex2D( _Emission, i.uv_texcoord );
			float4 Final_Emision114 = ( ( saturate( bodyHighlight206 ) * _HighlightColor * _Highlight ) + Emision75 );
			o.Emission = Final_Emision114.rgb;
			float4 tex2DNode139 = tex2D( _Specular, i.uv_texcoord );
			float3 Specular140 = ( (tex2DNode139).rgb * _SpecularLevel );
			o.Specular = Specular140;
			float Smoothness155 = ( tex2DNode139.a * _Smoothness );
			o.Smoothness = Smoothness155;
			float4 Occlusion86 = tex2D( _Occlusion, i.uv_texcoord );
			o.Occlusion = Occlusion86.r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18200
90;504;1409;874;3503.269;-260.6053;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;134;-1809.323,-657.2075;Inherit;False;881.8505;216.1514;Comment;5;202;138;176;128;178;Highlight Pulse;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-1774.942,-594.4434;Float;False;Property;_HighlightSpeed;Highlight Speed;9;0;Create;True;0;0;False;0;False;1;0.75;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;178;-1680.982,-519.8587;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-1486.594,-589.3;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;209;-1814.213,-260.5081;Inherit;False;1515.112;444.054;Comment;10;206;201;208;188;190;204;199;203;193;184;Body Highlight;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;138;-1333.993,-589.3563;Inherit;False;1;0;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;202;-1144.477,-594.1838;Inherit;False;scaledTime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;184;-1748.739,-50.759;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;193;-1549.633,-50.64425;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0.4,1,0.5;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;-1514.442,-175.4838;Inherit;False;202;scaledTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;204;-1268.842,-170.1838;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.66;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;199;-1394.812,-50.46581;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;190;-1085.949,-170.3488;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;188;-942.9491,-171.1487;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;208;-1121.552,67.75393;Inherit;False;Property;_BodyMaxHighlight;Body Max Highlight;11;0;Create;True;0;0;True;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;137;-3205.043,-1307.715;Inherit;False;1241.494;1493.419;Comment;20;65;140;86;64;153;85;139;154;52;63;75;74;67;58;51;143;155;156;159;181;Textures;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;201;-768.341,-19.51733;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;173;-3102.972,311.9254;Inherit;False;1139.579;534.1803;Comment;8;114;116;83;113;192;55;214;207;Emission Mix;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;206;-518.4187,-24.23312;Inherit;False;bodyHighlight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;51;-3155.043,-708.014;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;207;-3046.418,420.4713;Inherit;False;206;bodyHighlight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;74;-2730.626,-673.1343;Inherit;True;Property;_Emission;Emission;3;0;Create;True;0;0;False;0;False;-1;7a170cdb7cc88024cb628cfcdbb6705c;7a170cdb7cc88024cb628cfcdbb6705c;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;139;-2933.278,-230.8771;Inherit;True;Property;_Specular;Specular;5;0;Create;True;0;0;False;0;False;-1;None;0688235f1fee46b4581bcc1cf189cf3a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;192;-2709.559,424.8015;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;214;-3071.941,718.2048;Inherit;False;Property;_Highlight;Highlight;10;1;[PerRendererData];Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;55;-3073.038,539.0416;Float;False;Property;_HighlightColor;Highlight Color;8;0;Create;True;0;0;False;0;False;0.7065311,0.9705882,0.9596617,1;0.7065311,0.9705881,0.9596617,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;75;-2416.242,-673.1136;Float;False;Emision;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;159;-2582.691,-230.7932;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-2915.935,42.25915;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;True;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-2534.29,423.9981;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-2604.859,717.8856;Inherit;False;75;Emision;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;154;-2914.05,-36.44231;Inherit;False;Property;_SpecularLevel;Specular Level;6;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;52;-2733.341,-1061.514;Inherit;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;False;-1;7130c16fd8005b546b111d341310a9a4;84508b93f15f2b64386ec07486afc7a3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;63;-2686.046,-1238.289;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;False;0.9044118,0.6640914,0.03325041,0;0.992647,0.7937149,0.6131055,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;58;-2732.544,-869.0231;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;False;0;False;-1;11f03d9db1a617e40b7ece71f0a84f6f;11f03d9db1a617e40b7ece71f0a84f6f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;116;-2347.655,423.9622;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;85;-2730.141,-476.4239;Inherit;True;Property;_Occlusion;Occlusion;4;0;Create;True;0;0;True;0;False;-1;a8de9c9c15d9c7e4eaa883c727391bee;a8de9c9c15d9c7e4eaa883c727391bee;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-2399.541,-1153.815;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;153;-2367.253,-225.939;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-2366.582,25.42588;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;65;-2238.84,-1158.914;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;155;-2195.081,20.35143;Inherit;False;Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;114;-2184.479,417.9616;Float;False;Final_Emision;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;140;-2197.2,-231.4736;Inherit;False;Specular;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;158;-686.5558,-1080.812;Inherit;False;622;622;;7;0;76;68;70;157;142;69;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;67;-2359.621,-869.0596;Float;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;86;-2416.141,-476.3118;Float;False;Occlusion;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-611.8044,-550.3024;Inherit;False;86;Occlusion;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;142;-608.3095,-733.5464;Inherit;False;140;Specular;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-611.351,-909.6763;Inherit;False;67;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;157;-630.5558,-639.8127;Inherit;False;155;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;181;-2413.41,-87.91034;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-612.462,-992.2867;Inherit;False;65;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-642.4898,-821.4208;Inherit;False;114;Final_Emision;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-309.1619,-898.3948;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;NeoFPS/Standard/InteractiveHighlightSpecular;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;176;0;128;0
WireConnection;176;1;178;0
WireConnection;138;0;176;0
WireConnection;202;0;138;0
WireConnection;193;0;184;0
WireConnection;204;0;203;0
WireConnection;199;0;193;0
WireConnection;190;0;204;0
WireConnection;190;1;199;0
WireConnection;190;2;199;1
WireConnection;190;3;199;2
WireConnection;188;0;190;0
WireConnection;201;0;188;0
WireConnection;201;4;208;0
WireConnection;206;0;201;0
WireConnection;74;1;51;0
WireConnection;139;1;51;0
WireConnection;192;0;207;0
WireConnection;75;0;74;0
WireConnection;159;0;139;0
WireConnection;83;0;192;0
WireConnection;83;1;55;0
WireConnection;83;2;214;0
WireConnection;52;1;51;0
WireConnection;58;1;51;0
WireConnection;116;0;83;0
WireConnection;116;1;113;0
WireConnection;85;1;51;0
WireConnection;64;0;63;0
WireConnection;64;1;52;0
WireConnection;153;0;159;0
WireConnection;153;1;154;0
WireConnection;156;0;139;4
WireConnection;156;1;143;0
WireConnection;65;0;64;0
WireConnection;155;0;156;0
WireConnection;114;0;116;0
WireConnection;140;0;153;0
WireConnection;67;0;58;0
WireConnection;86;0;85;0
WireConnection;0;0;68;0
WireConnection;0;1;69;0
WireConnection;0;2;70;0
WireConnection;0;3;142;0
WireConnection;0;4;157;0
WireConnection;0;5;76;0
ASEEND*/
//CHKSM=FF69A93EE7015C08FA0365B94EFA0030A1E7BA71