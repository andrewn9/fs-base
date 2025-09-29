shader_type spatial;

uniform sampler2D albedo_texture : source_color;
uniform float THRESHOLD = 0.5;
instance uniform bool DISABLED = true;

varying vec3 world_vertex;

void vertex() {
    world_vertex = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec4 tex_color = texture(albedo_texture, UV);
	
	if (!DISABLED) {
	    const mat4 bayer_matrix_4x4 = mat4(
		    vec4(    -0.5,       0.0,  -0.375,   0.125 ),
		    vec4(    0.25,   -0.25,   0.375, - 0.125 ),
		    vec4( -0.3125,  0.1875, -0.4375,  0.0625 ),
		    vec4(  0.4375, -0.0625,  0.3125, -0.1875 )
		);
		
	    float alpha = COLOR.r;
		vec2 SCREEN_CORDS = FRAGCOORD.xy;
		
		float bayer_shift = bayer_matrix_4x4[int(mod(SCREEN_CORDS.x, 4.0))][int(mod(SCREEN_CORDS.y, 4.0))];
	    alpha += bayer_shift;
	    ALBEDO = tex_color.rgb;
	    
		if (alpha < THRESHOLD) {
			discard;
		}
	}
	else
	{
		ALBEDO = tex_color.rgb;
	}
}
