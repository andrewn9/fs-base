shader_type spatial;

uniform sampler2D albedo_texture : source_color;
uniform float THRESHOLD = 0.5;
instance uniform bool Dither = true;

varying vec3 world_vertex;

render_mode cull_back, depth_draw_opaque;

void vertex() {
    world_vertex = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec4 tex_color = texture(albedo_texture, UV);

    // Base color from texture
    ALBEDO = tex_color.rgb;

	// Only apply dithering if the object is visible to a specific camera
	// Example: layer 1 = main camera, layer 2 = HUD/sub-viewport
	if ((CAMERA_VISIBLE_LAYERS & uint(1)) != 0u && Dither) {
	    const mat4 bayer4x4 = mat4(
	        vec4(-0.5,   0.0,  -0.375,  0.125),
	        vec4(0.25, -0.25,  0.375, -0.125),
	        vec4(-0.3125, 0.1875, -0.4375, 0.0625),
	        vec4(0.4375, -0.0625, 0.3125, -0.1875)
	    );

	    float alpha = COLOR.r; // vertex color masking
	    vec2 screen_coords = FRAGCOORD.xy;

	    int x = int(mod(screen_coords.x, 4.0));
	    int y = int(mod(screen_coords.y, 4.0));
	    alpha += bayer4x4[x][y];

	    if (alpha < THRESHOLD) {
	        discard; // removes masked fragments
	    }
	}
}
