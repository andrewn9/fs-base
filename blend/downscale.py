import os
import numpy as np
from PIL import Image

# === Configuration ===
folder = "textures"  # Folder containing your textures
target_size = (64, 64)  # Resize target (width, height)
noise_strength = 25     # Noise intensity: 0-255 (lower is subtle, higher is strong)

# Supported image extensions
image_extensions = {'.png', '.jpg', '.jpeg', '.bmp', '.tga'}

# Function to add noise to a PIL image
def add_noise(pil_image, strength):
    # Convert image to numpy array
    arr = np.array(pil_image).astype(np.int16)

    # Generate random noise
    noise = np.random.randint(-strength, strength + 1, arr.shape, dtype=np.int16)

    # Add noise and clip to valid range
    noisy_arr = np.clip(arr + noise, 0, 255).astype(np.uint8)

    # Convert back to PIL image
    return Image.fromarray(noisy_arr)

# Process each image in the folder
for filename in os.listdir(folder):
    name, ext = os.path.splitext(filename)
    if ext.lower() not in image_extensions:
        continue  # Skip non-image files

    path = os.path.join(folder, filename)

    # Open, resize, add noise, and overwrite
    with Image.open(path) as img:
        resized = img.resize(target_size, resample=Image.NEAREST)
        noisy = add_noise(resized, noise_strength)
        noisy.save(path)

    print(f"Resized to 64x64 and added noise: {filename}")
