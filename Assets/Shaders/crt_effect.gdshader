// Assets/Shaders/crt_effect.gdshader
shader_type canvas_item;

uniform float scan_line_count : hint_range(0, 1080) = 50.0;
uniform float scan_line_opacity : hint_range(0, 1) = 0.4;
uniform vec4 base_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);  // Green
uniform float brightness : hint_range(0, 2) = 1.2;
uniform float flicker_intensity : hint_range(0, 1) = 0.05;

float random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898,78.233))) * 43758.5453123);
}

void fragment() {
    // Basic scan lines
    float scan_line = sin(UV.y * scan_line_count * PI * 2.0) * 0.5 + 0.5;
    scan_line = 1.0 - (scan_line * scan_line_opacity);
    
    // Screen flicker
    float flicker = random(vec2(TIME * 0.1, 0.0)) * flicker_intensity;
    
    // Vignette effect
    vec2 vignette_uv = UV * 2.0 - 1.0;
    float vignette = 1.0 - dot(vignette_uv, vignette_uv) * 0.3;
    
    // Get screen color and apply effects
    vec4 color = textureLod(TEXTURE, UV, 0.0);
    color *= base_color;
    color *= scan_line;
    color *= (1.0 - flicker);
    color *= vignette;
    color *= brightness;
    
    COLOR = color;
}