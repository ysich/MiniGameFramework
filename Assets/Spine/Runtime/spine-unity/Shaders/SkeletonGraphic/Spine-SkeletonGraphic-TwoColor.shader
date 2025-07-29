// This is a premultiply-alpha adaptation of the built-in Unity shader "UI/Default" in Unity 5.6.2 to allow Unity UI stencil masking.
//  带亮部颜色.  无Mask.  装扮 眉毛
Shader "Spine/SkeletonGraphic TwoColor"
{
	Properties
	{
		[PerRendererData] _MainTex ("主贴图", 2D) = "white" {}
		_Color ("主色调", Color) = (1,1,1,1)
		_TintColor2 ("亮部颜色", Color) = (1,1,1,1)
		_MainTexPolar("亮部强度(X)", Vector) = (1,1,1,1)

		[Space(40)]
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_CANVAS_GROUP_COMPATIBLE)] _CanvasGroupCompatible("CanvasGroup Compatible", Int) = 1

		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

		[HideInInspector] _ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("__src", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("__dst", Float) = 10
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend [_SrcBlend] [_DstBlend]
		ColorMask [_ColorMask]

		Pass
		{
			Name "Normal"

		CGPROGRAM
			#pragma multi_compile _ UNITY_UI_CLIP_RECT
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _ _CANVAS_GROUP_COMPATIBLE
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "../CGIncludes/Spine-Common.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct VertexInput {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput {
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifndef ENABLE_GRAYSCALE
			fixed4 _Color;
			#endif
			fixed4 _TextureSampleAdd;

			#ifdef UNITY_UI_CLIP_RECT
			float4 _ClipRect;
			#endif

			#ifdef ENABLE_FILL
			float4 _FillColor;
			float _FillPhase;
			#endif
			#ifdef ENABLE_GRAYSCALE
			float _GrayPhase;
			#endif

			fixed4 _TintColor2;
			fixed4 _MainTexPolar;

			sampler2D _MainTex;


			VertexOutput vert (VertexInput IN) {
				VertexOutput OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;

				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
				#endif

			#ifdef _CANVAS_GROUP_COMPATIBLE
				half4 vertexColor = IN.color;
				// CanvasGroup alpha sets vertex color alpha, but does not premultiply it to rgb components.
				vertexColor.rgb *= vertexColor.a;
				// Unfortunately we cannot perform the TargetToGamma and PMAGammaToTarget transformations,
				// as these would be wrong with modified alpha.
			#else
				// Note: CanvasRenderer performs a GammaToTargetSpace conversion on vertex color already,
				// however incorrectly assuming straight alpha color.
				// Saturated version used to prevent numerical issues of certain low-alpha values.
				float4 vertexColor = PMAGammaToTargetSpaceSaturated(half4(TargetToGammaSpace(IN.color.rgb), IN.color.a));
			#endif
				OUT.color = vertexColor;
			#ifndef ENABLE_GRAYSCALE
				OUT.color *= float4(_Color.rgb * _Color.a, _Color.a); // Combine a PMA version of _Color with vertexColor.
			#endif

				return OUT;
			}


			fixed4 frag (VertexOutput IN) : SV_Target
			{
				half4 texColor = tex2D(_MainTex, IN.texcoord);

				#if defined(_STRAIGHT_ALPHA_INPUT)
				texColor.rgb *= texColor.a;
				#endif

				// Mask计算
				texColor.rgb = texColor.rgb * _Color.rgb * _Color.a;
				half3 c1 = _TintColor2.rgb * _TintColor2.a * _MainTexPolar.x;
				texColor.rgb = texColor.rgb + c1;

				half4 color = (texColor + _TextureSampleAdd) * IN.color;
	
				#ifdef UNITY_UI_CLIP_RECT
				color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				#ifdef ENABLE_FILL
				color.rgb = lerp(color.rgb, (_FillColor.rgb * color.a), _FillPhase); // make sure to PMA _FillColor.
				#endif
				#ifdef ENABLE_GRAYSCALE
				color.rgb = lerp(color.rgb, dot(color.rgb, float3(0.3, 0.59, 0.11)), _GrayPhase);
				#endif
				return color;
			}
		ENDCG
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
