import os
from PIL import Image, ImageDraw

output_dir = r"E:\university\term7\Game\GD-HW1-ChromeDino\Assets\Sprites"
os.makedirs(output_dir, exist_ok=True)

DARK = (83, 83, 83, 255)       # #535353 Classic Dino Grey
WHITE = (255, 255, 255, 255)
GOLD = (243, 156, 18, 255)
GOLD_LIGHT = (254, 211, 48, 255)
GOLD_DARK = (211, 84, 0, 255)
RED = (231, 76, 60, 255)
CLOUD_GREY = (180, 180, 180, 255)

def scale_img(img, factor=2):
    return img.resize((img.width * factor, img.height * factor), Image.Resampling.NEAREST)

# 1. OFFICIAL GOOGLE CHROME DINO & PTERODACTYL SPRITES
import urllib.request
url_sheet = "https://raw.githubusercontent.com/chromium/chromium/main/components/neterror/resources/images/default_100_percent/offline/100-offline-sprite.png"
temp_sheet = os.path.join(output_dir, "temp_sheet.png")
try:
    urllib.request.urlretrieve(url_sheet, temp_sheet)
    sheet = Image.open(temp_sheet).convert("RGBA")
    
    crops = {
        "Dino_Jump": (848, 2, 892, 49),
        "Dino_Run1": (936, 2, 980, 49),
        "Dino_Run2": (980, 2, 1024, 49),
        "Dino_Hit":  (1024, 2, 1068, 49),
        "Pterodactyl_Fly1": (134, 2, 180, 42),
        "Pterodactyl_Fly2": (180, 2, 226, 42),
    }
    for name, box in crops.items():
        cropped = sheet.crop(box)
        scaled = scale_img(cropped, 2)
        scaled.save(os.path.join(output_dir, f"{name}.png"))
    
    # Update main Dino and Pterodactyl fallback sprites
    scale_img(sheet.crop(crops["Dino_Jump"]), 2).save(os.path.join(output_dir, "Dino.png"))
    scale_img(sheet.crop(crops["Pterodactyl_Fly1"]), 2).save(os.path.join(output_dir, "Pterodactyl.png"))
    
    if os.path.exists(temp_sheet):
        os.remove(temp_sheet)
    print("Official Chromium Dino and Pterodactyl sprites extracted successfully.")
except Exception as e:
    print("Failed to download official sheet:", e)

# 2. CACTUS SPRITE (28 x 50)
cactus_raw = Image.new("RGBA", (28, 50), (0, 0, 0, 0))
d = ImageDraw.Draw(cactus_raw)

# Central stem
d.rectangle([10, 2, 17, 49], fill=DARK)
# Left branch
d.rectangle([2, 14, 7, 28], fill=DARK)
d.rectangle([2, 24, 10, 28], fill=DARK)
# Right branch
d.rectangle([20, 18, 25, 34], fill=DARK)
d.rectangle([17, 30, 25, 34], fill=DARK)

cactus = scale_img(cactus_raw, 2) # 56x100
cactus.save(os.path.join(output_dir, "Cactus.png"))

# 3. PTERODACTYL SPRITES (Extracted above from official sheet)

# 4. COIN SPRITE (32 x 32)
coin = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
d = ImageDraw.Draw(coin)
d.ellipse([2, 2, 29, 29], fill=GOLD_DARK)
d.ellipse([4, 4, 27, 27], fill=GOLD)
d.ellipse([7, 7, 24, 24], fill=GOLD_LIGHT)
d.rectangle([14, 9, 17, 22], fill=GOLD_DARK)
d.rectangle([11, 11, 20, 14], fill=GOLD_DARK)
d.rectangle([11, 17, 20, 20], fill=GOLD_DARK)
coin.save(os.path.join(output_dir, "Coin.png"))

# 5. GROUND SPRITE (Tileable, 600 x 24)
ground = Image.new("RGBA", (600, 24), (0, 0, 0, 0))
d = ImageDraw.Draw(ground)

# Main ground horizon line
d.line([(0, 4), (599, 4)], fill=DARK, width=2)

# Small bumps, bumps and dashes under the line
import random
random.seed(42)
for x in range(0, 595, 12):
    if random.random() > 0.4:
        length = random.randint(3, 10)
        y = random.randint(7, 18)
        d.line([(x, y), (x + length, y)], fill=DARK, width=1)
    if random.random() > 0.7:
        d.point([(x + 2, random.randint(9, 20))], fill=DARK)

ground.save(os.path.join(output_dir, "Ground.png"))

# 6. CLOUD SPRITE (64 x 24)
cloud = Image.new("RGBA", (64, 24), (0, 0, 0, 0))
d = ImageDraw.Draw(cloud)
d.ellipse([10, 8, 30, 20], fill=CLOUD_GREY)
d.ellipse([20, 4, 45, 20], fill=CLOUD_GREY)
d.ellipse([35, 8, 55, 20], fill=CLOUD_GREY)
d.rectangle([15, 14, 50, 20], fill=CLOUD_GREY)
cloud.save(os.path.join(output_dir, "Cloud.png"))

# 7. HEART SPRITE (32 x 32)
heart = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
d = ImageDraw.Draw(heart)
d.ellipse([4, 4, 17, 17], fill=RED)
d.ellipse([14, 4, 27, 17], fill=RED)
d.polygon([(4, 13), (27, 13), (15, 28)], fill=RED)
d.ellipse([7, 7, 11, 11], fill=WHITE) # shine
heart.save(os.path.join(output_dir, "Heart.png"))

print("All sprites successfully generated in Assets/Sprites!")
