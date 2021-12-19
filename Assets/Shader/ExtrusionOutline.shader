Shader "Custom/ExtrusionOutline" 
{
	Properties 
	{
		_Color ("Outline Color", Color) = (0,0,0,1)
		_Width ("Outline width", Range (0.0, 0.5)) = 0.1
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		Cull Front
        ZWrite On
        ColorMask RGB
        Blend SrcAlpha OneMinusSrcAlpha
		
		Pass 
		{
			Name "OUTLINE"
			
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
            CBUFFER_START(UnityPerMaterial)
            float _Width;
            float4 _Color;
            CBUFFER_END
			
            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
        
            struct v2f 
            {
                float4 pos : SV_POSITION;
                half fogCoord : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            v2f vert(appdata input) 
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
	
                input.vertex.xyz += input.normal.xyz * _Width;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
                output.pos = vertexInput.positionCS; 
                
                output.color = _Color;
                output.fogCoord = ComputeFogFactor(output.pos.z);
                return output;
            }
			
			half4 frag(v2f i) : SV_Target
			{
				i.color.rgb = MixFog(i.color.rgb, i.fogCoord);
				return i.color;
			}
            ENDHLSL
		}
	}
}
