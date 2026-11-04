# Genera el ícono de PadBox (src\PadBox.ico) y las imágenes que pide la Microsoft Store (store\Assets).
# El logo son los 4 cuadros de la app: dos amarillos (preset anterior/siguiente), morado (Música) y verde (Oficina).
#   python store\make_assets.py
import os

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(HERE)
ASSETS = os.path.join(HERE, "Assets")
COLORS = [(255, 204, 0), (255, 204, 0), (124, 92, 255), (0, 184, 148)]
SS = 4


def logo(size, pad=0.12):
    """Los 4 cuadros redondeados, centrados en un lienzo transparente de size x size."""
    big = size * SS
    img = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    inner = big * (1 - 2 * pad)
    gap = inner * 0.1
    cell = (inner - gap) / 2
    x0 = y0 = big * pad
    for k, col in enumerate(COLORS):
        r, c = divmod(k, 2)
        x, y = x0 + c * (cell + gap), y0 + r * (cell + gap)
        d.rounded_rectangle([x, y, x + cell, y + cell], cell * 0.24, fill=col + (255,))
    return img.resize((size, size), Image.LANCZOS)


def wide(w, h):
    # Solo el logo, sin texto: así el ícono no depende de la licencia de ninguna fuente.
    big = Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0))
    mark = logo(int(h * 0.62) * SS, 0.06)
    big.alpha_composite(mark, ((w * SS - mark.size[0]) // 2, (h * SS - mark.size[1]) // 2))
    return big.resize((w, h), Image.LANCZOS)


def save(img, name):
    img.save(os.path.join(ASSETS, name))


def main():
    os.makedirs(ASSETS, exist_ok=True)
    # Ícono del .exe (varios tamaños en un solo .ico)
    sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
    logo(256, 0.06).save(os.path.join(REPO, "src", "PadBox.ico"), sizes=[(s, s) for s in sizes])
    # Imágenes del paquete MSIX (escala 100 y 200; tamaños "targetsize" para la barra de tareas e inicio)
    for scale in (100, 200):
        k = scale / 100
        save(logo(int(50 * k), 0.08), f"StoreLogo.scale-{scale}.png")
        save(logo(int(44 * k), 0.08), f"Square44x44Logo.scale-{scale}.png")
        save(logo(int(150 * k), 0.22), f"Square150x150Logo.scale-{scale}.png")
        save(wide(int(310 * k), int(150 * k)), f"Wide310x150Logo.scale-{scale}.png")
    for ts in (16, 24, 32, 48, 256):
        save(logo(ts, 0.04), f"Square44x44Logo.targetsize-{ts}.png")
        save(logo(ts, 0.04), f"Square44x44Logo.targetsize-{ts}_altform-unplated.png")
    print("listo:", ASSETS)


if __name__ == "__main__":
    main()
