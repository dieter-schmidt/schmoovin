// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NeoFPS/Standard/LaserPointerBeam"
{
	Properties
	{
		_MaxIntensity("Max Intensity", Range( 0.1 , 2)) = 1.2
		_NoiseScale("Noise Scale", Range( 0.1 , 100)) = 10
		_NoiseMultiplier("Noise Multiplier", Range( 0 , 1)) = 1
		_NoiseOffset("Noise Offset", Range( -1 , 1)) = 0
		_Turbulence("Turbulence", Range( 0 , 1)) = 0.1

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
		ZTest LEqual
		Offset 0 , 0
		
		
		
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
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_POSITION


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
			uniform float _Turbulence;
			uniform float _NoiseScale;
			uniform float _NoiseMultiplier;
			uniform float _NoiseOffset;
			float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }
			float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }
			float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }
			float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }
			float snoise( float3 v )
			{
				const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
				float3 i = floor( v + dot( v, C.yyy ) );
				float3 x0 = v - i + dot( i, C.xxx );
				float3 g = step( x0.yzx, x0.xyz );
				float3 l = 1.0 - g;
				float3 i1 = min( g.xyz, l.zxy );
				float3 i2 = max( g.xyz, l.zxy );
				float3 x1 = x0 - i1 + C.xxx;
				float3 x2 = x0 - i2 + C.yyy;
				float3 x3 = x0 - 0.5;
				i = mod3D289( i);
				float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
				float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
				float4 x_ = floor( j / 7.0 );
				float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
				float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
				float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
				float4 h = 1.0 - abs( x ) - abs( y );
				float4 b0 = float4( x.xy, y.xy );
				float4 b1 = float4( x.zw, y.zw );
				float4 s0 = floor( b0 ) * 2.0 + 1.0;
				float4 s1 = floor( b1 ) * 2.0 + 1.0;
				float4 sh = -step( h, 0.0 );
				float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
				float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
				float3 g0 = float3( a0.xy, h.x );
				float3 g1 = float3( a0.zw, h.y );
				float3 g2 = float3( a1.xy, h.z );
				float3 g3 = float3( a1.zw, h.w );
				float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
				g0 *= norm.x;
				g1 *= norm.y;
				g2 *= norm.z;
				g3 *= norm.w;
				float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
				m = m* m;
				m = m* m;
				float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
				return 42.0 * dot( m, px);
			}
			

			
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
				float2 uv0127 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float falloff146 = ( 1.0 - abs( (uv0127.y*2.0 + -1.0) ) );
				float a142 = i.ase_color.a;
				float temp_output_154_0 = ( _MaxIntensity * falloff146 * a142 );
				float mulTime134 = _Time.y * _Turbulence;
				float3 appendResult136 = (float3(( mulTime134 * 0.4 ) , mulTime134 , ( mulTime134 * 0.35 )));
				float simplePerlin3D113 = snoise( ( WorldPosition + appendResult136 )*_NoiseScale );
				simplePerlin3D113 = simplePerlin3D113*0.5 + 0.5;
				float noiseAlpha64 = saturate( ( ( simplePerlin3D113 * _NoiseMultiplier ) + _NoiseOffset ) );
				float clampResult157 = clamp( ( ( temp_output_154_0 * temp_output_154_0 * noiseAlpha64 ) - 1.0 ) , 0.0 , 1.0 );
				float3 rgb147 = ( (i.ase_color).rgb + clampResult157 );
				float lerpResult165 = lerp( 8.0 , 2.0 , a142);
				float brightnessMultiplier170 = pow( ( falloff146 * noiseAlpha64 ) , lerpResult165 );
				
				
				finalColor = float4( ( rgb147 * brightnessMultiplier170 ) , 0.0 );
				return finalColor;
			}
			ENDCG
		}
	}
	
	
	
}
/*ASEBEGIN
Version=18200
113;566;1608;790;2333.732;2099.46;1.557614;True;True
Node;AmplifyShaderEditor.CommentaryNode;69;-1947.037,-715.798;Inherit;False;2492.571;605.8727;Comment;15;64;125;124;117;123;113;116;115;135;136;114;137;138;134;139;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-1911.975,-347.2093;Inherit;False;Property;_Turbulence;Turbulence;4;0;Create;True;0;0;False;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;134;-1603.603,-340.9587;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;-1350.83,-290.5655;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.35;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;137;-1352.531,-423.6732;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;114;-1444.08,-592.6815;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;136;-1110.63,-423.1288;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;63;-1944.437,-1147.844;Inherit;False;1571.668;311.7296;Comment;5;146;144;43;90;127;Edge Blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-911.2338,-389.2053;Inherit;False;Property;_NoiseScale;Noise Scale;1;0;Create;True;0;0;True;0;False;10;10;0.1;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;127;-1857.864,-1045.22;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;135;-896.4927,-591.9949;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;116;-594.6619,-478.1503;Inherit;False;Property;_NoiseMultiplier;Noise Multiplier;2;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;113;-608.2985,-598.2979;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;90;-1525.313,-998.9467;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-305.7843,-594.2941;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-587.7141,-387.2325;Inherit;False;Property;_NoiseOffset;Noise Offset;3;0;Create;True;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;43;-1288.7,-999.6477;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;162;-1936.136,-2060.852;Inherit;False;1876.375;774.2446;Comment;13;147;158;157;143;156;155;154;153;152;159;142;148;174;RGB Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;144;-903.6365,-999.1467;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;124;-106.7836,-546.2941;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;148;-1876.237,-1897.101;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;146;-676.7606,-1005.48;Inherit;False;falloff;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;142;-1599.906,-1809.221;Inherit;False;a;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;125;65.88432,-545.3531;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;153;-1870.214,-1655.966;Inherit;False;Property;_MaxIntensity;Max Intensity;0;0;Create;True;0;0;False;0;False;1.2;1.419656;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;251.437,-549.7433;Inherit;False;noiseAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;159;-1777.13,-1489.715;Inherit;False;142;a;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-1776.843,-1573.397;Inherit;False;146;falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;174;-1528.809,-1430.156;Inherit;False;64;noiseAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;-1473.039,-1567.791;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;155;-1291.942,-1584.661;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;163;-1927.154,-2768.306;Inherit;False;1305.509;586.2621;Comment;7;172;166;170;167;165;164;173;Brightness;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;164;-1710.756,-2326.957;Inherit;False;142;a;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;172;-1808.92,-2479.823;Inherit;False;64;noiseAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;166;-1820.995,-2690.364;Inherit;True;146;falloff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;156;-1121.274,-1585.189;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;157;-939.1974,-1585.959;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;173;-1526.471,-2599.478;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;143;-1565.884,-1900.805;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;165;-1478.692,-2368.482;Inherit;False;3;0;FLOAT;8;False;1;FLOAT;2;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;167;-1247.498,-2453.41;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;158;-663.7816,-1898.584;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;170;-945.4786,-2457.553;Inherit;False;brightnessMultiplier;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;140;-1935.841,48.57246;Inherit;False;1144.852;535.6718;;4;149;150;66;29;Output;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;-361.0112,-1902.728;Inherit;True;rgb;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-1836.931,268.6997;Inherit;False;170;brightnessMultiplier;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-1831.964,175.2019;Inherit;False;147;rgb;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;150;-1442.315,180.5867;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;149;-1042.849,177.9191;Float;False;True;-1;2;;100;1;NeoFPS/Standard/LaserPointerBeam;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;4;1;False;-1;1;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;2;RenderType=Overlay=RenderType;Queue=Transparent=Queue=0;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;134;0;139;0
WireConnection;138;0;134;0
WireConnection;137;0;134;0
WireConnection;136;0;137;0
WireConnection;136;1;134;0
WireConnection;136;2;138;0
WireConnection;135;0;114;0
WireConnection;135;1;136;0
WireConnection;113;0;135;0
WireConnection;113;1;115;0
WireConnection;90;0;127;2
WireConnection;123;0;113;0
WireConnection;123;1;116;0
WireConnection;43;0;90;0
WireConnection;144;0;43;0
WireConnection;124;0;123;0
WireConnection;124;1;117;0
WireConnection;146;0;144;0
WireConnection;142;0;148;4
WireConnection;125;0;124;0
WireConnection;64;0;125;0
WireConnection;154;0;153;0
WireConnection;154;1;152;0
WireConnection;154;2;159;0
WireConnection;155;0;154;0
WireConnection;155;1;154;0
WireConnection;155;2;174;0
WireConnection;156;0;155;0
WireConnection;157;0;156;0
WireConnection;173;0;166;0
WireConnection;173;1;172;0
WireConnection;143;0;148;0
WireConnection;165;2;164;0
WireConnection;167;0;173;0
WireConnection;167;1;165;0
WireConnection;158;0;143;0
WireConnection;158;1;157;0
WireConnection;170;0;167;0
WireConnection;147;0;158;0
WireConnection;150;0;29;0
WireConnection;150;1;66;0
WireConnection;149;0;150;0
ASEEND*/
//CHKSM=4794C3846AF84F904B7369C6464DC4C73031E24D