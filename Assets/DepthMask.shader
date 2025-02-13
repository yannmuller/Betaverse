Shader "Custom/DepthMask" {
    SubShader {
        Tags { "Queue"="Geometry-0" } // Render first.
        Pass {
            ZWrite On
            ColorMask 0  // Don’t write color.
        }
    }
}
