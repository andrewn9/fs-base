shader_type canvas_item;
render_mode blend_premul_alpha, unshaded;

void fragment() {
    vec4 tex_col = texture(TEXTURE, UV);

    // If alpha is low, fill color — otherwise make transparent
    if (tex_col.a < 0.05) {
        COLOR.rgb = vec3(1.0); // You can change this to match your background
        COLOR.a = 1.0;
    } else {
        discard; // Don't draw anything where character is visible
    }
}
