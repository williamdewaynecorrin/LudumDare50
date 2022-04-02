Shader "Custom/EmissionFlowDualColorDistortion"
{
    Properties
    {
		//[Header("Base/Textures")]
		//[Space]

        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainTexScrollX("Main Texture Scroll X", Range(-3,3)) = 0
		_MainTexScrollY("Main Texture Scroll Y", Range(-3,3)) = 0
		_Metallic("Metallic", Range(0,1)) = 0.5
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_NormalMap("NormalMap", 2D) = "bump" {}
		[Toggle] _UseNormalUVs("Use Separate Normal UVs?", Float) = 0

		_DistortionTex("Distortion Texture", 2D) = "black" {}
		_DistortionStrength("Distortion Strength", Range(0, 5)) = 1
		_DistortionNoise("Distortion Noise", Range(0, 5)) = 1
		_DistortionSpeed("Distortion Speed", Range(0, 5)) = 1

		_NormalStrength("NormalStrength", Float) = 1
		_NormalMapScrollX("Normal Map Scroll X", Range(-3,3)) = 0
		_NormalMapScrollY("Normal Map Scroll Y", Range(-3,3)) = 0
		_EmissionTex("EmissionTex (RGB)", 2D) = "white" {}
		_EmissionFlow("EmissionFlow (GS)", 2D) = "white" {}
		_EmissionTexScrollX("Emission Tex Scroll X", Range(-3,3)) = 0
		_EmissionTexScrollY("Emission Tex Scroll Y", Range(-3,3)) = 0

		//[Header("Dual Colors")]
		//[Space]

		[HDR] _EmissionColor("EmissionColor", Color) = (1,1,1,1)
		[HDR] _EmissionColor2("EmissionColor2", Color) = (1,1,1,1)
		[Toggle]_DualColorUniform("Dual Color Uniform", Float) = 0
		_DualColorSpeed("Dual Color Speed Speed", Range(0,20)) = 0.5
		_DualColorAmp("Dual Color Amp", Range(0,1)) = 0.2
		_DualColorOffset("Dual Color Offset", Range(0, 1)) = 0.7

		//[Header("Emission Values")]
		//[Space]

		[Toggle] _UseEmissionUVs("Use Separate Emission UVs?", Float) = 0
		_EmissionFlowSpeed("Emission Flow Speed", Range(0,20)) = 0.5
		_EmissionFlowThresh("Emission Flow Threshold", Range(0,1)) = 0.1
		_EmissionMaxFlowValue("Emission Max Flow Value", Range(0,1)) = 0.995
		_EmissionFlowCutoff("Emission Flow Cutoff", Range(0,1)) = 0.001
		_EmissionCutoff("Emission Cutoff", Range(0,1)) = 0.989

		[Toggle] _ContinuousAnimation("Continuous Animation?", Float) = 0
		_ContinuousAnimationAmp("Continuous Animation Amplitude", Range(0,1)) = 0.2
		_ContinuousAnimationOffset("Continuous Animation Offset", Range(0,1)) = 0.7
	}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		ZWrite On
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;

		sampler2D _EmissionTex;
		sampler2D _EmissionFlow;
		sampler2D _DistortionTex;

        struct Input
        {
			float2 uv_MainTex;
			float2 uv_EmissionTex;
			float2 uv_EmissionFlow;
			float2 uv_NormalMap;
		};

		half _Metallic;
		half _Glossiness;
		half _NormalStrength;
		fixed4 _Color;
		fixed4 _EmissionColor;
		fixed4 _EmissionColor2;
		half _EmissionFlowSpeed;
		half _EmissionFlowThresh;
		half _EmissionFlowCutoff;
		half _EmissionCutoff;
		half _EmissionMaxFlowValue;
		bool _UseEmissionUVs;
		bool _UseNormalUVs;

		bool _ContinuousAnimation;
		half _ContinuousAnimationAmp;
		half _ContinuousAnimationOffset;

		bool _DualColorUniform;
		half _DualColorSpeed;
		half _DualColorAmp;
		half _DualColorOffset;

		half _MainTexScrollX;
		half _MainTexScrollY;
		half _NormalMapScrollX;
		half _NormalMapScrollY;
		half _EmissionTexScrollX;
		half _EmissionTexScrollY;

		half _DistortionStrength; 
		half _DistortionNoise;
		half _DistortionSpeed;

		int continuousanimationdirection = 1;// 1 for +, -1 for -
		float currentflow = 0;
		float colorlerptime = 0;

		float invlerp(float from, float to, float value) 
		{
			return (value - from) / (to - from);
		}

		bool FlowPassesThresh(half grayscale)
		{
			// -- check bounds initially
			bool upperbounds = grayscale > currentflow - _EmissionFlowThresh;
			bool lowerbounds = grayscale < currentflow + _EmissionFlowThresh;

			bool overflow_lb = grayscale - _EmissionFlowThresh < 0;
			bool overflow_ub = grayscale + _EmissionFlowThresh > _EmissionMaxFlowValue;

			if (overflow_lb && overflow_ub)
				return true;

			if (overflow_lb)
			{
				grayscale += _EmissionMaxFlowValue;
				if(!upperbounds)
					upperbounds = grayscale - currentflow <= _EmissionFlowThresh && grayscale - currentflow >= 0;
			}
			else if (overflow_ub)
			{
				grayscale -= _EmissionMaxFlowValue;
				if (!lowerbounds)
					lowerbounds = grayscale - currentflow >= -_EmissionFlowThresh && grayscale - currentflow <= 0;
			}

			return upperbounds && lowerbounds;
		}

		float3 FlowUVW(float2 uv, float2 flowvector, float time) 
		{
			float progress = frac(time);

			float3 uvw = float3(0, 0, 1);
			uvw.xy = uv - flowvector * progress;
			return uvw;
		}

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// -- scroll
			float2 maintexscroll = float2(_MainTexScrollX, _MainTexScrollY) * _Time.y;
			float2 normalmapscroll = float2(_NormalMapScrollX, _NormalMapScrollY) * _Time.y;
			float2 emissiontexscroll = float2(_EmissionTexScrollX, _EmissionTexScrollY) * _Time.y;

			fixed4 distortion = tex2D(_DistortionTex, IN.uv_MainTex);
			float2 flowvector = (distortion.rg * 2 - 1) * _DistortionStrength;
			float noise = distortion.a * _DistortionNoise;
			float distortiontime = _Time.y * _DistortionSpeed + noise;

			//maintexscroll = FlowUV(maintexscroll, flowvector, _Time.y);
			//normalmapscroll = FlowUV(normalmapscroll, flowvector, _Time.y);
			//emissiontexscroll = FlowUV(emissiontexscroll, flowvector, _Time.y);
			float3 mainuvw = FlowUVW(maintexscroll, flowvector, distortiontime);
			float3 normuvw = FlowUVW(normalmapscroll, flowvector, distortiontime);
			float3 emiuvw = FlowUVW(emissiontexscroll, flowvector, distortiontime);

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex + maintexscroll + mainuvw.xy) * _Color * mainuvw.z;
			o.Albedo = c.rgb;

			float2 normuv = (_UseNormalUVs == 0) ? IN.uv_MainTex : IN.uv_NormalMap;
			o.Normal = UnpackScaleNormal(tex2D(_NormalMap, normuv.xy + normalmapscroll) * normuvw.z, _NormalStrength);

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			fixed4 emi = tex2D(_EmissionTex, (_UseEmissionUVs == 0 ? IN.uv_MainTex : IN.uv_EmissionTex) + emiuvw.xy + emissiontexscroll) * emiuvw.z;
			fixed3 emicol = fixed3(0, 0, 0);

			// -- advance current flow differently depending on animation type
			if (_ContinuousAnimation)
			{
				currentflow = _ContinuousAnimationOffset + sin(_Time.y * _EmissionFlowSpeed) * _ContinuousAnimationAmp;
			}
			else
				currentflow = (_Time.y * _EmissionFlowSpeed) % _EmissionMaxFlowValue;

			// -- check current flow pixel value and match with shader flow value
			fixed4 emiflow = tex2D(_EmissionFlow, (_UseEmissionUVs == 0 ? IN.uv_MainTex : IN.uv_EmissionFlow));
			fixed flowgrayscale = emiflow.r;

			bool nocol = false;
			// -- only allow emission value through if it meets the current threshold
			if (emi.r <= _EmissionCutoff && emi.g <= _EmissionCutoff && emi.g <= _EmissionCutoff)
			{
				emicol.rgb = 0;
			}
			else if (FlowPassesThresh(flowgrayscale) && flowgrayscale > _EmissionFlowCutoff)
			{
				emicol = emi.rgb;
			}
			else
				nocol = true;

			// -- dual colors
			fixed4 emipropcol = fixed4(1, 1, 1, 1);
			colorlerptime = _DualColorOffset + sin(_Time.y * _DualColorSpeed) * _DualColorAmp;
			if (_DualColorUniform)
			{
				float emilerp = invlerp(0, _DistortionNoise, noise);
				emipropcol = lerp(_EmissionColor, _EmissionColor2, emilerp);
			}
			else
			{
				if (nocol)
				{
					emicol = fixed3(1, 1, 1);

					float lerpcol = clamp(invlerp(0, 1, colorlerptime), 0, 1);
					emipropcol = lerp(_EmissionColor, _EmissionColor2, lerpcol);
				}
				else
				{
					emipropcol = _EmissionColor;
				}
			}

			o.Emission = emicol * emipropcol;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
