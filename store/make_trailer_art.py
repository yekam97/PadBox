# Imágenes que pide la Store para los tráileres (1920 x 1080, PNG):
#   store/trailer/hero.png        «Super hero art»: se ve arriba de la ficha después del tráiler. NO puede llevar el nombre de la app.
#   store/trailer/thumb-es.png    miniatura del tráiler en español (sale del fotograma del logo del video)
#   store/trailer/thumb-en.png    miniatura del tráiler en inglés
# Antes: python video/render.py --stills 3   y   python video/render.py --lang en --stills 3
import os

from PIL import Image, ImageDraw, ImageFilter

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(HERE)
OUT = os.path.join(HERE, "trailer")
W, H, SS = 1920, 1080, 2
BG = (14, 16, 20)
# Colores de los pads de la app: modo, anterior/siguiente y los grupos de acciones
GRID = [
    (124, 92, 255), (255, 204, 0), (255, 204, 0), (255, 59, 48),
    (255, 149, 0), (255, 149, 0), (10, 132, 255), (10, 132, 255),
    (52, 199, 89), (52, 199, 89), (155, 92, 255), (255, 79, 163),
    (50, 215, 230), (0, 184, 148), (255, 79, 163), (242, 242, 242),
]


def glow(img, center, radius, color, alpha):
    layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    x, y = center
    d.ellipse([x - radius, y - radius, x + radius, y + radius], fill=color + (alpha,))
    img.alpha_composite(layer.filter(ImageFilter.GaussianBlur(radius * 0.45)))


def hero():
    img = Image.new("RGBA", (W * SS, H * SS), BG + (255,))
    glow(img, (W * SS * 0.38, H * SS * 0.5), 520 * SS, (124, 92, 255), 110)
    glow(img, (W * SS * 0.66, H * SS * 0.55), 480 * SS, (0, 184, 148), 80)
    cell, gap = 150 * SS, 26 * SS
    size = cell * 4 + gap * 3
    x0, y0 = (W * SS - size) // 2, (H * SS - size) // 2
    # sombra y cuerpo del controlador
    body = Image.new("RGBA", img.size, (0, 0, 0, 0))
    bd = ImageDraw.Draw(body)
    m = 56 * SS
    bd.rounded_rectangle([x0 - m, y0 - m, x0 + size + m, y0 + size + m], 48 * SS, fill=(0, 0, 0, 170))
    img.alpha_composite(body.filter(ImageFilter.GaussianBlur(40 * SS)))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([x0 - m, y0 - m, x0 + size + m, y0 + size + m], 48 * SS, fill=(34, 37, 46, 255), outline=(52, 56, 68, 255), width=3 * SS)
    lit = {3, 6, 9, 12}   # algunos pads «encendidos» con brillo
    pos = [(x0 + (k % 4) * (cell + gap), y0 + (k // 4) * (cell + gap)) for k in range(16)]
    for k in lit:   # primero los brillos, debajo de los pads
        x, y = pos[k]
        glow(img, (x + cell / 2, y + cell / 2), cell * 0.9, GRID[k], 140)
    d = ImageDraw.Draw(img)
    shine = Image.new("RGBA", img.size, (0, 0, 0, 0))   # reflejo suave arriba de cada pad (capa aparte para que se mezcle)
    sd = ImageDraw.Draw(shine)
    for k, col in enumerate(GRID):
        x, y = pos[k]
        d.rounded_rectangle([x, y, x + cell, y + cell], 22 * SS, fill=col + (255,))
        sd.rounded_rectangle([x + 6 * SS, y + 6 * SS, x + cell - 6 * SS, y + cell * 0.42], 18 * SS, fill=(255, 255, 255, 34))
    img.alpha_composite(shine)
    img.convert("RGB").resize((W, H), Image.LANCZOS).save(os.path.join(OUT, "hero.png"))


def thumbs():
    for lang, src in (("es", os.path.join(REPO, "video", "out", "still_3.0.png")),
                      ("en", os.path.join(REPO, "video", "out", "en", "still_3.0.png"))):
        Image.open(src).convert("RGB").resize((W, H)).save(os.path.join(OUT, f"thumb-{lang}.png"))


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    hero()
    thumbs()
    print("listo:", OUT)
