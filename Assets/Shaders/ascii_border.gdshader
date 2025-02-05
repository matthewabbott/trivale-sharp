// Assets/Shaders/ascii_border.gdshader
shader_type canvas_item;

uniform vec4 border_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float border_thickness : hint_range(0.0, 0.1) = 0.02;
uniform float corner_size : hint_range(0.0, 0.5) = 0.1;
uniform float glow_intensity : hint_range(0.0, 1.0) = 0.3;

void fragment() {
    vec2 uv = UV;
    vec4 color = texture(TEXTURE, UV);
    
    // Create border
    float border = 0.0;
    
    // Main edges
    if (uv.x < border_thickness || uv.x > 1.0 - border_thickness ||
        uv.y < border_thickness || uv.y > 1.0 - border_thickness) {
        border = 1.0;
    }
    
    // Corners - create ASCII-like corner pieces
    float corner = corner_size;
    if ((uv.x < corner && uv.y < corner) || // Top left
        (uv.x > 1.0-corner && uv.y < corner) || // Top right
        (uv.x < corner && uv.y > 1.0-corner) || // Bottom left
        (uv.x > 1.0-corner && uv.y > 1.0-corner)) { // Bottom right
        
        // Create a stepped pattern for ASCII feel
        vec2 corner_uv = fract(uv * 10.0);
        border = step(0.5, corner_uv.x + corner_uv.y);
    }
    
    // Add glow
    float glow = border * glow_intensity;
    vec4 border_with_glow = border_color * (1.0 + glow);
    
    // Mix original color with border
    COLOR = mix(color, border_with_glow, border);
}