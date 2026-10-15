// ==================================================
// File Name   : ToonShader_7.shader
// Created     : 2026/10/14
// Author      : 深野大輝
// Description : URP Forward+対応の3段階トゥーンシェーダー。
//               法線マップ、アルファクリップ、発光、アウトラインに対応。
// ==================================================

Shader "Custom/ToonShader_7"
{
    // ==================================================
    // Inspector設定項目
    // ==================================================
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        _ShadowColor("Shadow Color", Color) = (0.3, 0.3, 0.4, 1)
        _MidColor("Mid Color", Color) = (0.7, 0.7, 0.7, 1)
        _LightColor("Light Color", Color) = (1, 1, 1, 1)

        _ShadowThreshold("Shadow Threshold (0.0 - 1.0)", Range(0, 1)) = 0.3
        _LightThreshold("Light Threshold (0.0 - 1.0)", Range(0, 1)) = 0.6

        _SmoothWidth("Smooth Width (0.0 - 0.3)", Range(0, 0.3)) = 0.05
        _ShadowStrength("Shadow Strength (0.0 - 2.0)", Range(0, 2)) = 1.0
        _MidStrength("Mid Strength (0.0 - 2.0)", Range(0, 2)) = 1.0

        _LightStrength("Light Strength (0.0 - 2.0)", Range(0, 2)) = 1.0
        _LightColorStrength("Light Color Influence (0.0 - 2.0)", Range(0, 2)) = 1.0
        _LuminanceStrength("Luminance Influence (0.0 - 2.0)", Range(0, 2)) = 1.0

        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.2)) = 0.05

        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0, 2)) = 1.0

        [Toggle(_ALPHATEST_ON)]
        _AlphaClip("Alpha Clip", Float) = 0        
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5

        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionMap("Emission Map", 2D) = "white" {}
        _EmissionStrength("Emission Strength", Range(0, 5)) = 1
    }

    // ==================================================
    // URP描画設定
    // ==================================================
    SubShader
    {
        // Universal Render Pipeline用
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
    
        Cull Back
        ZWrite On
        ZTest LEqual

        // ==================================================
        // トゥーン描画Pass
        // ==================================================
        // ==================================================
        // 共通HLSL定義（すべてのPassで共有）
        // ==================================================
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half4 _MidColor;
                half4 _LightColor;
            
                float4 _BaseMap_ST;
                float _BumpScale;

                float _Cutoff;

                half4 _EmissionColor;
                float _EmissionStrength;

                float _ShadowThreshold;
                float _LightThreshold;

                float _SmoothWidth;
                float _ShadowStrength;
                float _MidStrength;
                float _LightStrength;
                float _LightColorStrength;
                float _LuminanceStrength;

                float4 _OutlineColor;
                float _OutlineWidth;

            CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }

            // 頂点・フラグメントシェーダー
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // メッシュ入力
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            // フラグメントシェーダーへの入力
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
            };
            
            // 追加ライトの情報を格納する構造体
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            // 法線マップのテクスチャとサンプラーを定義
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            // 発光マップのテクスチャとサンプラーを定義
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);



            // Vertex Shader
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                // オブジェクト空間の法線をワールド空間へ
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 tangentWS =    TransformObjectToWorldDir(input.tangentOS.xyz);
                float tangentSign =    input.tangentOS.w * GetOddNegativeScale();
                output.tangentWS = tangentWS;
                output.bitangentWS =    cross(output.normalWS, tangentWS) * tangentSign;

                // ワールド座標
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

                return output;
            }

            // Fragment Shader(Pixel Shader)
            half4 Frag(Varyings input) : SV_Target
            {
                //==================================================
                // ベースカラー
                //==================================================
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                #ifdef _ALPHATEST_ON
                    clip(baseColor.a - _Cutoff);
                #endif

                // ================================================
                // Normal Map(法線)
                // ================================================
                
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);                   
                
                float3 tangentWS = normalize(input.tangentWS);
                float3 bitangentWS = normalize(input.bitangentWS);
                float3 vertexNormalWS = normalize(input.normalWS);
                
                float3x3 tangentToWorld = float3x3(tangentWS, bitangentWS, vertexNormalWS);
                
                float3 normalWS = normalize(TransformTangentToWorld(normalTS, tangentToWorld));

                //==================================================
                //  全ライトの中で最大の光量を取得
                //==================================================

            
                //==================================================
                // メインライト
                //==================================================
                Light mainLight = GetMainLight();

                float3 mainLightDir = normalize(mainLight.direction);       // メインライトの方向を正規化
                float mainNdotL = saturate(dot(normalWS, mainLightDir));    // 法線とライトの方向の内積を計算して、0～1に収める
                float lightLuminance = dot(mainLight.color.rgb, float3(0.2126, 0.7152, 0.0722));    // メインライトの輝度を計算
                float mainLighting = mainNdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation * lightLuminance;


                
               //==================================================
               // 追加ライト
               //==================================================
               InputData inputData = (InputData)0;
               
               inputData.positionWS = input.positionWS;
               inputData.normalWS = normalWS;
               inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
               inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionHCS);
               float3 strongestLightColor = mainLight.color.rgb;    // 最も強い光の色をmainLightで初期化
               float strongestLighting = mainLighting;              // 最も強い光の光量をmainLightで初期化
               
               uint pixelLightCount = GetAdditionalLightsCount();
               
               LIGHT_LOOP_BEGIN(pixelLightCount)
               
                   Light light = GetAdditionalLight(lightIndex, input.positionWS); // 追加ライトの情報を取得               
                   float3 lightDir = normalize(light.direction);    // 追加ライトの方向を正規化               
                   float ndotL = saturate(dot(normalWS, lightDir)); // 法線とライトの方向の内積を計算して、0～1に収める               
                   float3 additionalLightColor = light.color.rgb;   // 追加ライトの色を取得
                   float additionalLuminance = dot(additionalLightColor, float3(0.2126, 0.7152, 0.0722));   // 追加ライトの輝度を計算

                   // 追加ライトの光量を計算
                   float additionalLighting = ndotL * light.distanceAttenuation * light.shadowAttenuation * additionalLuminance;
               
                   // 追加ライトの光量が最も強い光の光量より大きい場合、最も強い光の色と光量を更新
                   if(additionalLighting > strongestLighting)
                   {
                       strongestLighting = additionalLighting;
                       strongestLightColor = additionalLightColor;
                   }

               LIGHT_LOOP_END

                   strongestLighting *= _LuminanceStrength; // 輝度の影響を調整
                   strongestLightColor = lerp(float3(1,1,1), strongestLightColor, _LightColorStrength); // 光の色の影響を調整

               //==================================================
               // 光量を0～1に収める
               //==================================================
               float lighting = saturate(strongestLighting);

               //==================================================
               // 3段階の色
               //==================================================
               half3 shadowColor = baseColor.rgb * _ShadowColor.rgb * strongestLightColor * _ShadowStrength;
               half3 midColor    = baseColor.rgb * _MidColor.rgb    * strongestLightColor * _MidStrength;
               half3 lightColor  = baseColor.rgb * _LightColor.rgb  * strongestLightColor * _LightStrength;
               
               //==================================================
               // 境界を滑らかにするための補間値を計算
               //==================================================
               float shadowToMid = smoothstep(_ShadowThreshold - _SmoothWidth, _ShadowThreshold + _SmoothWidth, lighting);
               float midToLight  = smoothstep(_LightThreshold  - _SmoothWidth, _LightThreshold  + _SmoothWidth, lighting);

               //==================================================
               // 最終的な光量を3段階に分ける
               //==================================================
               half3 finalColor =
                   shadowColor * (1.0 - shadowToMid) +
                   midColor    * (shadowToMid * (1.0 - midToLight)) +
                   lightColor  * midToLight;


               //==================================================
               // 発光の計算
               //==================================================
               half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb;
               emission *= _EmissionColor.rgb * _EmissionStrength;             
               finalColor += emission;


               return half4(finalColor, baseColor.a);
            }

            ENDHLSL
        }

        // ==================================================
        // アウトライン描画
        // ==================================================
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "ToonOutline" }
        
            Cull Front
            ZWrite Off
            ZTest LEqual
        
            HLSLPROGRAM
            #pragma vertex VertOutline
            #pragma fragment FragOutline
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
        
            Varyings VertOutline(Attributes input)
            {
                Varyings output;
            
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);            
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
            
                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
            
                float maxScale = max(scale.x, max(scale.y, scale.z));
            
                positionWS += normalWS * _OutlineWidth * maxScale;
            
                output.positionHCS = TransformWorldToHClip(positionWS);
            
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            
                return output;
            }
        
            half4 FragOutline(Varyings input) : SV_Target
            {
                #ifdef _ALPHATEST_ON
            
                    half4 baseMap =
                        SAMPLE_TEXTURE2D(
                            _BaseMap,
                            sampler_BaseMap,
                            input.uv
                        );
            
                    half alpha = baseMap.a * _BaseColor.a;
            
                    clip(alpha - _Cutoff);
            
                #endif
            
                return _OutlineColor;
            }        

            ENDHLSL
        }
    }
}

