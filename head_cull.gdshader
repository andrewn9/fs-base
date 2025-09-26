shader_type spatial;

uniform sampler2D albedo_texture : source_color;
uniform vec3 CAMERA_POSITION;
uniform float FADE_DISTANCE = 2.0; // Distance within which to start fading

varying vec3 world_vertex;

void vertex() {
    world_vertex = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec4 tex_color = texture(albedo_texture, UV);
    
    float camDist = distance(CAMERA_POSITION, world_vertex);
    
    // Fade when close to the camera
    float alpha = smoothstep(0.0, FADE_DISTANCE, camDist);
    
    ALBEDO = tex_color.rgb;
    ALPHA = tex_color.a * alpha;

    if (ALPHA <= 0.0) {
        discard;
    }
}
