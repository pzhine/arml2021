// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:True,rprd:True,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:2865,x:33990,y:32383,varname:node_2865,prsc:2|custl-4546-OUT,alpha-4666-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:1515,x:30858,y:32763,varname:node_1515,prsc:2;n:type:ShaderForge.SFN_Append,id:9961,x:31074,y:32763,varname:node_9961,prsc:2|A-1515-XYZ,B-1515-W;n:type:ShaderForge.SFN_Matrix4x4Property,id:5596,x:31074,y:32527,ptovrint:False,ptlb:_Camera2World,ptin:_Camera2World,varname:node_5596,prsc:0,glob:True,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,m00:1,m01:0,m02:0,m03:0,m10:0,m11:1,m12:0,m13:0,m20:0,m21:0,m22:1,m23:0,m30:0,m31:0,m32:0,m33:1;n:type:ShaderForge.SFN_MultiplyMatrix,id:5815,x:31302,y:32659,varname:node_5815,prsc:2|A-5596-OUT,B-9961-OUT;n:type:ShaderForge.SFN_LightPosition,id:2236,x:31341,y:32426,varname:node_2236,prsc:2;n:type:ShaderForge.SFN_Distance,id:7732,x:31625,y:32549,varname:node_7732,prsc:2|A-2236-XYZ,B-5815-OUT;n:type:ShaderForge.SFN_Slider,id:2505,x:31496,y:32759,ptovrint:False,ptlb:LightFalloff,ptin:_LightFalloff,varname:node_2505,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:3,max:10;n:type:ShaderForge.SFN_Power,id:5602,x:31894,y:32552,varname:node_5602,prsc:2|VAL-7732-OUT,EXP-2505-OUT;n:type:ShaderForge.SFN_Subtract,id:5506,x:32156,y:32479,varname:node_5506,prsc:2|A-9013-OUT,B-5602-OUT;n:type:ShaderForge.SFN_Slider,id:9013,x:31835,y:32420,ptovrint:False,ptlb:LightRange,ptin:_LightRange,varname:node_9013,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:10;n:type:ShaderForge.SFN_Max,id:690,x:32369,y:32479,varname:node_690,prsc:2|A-5506-OUT,B-98-OUT;n:type:ShaderForge.SFN_ValueProperty,id:98,x:32188,y:32689,ptovrint:False,ptlb:node_98,ptin:_node_98,varname:node_98,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:8559,x:32632,y:32481,varname:node_8559,prsc:2|IN-690-OUT,IMIN-98-OUT,IMAX-9013-OUT,OMIN-98-OUT,OMAX-1029-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1029,x:32454,y:32679,ptovrint:False,ptlb:node_1029,ptin:_node_1029,varname:node_1029,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:4565,x:32981,y:31954,varname:node_4565,prsc:2|A-2898-RGB,B-5919-RGB,C-9746-RGB,D-8559-OUT;n:type:ShaderForge.SFN_LightColor,id:9746,x:32645,y:32189,varname:node_9746,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:2898,x:32651,y:32658,ptovrint:False,ptlb:node_2898,ptin:_node_2898,varname:node_2898,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_VertexColor,id:5919,x:32680,y:32914,varname:node_5919,prsc:2;n:type:ShaderForge.SFN_Min,id:241,x:33330,y:32444,varname:node_241,prsc:2|A-3496-OUT,B-2894-OUT;n:type:ShaderForge.SFN_Slider,id:2894,x:32942,y:32721,ptovrint:False,ptlb:LightClamp,ptin:_LightClamp,varname:node_2894,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3,max:1;n:type:ShaderForge.SFN_Multiply,id:4666,x:33112,y:32815,varname:node_4666,prsc:2|A-2898-A,B-5919-A;n:type:ShaderForge.SFN_Lerp,id:4546,x:33721,y:32545,varname:node_4546,prsc:2|A-5919-RGB,B-241-OUT,T-5977-OUT;n:type:ShaderForge.SFN_Color,id:1307,x:33544,y:32311,ptovrint:False,ptlb:node_1307,ptin:_node_1307,varname:node_1307,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.7379308,c2:0,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:3496,x:32975,y:32321,varname:node_3496,prsc:2|A-9746-RGB,B-8559-OUT;n:type:ShaderForge.SFN_Ceil,id:6533,x:33465,y:32634,varname:node_6533,prsc:2|IN-8559-OUT;n:type:ShaderForge.SFN_Max,id:5977,x:33460,y:32836,varname:node_5977,prsc:2|A-2894-OUT,B-8559-OUT;proporder:2505-9013-98-1029-2898-2894-1307;pass:END;sub:END;*/

Shader "Shader Forge/LitParticle" {
    Properties {
        _LightFalloff ("LightFalloff", Range(0, 10)) = 3
        _LightRange ("LightRange", Range(0, 10)) = 1
        _node_98 ("node_98", Float ) = 0
        _node_1029 ("node_1029", Float ) = 1
        _node_2898 ("node_2898", 2D) = "white" {}
        _LightClamp ("LightClamp", Range(0, 1)) = 0.3
        _node_1307 ("node_1307", Color) = (0.7379308,0,1,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform fixed4x4 _Camera2World;
            uniform float _LightFalloff;
            uniform float _LightRange;
            uniform float _node_98;
            uniform float _node_1029;
            uniform sampler2D _node_2898; uniform float4 _node_2898_ST;
            uniform float _LightClamp;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float node_8559 = (_node_98 + ( (max((_LightRange-pow(distance(_WorldSpaceLightPos0.rgb,mul(_Camera2World,float4(i.posWorld.rgb,i.posWorld.a))),_LightFalloff)),_node_98) - _node_98) * (_node_1029 - _node_98) ) / (_LightRange - _node_98));
                float3 finalColor = lerp(i.vertexColor.rgb,min((_LightColor0.rgb*node_8559),_LightClamp),max(_LightClamp,node_8559));
                float4 _node_2898_var = tex2D(_node_2898,TRANSFORM_TEX(i.uv0, _node_2898));
                return fixed4(finalColor,(_node_2898_var.a*i.vertexColor.a));
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform fixed4x4 _Camera2World;
            uniform float _LightFalloff;
            uniform float _LightRange;
            uniform float _node_98;
            uniform float _node_1029;
            uniform sampler2D _node_2898; uniform float4 _node_2898_ST;
            uniform float _LightClamp;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(2,3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float node_8559 = (_node_98 + ( (max((_LightRange-pow(distance(_WorldSpaceLightPos0.rgb,mul(_Camera2World,float4(i.posWorld.rgb,i.posWorld.a))),_LightFalloff)),_node_98) - _node_98) * (_node_1029 - _node_98) ) / (_LightRange - _node_98));
                float3 finalColor = lerp(i.vertexColor.rgb,min((_LightColor0.rgb*node_8559),_LightClamp),max(_LightClamp,node_8559));
                float4 _node_2898_var = tex2D(_node_2898,TRANSFORM_TEX(i.uv0, _node_2898));
                return fixed4(finalColor * (_node_2898_var.a*i.vertexColor.a),0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
