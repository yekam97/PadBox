# Motion graphics promocional de PadBox: 1920x1080, 30 fps, 8 escenas de 2 compases a 126 BPM (~30 s).
# Dibuja cada fotograma con Pillow, escribe un MP4 intermedio (OpenCV) y la banda sonora (WAV) hecha con
# los sonidos originales del preset "Drum Pad EDM". mux.ps1 junta ambos en un MP4 H.264 con audio.
#   python render.py              -> out\video.mp4 + out\musica.wav
#   python render.py --stills 2,9 -> out\still_2.0.png, out\still_9.0.png (para revisar)
#   python render.py --lang en    -> versión en inglés en out\en (usa las capturas de docs\en)
import math
import os
import sys
import wave
from functools import lru_cache

import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(HERE)
LANG = sys.argv[sys.argv.index("--lang") + 1] if "--lang" in sys.argv else "es"
OUT = os.path.join(HERE, "out", "en") if LANG == "en" else os.path.join(HERE, "out")
SHOTS = os.path.join(REPO, "docs", "en") if LANG == "en" else os.path.join(REPO, "docs")

# Textos del video en inglés (cada texto que se dibuja se busca aquí tal cual)
EN = {
    "Tu controlador de pads, a otro nivel.": "Your pad controller, taken to the next level.",
    "Cualquier": "Works with",
    "controlador MIDI": "MIDI controllers",
    "de 8 o 16 pads": "with 8 or 16 pads",
    "y más": "and more",
    "El pad 1 cambia de modo": "Pad 1 switches modes",
    "y los pads 2 y 3 cambian de preset": "and pads 2 and 3 switch presets",
    "MÚSICA": "MUSIC",
    "OFICINA": "OFFICE",
    "MÚSICA  ·  1 de 13": "MUSIC  ·  1 of 13",
    "OFICINA  ·  1 de 6": "OFFICE  ·  1 of 6",
    "Batería rock": "Rock drums",
    "Escritorio": "Desktop",
    "Balada": "Ballad",
    "Reggaetón": "Reggaeton",
    "13 presets": "13 unique",
    "únicos": "presets",
    "Cada uno con su ritmo,": "Each with its own beat,",
    "instrumento y tonalidad": "instrument and key",
    "Bucles": "Synced",
    "sincronizados": "loops",
    "Prende capas y entran a tiempo": "Layers come in right on the beat",
    "Atajos con": "One-touch",
    "un toque": "shortcuts",
    "Windows · Navegador · OBS": "Windows · Browser · OBS",
    "Tus pads,": "Your pads,",
    "a todo color": "in full color",
    "El color cambia con cada preset": "Colors change with each preset",
    "M-VAVE SMC-PAD Pocket: probado": "M-VAVE SMC-PAD Pocket: tested",
    "Perfiles sin probar: Akai · Novation": "Untested profiles: Akai · Novation",
    "Otros: con «Detectar pads»": "Others: with “Detect pads”",
    "Gratis  ·  Windows 10 y 11  ·  Código abierto": "Free  ·  Windows 10 and 11  ·  Open source",
    "Consíguelo en Microsoft Store": "Get it on the Microsoft Store",
}


def tr(s):
    return EN.get(s, s) if LANG == "en" else s


W, H, FPS = 1920, 1080, 30
BPM = 126
BEAT = 60 / BPM
SCENE = 8 * BEAT                       # 2 compases (= largo de los bucles EDM)
N_SCENES = 8
DURATION = N_SCENES * SCENE
SS = 3                                 # supermuestreo para bordes suaves

# ---------- colores (los de la app) ----------
BG = (14, 16, 20)
PANEL = (30, 33, 41)
BODY = (34, 37, 46)
TEXT = (236, 238, 244)
MUTED = (150, 158, 178)
MUSIC = (124, 92, 255)
OFFICE = (0, 184, 148)
YELLOW = (255, 204, 0)
DEVICE_YELLOW = (250, 250, 0)
RED, ORANGE, GREEN = (255, 59, 48), (255, 149, 0), (52, 199, 89)
CYAN, BLUE, PURPLE, PINK, WHITE = (50, 215, 230), (10, 132, 255), (155, 92, 255), (255, 79, 163), (242, 242, 242)

FONT_DIR = r"C:\Windows\Fonts"
FONT_FILES = {"regular": "segoeui.ttf", "semibold": "seguisb.ttf", "bold": "segoeuib.ttf", "icons": "SegoeIcons.ttf"}


@lru_cache(64)
def font(weight, size):
    return ImageFont.truetype(os.path.join(FONT_DIR, FONT_FILES[weight]), size)


# ---------- animación ----------
def clamp(x, a=0.0, b=1.0):
    return max(a, min(b, x))


def prog(t, start, dur):
    return clamp((t - start) / dur)


def ease_out(x):
    x = clamp(x)
    return 1 - (1 - x) ** 3


def ease_in_out(x):
    x = clamp(x)
    return 4 * x ** 3 if x < 0.5 else 1 - (-2 * x + 2) ** 3 / 2


def ease_back(x, s=1.9):
    x = clamp(x)
    return 1 + (s + 1) * (x - 1) ** 3 + s * (x - 1) ** 2


def pulse(t, decay=0.16):
    """1 en cada tiempo (negra) y cae rápido: para que todo "golpee" con la música."""
    return math.exp(-(t % BEAT) / decay)


def mix(a, b, k):
    return tuple(int(a[i] + (b[i] - a[i]) * k) for i in range(3))


def q(c, step=6):
    return tuple(int(v) // step * step for v in c)


# ---------- sprites ----------
def paste(frame, spr, x, y, alpha=1.0):
    if spr is None or alpha <= 0.004:
        return
    x, y = int(round(x)), int(round(y))
    sw, sh = spr.size
    l, t, r, b = max(0, x), max(0, y), min(W, x + sw), min(H, y + sh)
    if r <= l or b <= t:
        return
    crop = spr.crop((l - x, t - y, r - x, b - y))
    if alpha < 0.996:
        crop = crop.copy()
        crop.putalpha(crop.getchannel("A").point(lambda v, k=alpha: int(v * k)))
    frame.alpha_composite(crop, dest=(l, t))


def paste_c(frame, spr, cx, cy, alpha=1.0):
    paste(frame, spr, cx - spr.size[0] / 2, cy - spr.size[1] / 2, alpha)


@lru_cache(2048)
def rrect(w, h, r, fill, outline=None, width=0):
    w, h = max(2, int(w)), max(2, int(h))
    big = Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0))
    ImageDraw.Draw(big).rounded_rectangle([0, 0, w * SS - 1, h * SS - 1], int(r * SS), fill=fill,
                                          outline=outline, width=int(width * SS))
    return big.resize((w, h), Image.LANCZOS)


@lru_cache(512)
def glow(color, size):
    size = max(8, int(size) // 10 * 10)
    yy, xx = np.mgrid[0:size, 0:size]
    d = np.sqrt((xx - size / 2) ** 2 + (yy - size / 2) ** 2) / (size / 2)
    a = np.clip(1 - d, 0, 1) ** 2.2
    arr = np.zeros((size, size, 4), np.uint8)
    arr[..., 0], arr[..., 1], arr[..., 2] = color
    arr[..., 3] = (a * 255).astype(np.uint8)
    return Image.fromarray(arr, "RGBA")


@lru_cache(1024)
def text(s, size, weight="regular", color=TEXT):
    s = tr(s)
    f = font(weight, size)
    asc, desc = f.getmetrics()
    w = int(f.getlength(s)) + 8
    img = Image.new("RGBA", (w, asc + desc + 4), (0, 0, 0, 0))
    ImageDraw.Draw(img).text((4, 2), s, font=f, fill=color)
    return img


@lru_cache(256)
def glyph(code, size, color=(255, 255, 255)):
    f = font("icons", size)
    ch = chr(code)
    l, t, r, b = f.getbbox(ch)
    img = Image.new("RGBA", (r - l + 4, b - t + 4), (0, 0, 0, 0))
    ImageDraw.Draw(img).text((2 - l, 2 - t), ch, font=f, fill=color)
    return img


@lru_cache(256)
def tile(code, color, size):
    """Ícono como los de PadBox: cuadro con degradado del color y el símbolo en blanco."""
    size = int(size)
    top, bot = mix(color, (255, 255, 255), 0.2), mix(color, (0, 0, 0), 0.15)
    k = np.linspace(0, 1, size)[:, None, None]
    grad = (np.array(top)[None, None, :] * (1 - k) + np.array(bot)[None, None, :] * k).repeat(size, axis=1)
    arr = np.dstack([grad.astype(np.uint8), np.zeros((size, size, 1), np.uint8)])
    img = Image.fromarray(arr, "RGBA")
    img.putalpha(rrect(size, size, size * 0.24, (255, 255, 255, 255)).getchannel("A"))
    g = glyph(code, int(size * 0.5))
    img.alpha_composite(g, ((size - g.size[0]) // 2, (size - g.size[1]) // 2))
    return img


def pill(label, color, size=26, pad_x=22, weight="bold", text_color=(255, 255, 255)):
    t = text(label, size, weight, text_color)
    h = int(size * 1.9)
    p = rrect(t.size[0] + pad_x * 2, h, h / 2, color + (255,)).copy()
    p.alpha_composite(t, (pad_x, (h - t.size[1]) // 2))
    return p


# ---------- recursos ----------
def load_shot(name):
    for p in (os.path.join(SHOTS, name),):
        if os.path.exists(p):
            return Image.open(p).convert("RGBA")
    raise FileNotFoundError(name)


SHOT_MUSIC = load_shot("captura-primera-vez.png")
SHOT_OFFICE = load_shot("captura-oficina.png")
SHOT_EDM = load_shot("captura-edm.png")


@lru_cache(64)
def shot_scaled(which, w):
    src = {"music": SHOT_MUSIC, "office": SHOT_OFFICE, "edm": SHOT_EDM}[which]
    w = int(w) // 4 * 4
    h = int(src.size[1] * w / src.size[0])
    img = src.resize((w, h), Image.LANCZOS)
    img.putalpha(rrect(w, h, 14, (255, 255, 255, 255)).getchannel("A"))
    return img


@lru_cache(16)
def shadow(w, h):
    w, h, m = int(w), int(h), 70
    img = Image.new("RGBA", (w + m * 2, h + m * 2), (0, 0, 0, 0))
    img.alpha_composite(rrect(w, h, 18, (0, 0, 0, 170)), (m, m + 24))
    return img.filter(ImageFilter.GaussianBlur(34))


def draw_shot(frame, which, x, y, w, alpha=1.0):
    s = shot_scaled(which, w)
    paste(frame, shadow(s.size[0], s.size[1]), x - 70, y - 70, alpha)
    paste(frame, s, x, y, alpha)
    return s.size


def make_background():
    yy = np.linspace(0, 1, H)[:, None]
    xx = np.linspace(0, 1, W)[None, :]
    base = np.array(BG, float)
    shade = 1 + 0.25 * (1 - yy) - 0.35 * ((xx - 0.5) ** 2 + (yy - 0.45) ** 2)
    arr = np.clip(base[None, None, :] * shade[..., None], 0, 255).astype(np.uint8)
    img = Image.fromarray(arr, "RGB").convert("RGBA")
    d = ImageDraw.Draw(img)
    for gx in range(0, W, 60):                      # cuadrícula de puntos muy suave
        for gy in range(0, H, 60):
            d.point((gx, gy), fill=(40, 44, 54, 255))
    return img


BACKGROUND = make_background()

# Íconos (mismos símbolos que PadBox)
IC = {"desktop": (0xE7F4, BLUE), "browser": (0xE774, OFFICE), "obs": (0xE7C8, (229, 72, 77)), "brush": (0xE771, (49, 168, 255)),
      "pen": (0xE76D, (255, 154, 0)), "doc": (0xE8A5, (79, 107, 237)), "notes": (0xE8D6, ORANGE), "keys": (0xE765, (91, 140, 255)),
      "song": (0xE90B, PINK), "bolt": (0xE945, (245, 176, 0)), "stream": (0xE93E, (142, 68, 255)), "drum": (0xE8D6, (255, 90, 79)),
      "play": (0xE768, (46, 204, 113)), "mic": (0xE720, (232, 67, 147))}
CHECK = 0xE73E


# ---------- el controlador dibujado ----------
def controller(frame, cx, cy, size, colors, press, alpha=1.0, label=None, label_color=MUTED):
    bw, bh = int(size), int(size * 1.1)
    x0, y0 = cx - bw / 2, cy - bh / 2
    paste(frame, shadow(bw, bh), x0 - 70, y0 - 70, alpha)
    paste(frame, rrect(bw, bh, size * 0.06, BODY + (255,), (52, 57, 70, 255), 2), x0, y0, alpha)
    # pantalla pequeña arriba
    paste(frame, rrect(bw * 0.5, size * 0.07, size * 0.02, (20, 22, 28, 255)), x0 + bw * 0.08, y0 + size * 0.045, alpha)
    if label:
        t = text(label, int(size * 0.035), "semibold", label_color)
        paste(frame, t, x0 + bw * 0.1, y0 + size * 0.045 + (size * 0.07 - t.size[1]) / 2, alpha)
    for k in range(3):   # perillas
        paste_c(frame, rrect(size * 0.07, size * 0.07, size * 0.035, (58, 63, 76, 255)), x0 + bw * (0.68 + k * 0.1), y0 + size * 0.08, alpha)
    margin, gap = size * 0.08, size * 0.035
    pad = (bw - 2 * margin - 3 * gap) / 4
    top = y0 + size * 0.19
    for i in range(16):
        r, c = divmod(i, 4)
        px, py = x0 + margin + c * (pad + gap), top + r * (pad + gap)
        g = clamp(press[i])
        col = mix(colors[i], (255, 255, 255), 0.65 * g)
        paste_c(frame, glow(q(colors[i], 16), pad * (1.9 + 0.5 * g)), px + pad / 2, py + pad / 2, alpha * (0.28 + 0.5 * g))
        paste(frame, rrect(pad, pad, pad * 0.16, q(mix(col, BODY, 0.12)) + (255,)), px, py, alpha)
        paste(frame, rrect(pad * 0.86, pad * 0.3, pad * 0.1, (255, 255, 255, 40)), px + pad * 0.07, py + pad * 0.06, alpha)


def caption(frame, x, y, lt, tag, tag_color, lines, sub, delay=0.2, title_size=76):
    a = ease_out(prog(lt, delay, 0.55))
    yy = y + (1 - a) * 50
    if tag:
        paste(frame, pill(tag, tag_color), x, yy, a)
        yy += 74
    for k, line in enumerate(lines):
        ak = ease_out(prog(lt, delay + 0.08 * (k + 1), 0.55))
        paste(frame, text(line, title_size, "bold"), x - 4, yy + (1 - ak) * 50, ak)
        yy += title_size * 1.22
    if sub:
        for k, line in enumerate(sub if isinstance(sub, list) else [sub]):
            ak = ease_out(prog(lt, delay + 0.3 + 0.06 * k, 0.55))
            paste(frame, text(line, 36, "regular", MUTED), x, yy + 14 + (1 - ak) * 40, ak)
            yy += 50


def osd(frame, cx, cy, mode, count, name, icon, accent, alpha):
    w, h = 640, 150
    box = rrect(w, h, 34, PANEL + (255,), mix(accent, (60, 65, 78), 0.4) + (255,), 2).copy()
    code, col = IC[icon]
    box.alpha_composite(tile(code, col, 102), (24, 24))
    box.alpha_composite(text(f"{mode}  ·  {count}", 24, "semibold", accent), (150, 26))
    box.alpha_composite(text(name, 46, "semibold"), (148, 58))
    paste(frame, shadow(w, h), cx - w / 2 - 70, cy - h / 2 - 70, alpha * 0.8)
    paste_c(frame, box, cx, cy, alpha)


# ---------- escenas (lt = segundos desde el inicio de la escena) ----------
LOGO_COLORS = [YELLOW, YELLOW, MUSIC, OFFICE]


def logo(frame, cx, cy, s, t0, lt, alpha=1.0):
    for k in range(4):
        a = ease_back(prog(lt, t0 + k * 0.1, 0.5))
        r, c = divmod(k, 2)
        sz = s * a
        paste_c(frame, rrect(max(2, sz), max(2, sz), sz * 0.22, LOGO_COLORS[k] + (255,)),
                cx + (c - 0.5) * (s + s * 0.14), cy + (r - 0.5) * (s + s * 0.14), alpha * clamp(a * 2))


def scene_intro(f, lt, t):
    p = pulse(t)
    paste_c(f, glow(MUSIC, 1300), 760, 540, 0.35 + 0.1 * p)
    paste_c(f, glow(OFFICE, 1100), 1200, 560, 0.25)
    slide = ease_in_out(prog(lt, 1.25, 0.6))
    lx = W / 2 - 330 * slide
    logo(f, lx, 500, 92, 0.15, lt)
    word = text("PadBox", 190, "bold")
    reveal = ease_out(prog(lt, 1.45, 0.7))
    if reveal > 0:
        wcrop = word.crop((0, 0, max(1, int(word.size[0] * reveal)), word.size[1]))
        paste(f, wcrop, lx + 150, 500 - word.size[1] / 2 - 12, reveal)
    a = ease_out(prog(lt, 2.25, 0.6))
    tg = text("Tu controlador de pads, a otro nivel.", 46, "regular", MUTED)
    paste_c(f, tg, W / 2, 700 + (1 - a) * 30, a)


def chase_press(t, order, speed=0.5, decay=0.35):
    """Pads que se encienden uno tras otro al ritmo (cada media negra)."""
    press = [0.0] * 16
    step = BEAT * speed
    k = int(t / step)
    for back in range(4):
        idx = k - back
        if idx < 0:
            continue
        dt = t - idx * step
        press[order[idx % 16]] = max(press[order[idx % 16]], math.exp(-dt / decay))
    return press


SPIRAL = [0, 1, 2, 3, 7, 11, 15, 14, 13, 12, 8, 4, 5, 6, 10, 9]


def scene_controller(f, lt, t):
    paste_c(f, glow(DEVICE_YELLOW, 1200), 600, 560, 0.18 + 0.08 * pulse(t))
    a = ease_out(prog(lt, 0.0, 0.7))
    colors = [mix(DEVICE_YELLOW, BODY, 0.08)] * 16
    controller(f, 600, 560 + (1 - a) * 120, 640, colors, chase_press(t, SPIRAL), a, "SMC-PAD")
    caption(f, 1060, 300, lt, None, None, ["Cualquier", "controlador MIDI"], ["de 8 o 16 pads"], 0.25)
    brands = ["M-VAVE", "Akai", "Novation", "y más"]
    x = 1064
    for k, b in enumerate(brands):
        ak = ease_back(prog(lt, 1.2 + k * BEAT / 2, 0.45))
        p = pill(b, (44, 48, 60), 28, weight="semibold", text_color=TEXT)
        paste(f, p, x, 690 + (1 - ak) * 30, clamp(ak * 1.5))
        x += p.size[0] + 14


def scene_modes(f, lt, t):
    switch = SCENE * 0.5
    k = ease_in_out(prog(lt, switch - 0.15, 0.4))
    accent = mix(MUSIC, OFFICE, k)
    paste_c(f, glow(q(accent, 16), 1300), W / 2, 480, 0.32)
    a = ease_out(prog(lt, 0.05, 0.6))
    paste_c(f, text("El pad 1 cambia de modo", 66, "bold"), W / 2, 190 + (1 - a) * 40, a)
    a2 = ease_out(prog(lt, 0.3, 0.6))
    paste_c(f, text("y los pads 2 y 3 cambian de preset", 36, "regular", MUTED), W / 2, 262 + (1 - a2) * 30, a2)
    # interruptor MÚSICA / OFICINA
    tw, th = 820, 132
    tx, ty = W / 2 - tw / 2, 400
    at = ease_out(prog(lt, 0.4, 0.6))
    paste(f, rrect(tw, th, th / 2, PANEL + (255,), (60, 65, 78, 255), 2), tx, ty + (1 - at) * 40, at)
    knob_x = tx + 10 + (tw / 2 - 10) * k
    paste(f, rrect(tw / 2 - 10, th - 20, (th - 20) / 2, q(accent, 4) + (255,)), knob_x, ty + 10 + (1 - at) * 40, at)
    paste_c(f, text("MÚSICA", 44, "bold", mix((255, 255, 255), MUTED, k)), tx + tw / 4, ty + th / 2 + (1 - at) * 40, at)
    paste_c(f, text("OFICINA", 44, "bold", mix(MUTED, (255, 255, 255), k)), tx + tw * 3 / 4, ty + th / 2 + (1 - at) * 40, at)
    # el pad 1 que se toca
    press = math.exp(-max(0, lt - (switch - 0.15)) / 0.25) if lt > switch - 0.15 else 0
    ring = prog(lt, switch - 0.15, 0.6)
    pc = (W / 2 - 560, ty + th / 2)
    if ring > 0 and ring < 1:
        r = 90 + 160 * ease_out(ring)
        paste_c(f, rrect(r, r, r * 0.2, None, q(accent, 8) + (int(200 * (1 - ring)),), 4), pc[0], pc[1], at)
    pcol = mix(accent, (255, 255, 255), 0.5 * press)
    paste_c(f, glow(q(accent, 16), 260), pc[0], pc[1], at * 0.5)
    paste_c(f, rrect(110, 110, 20, pcol + (255,)), pc[0], pc[1], at)
    paste_c(f, text("1", 40, "bold"), pc[0], pc[1], at)
    # aviso en pantalla (como el de la app)
    ao = ease_out(prog(lt, 0.8, 0.5))
    if k < 0.5:
        osd(f, W / 2, 760 + (1 - ao) * 80, "MÚSICA", "1 de 13", "Batería rock", "drum", MUSIC, ao * (1 - k * 2))
    else:
        osd(f, W / 2, 760, "OFICINA", "1 de 6", "Escritorio", "desktop", OFFICE, (k - 0.5) * 2)


GENRES = ["Cumbia", "Cumbia villera", "Vallenato", "Cuarteto", "Rock", "Balada", "Blues", "Bossa nova", "Reggaetón", "Dubstep"]


def scene_music(f, lt, t):
    paste_c(f, glow(MUSIC, 1400), 1250, 540, 0.3 + 0.08 * pulse(t))
    a = ease_out(prog(lt, 0.0, 0.8))
    w = 1060 + 40 * ease_out(prog(lt, 0, SCENE))
    draw_shot(f, "music", 790 + (1 - a) * 260, 160 + (1 - a) * 30 - (w - 1060) * 0.35, w, a)
    caption(f, 110, 190, lt, "MÚSICA", MUSIC, ["13 presets", "únicos"], ["Cada uno con su ritmo,", "instrumento y tonalidad"], 0.2, 80)
    x, y = 112, 700
    for k, g in enumerate(GENRES):
        ak = ease_back(prog(lt, 0.9 + k * BEAT / 2, 0.4))
        p = pill(g, (44, 48, 60), 26, weight="semibold", text_color=TEXT)
        if x + p.size[0] > 680:
            x, y = 112, y + 64
        paste(f, p, x, y + (1 - ak) * 24, clamp(ak * 1.5))
        x += p.size[0] + 12


EDM_PX, EDM_PY, EDM_PW, EDM_PH = [42, 306, 569, 833], [155, 350, 545, 740], 244, 173


def scene_edm(f, lt, t):
    p = pulse(t)
    paste_c(f, glow(PINK, 1300), 620, 540, 0.22 + 0.12 * p)
    paste_c(f, glow(BLUE, 1100), 1500, 600, 0.22)
    a = ease_out(prog(lt, 0.0, 0.8))
    sw = 1080
    sx, sy = 90 - (1 - a) * 260, 175
    s = sw / 1672
    draw_shot(f, "edm", sx, sy, sw, a)
    # bucles que se van prendiendo: borde que late al ritmo
    on = int(prog(lt, 0.5, SCENE - 1.0) * 9)
    for k in range(on):
        i = 3 + k
        r, c = divmod(i, 4)
        x, y = sx + EDM_PX[c] * s, sy + EDM_PY[r] * s
        paste(f, rrect(EDM_PW * s, EDM_PH * s, 12, None, (255, 255, 255, int(120 + 135 * p)), 3), x, y, a)
    caption(f, 1260, 200, lt, "DRUM PAD", PINK, ["Bucles", "sincronizados"], ["Prende capas y entran a tiempo"], 0.25, 76)
    # ecualizador
    bars, bx, by, bw = 22, 1264, 820, 22
    for k in range(bars):
        hgt = 30 + 150 * (0.5 + 0.5 * math.sin(t * (3 + k * 0.7) + k)) * (0.35 + 0.65 * p) * ease_out(prog(lt, 0.6, 0.5))
        col = mix(PINK, BLUE, k / bars)
        paste(f, rrect(bw - 6, hgt, 5, col + (255,)), bx + k * bw, by - hgt, a)


def scene_office(f, lt, t):
    paste_c(f, glow(OFFICE, 1400), 1250, 560, 0.28 + 0.06 * pulse(t))
    a = ease_out(prog(lt, 0.0, 0.8))
    draw_shot(f, "office", 800 + (1 - a) * 260, 190, 1040, a)
    caption(f, 110, 210, lt, "OFICINA", OFFICE, ["Atajos con", "un toque"], ["Windows · Navegador · OBS", "Photoshop · Illustrator · Word"], 0.2, 80)
    spots = [("desktop", 770, 150), ("browser", 1820, 260), ("obs", 1840, 640), ("brush", 760, 930), ("pen", 1300, 990), ("doc", 1760, 950)]
    for k, (name, x, y) in enumerate(spots):
        ak = ease_back(prog(lt, 0.7 + k * BEAT / 2, 0.45))
        code, col = IC[name]
        sz = 112 * ak
        if sz < 4:
            continue
        bob = math.sin(t * 2.2 + k) * 10
        paste_c(f, glow(col, 260), x, y + bob, 0.45 * ak)
        paste_c(f, tile(code, col, max(8, int(sz))), x, y + bob, clamp(ak * 1.5))


PALETTES = [
    ("Escritorio", [OFFICE, YELLOW, YELLOW, CYAN, CYAN, ORANGE, ORANGE, PURPLE, PURPLE, PINK, GREEN, GREEN, GREEN, BLUE, BLUE, BLUE]),
    ("OBS", [OFFICE, YELLOW, YELLOW, RED, RED, RED, RED, BLUE, BLUE, BLUE, BLUE, ORANGE, ORANGE, PINK, PURPLE, PURPLE]),
    ("Drum Pad EDM", [MUSIC, YELLOW, YELLOW, RED, RED, RED, ORANGE, ORANGE, BLUE, BLUE, GREEN, GREEN, PURPLE, PINK, PINK, PINK]),
    ("Cumbia", [MUSIC, YELLOW, YELLOW, RED] + [ORANGE] * 12),
]


def scene_colors(f, lt, t):
    step = SCENE / len(PALETTES)
    idx = min(len(PALETTES) - 1, int(lt / step))
    since = lt - idx * step
    name, pal = PALETTES[idx]
    prev = PALETTES[idx - 1][1] if idx > 0 else [mix(DEVICE_YELLOW, BODY, 0.08)] * 16
    colors, press = [], []
    for i in range(16):
        r, c = divmod(i, 4)
        k = ease_out(prog(since, math.hypot(r, c) * 0.05, 0.22))
        colors.append(mix(prev[i], pal[i], k))
        press.append(0.8 * math.exp(-max(0, since - math.hypot(r, c) * 0.05) / 0.2) if since > math.hypot(r, c) * 0.05 else 0)
    paste_c(f, glow(q(pal[5], 16), 1300), 600, 560, 0.25)
    a = ease_out(prog(lt, 0.0, 0.6))
    controller(f, 600, 560 + (1 - a) * 100, 640, colors, press, a, name, TEXT)
    caption(f, 1060, 190, lt, None, None, ["Tus pads,", "a todo color"], ["El color cambia con cada preset"], 0.2, 80)
    # Solo el Pocket lleva ✓: es el único probado con el aparato real.
    items = [("M-VAVE SMC-PAD Pocket: probado", True), ("Perfiles sin probar: Akai · Novation", False), ("Otros: con «Detectar pads»", False)]
    for k, (it, tested) in enumerate(items):
        ak = ease_out(prog(lt, 1.0 + k * BEAT / 2, 0.5))
        y = 590 + k * 70
        if tested:
            paste(f, glyph(CHECK, 34, GREEN), 1064, y + 8 + (1 - ak) * 20, ak)
        paste(f, text(it, 36, "semibold"), 1116, y + (1 - ak) * 20, ak)


def scene_outro(f, lt, t):
    p = pulse(t)
    paste_c(f, glow(MUSIC, 1500), W / 2, 470, 0.32 + 0.1 * p)
    a = ease_out(prog(lt, 0.0, 0.7))
    logo(f, W / 2 - 330, 380, 80, 0.0, lt)
    word = text("PadBox", 170, "bold")
    paste(f, word, W / 2 - 200, 380 - word.size[1] / 2 - 10, a)
    a2 = ease_out(prog(lt, 0.45, 0.6))
    paste_c(f, text("Gratis  ·  Windows 10 y 11  ·  Código abierto", 40, "regular", MUTED), W / 2, 540 + (1 - a2) * 30, a2)
    a3 = ease_back(prog(lt, 0.9, 0.6))
    if a3 > 0.02:
        btn = pill("Consíguelo en Microsoft Store", MUSIC, 40, 44)
        sc = (0.6 + 0.4 * a3) * (1 + 0.03 * p)
        btn = btn.resize((int(btn.size[0] * sc), int(btn.size[1] * sc)), Image.LANCZOS)
        paste_c(f, glow(MUSIC, 900), W / 2, 680, 0.4 * clamp(a3))
        paste_c(f, btn, W / 2, 680, clamp(a3 * 1.5))
    a4 = ease_out(prog(lt, 1.5, 0.6))
    paste_c(f, text("github.com/yekam97/PadBox", 32, "regular", MUTED), W / 2, 800 + (1 - a4) * 20, a4)


SCENES = [scene_intro, scene_controller, scene_modes, scene_music, scene_edm, scene_office, scene_colors, scene_outro]


def render_frame(t):
    frame = BACKGROUND.copy()
    si = min(N_SCENES - 1, int(t / SCENE))
    lt = t - si * SCENE
    layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    SCENES[si](layer, lt, t)
    out = 1.0 if si == N_SCENES - 1 else 1 - ease_in_out(prog(lt, SCENE - 0.3, 0.3))
    paste(frame, layer, 0, 0, out)
    if si > 0 and lt < 0.25:   # destello en cada cambio de escena (coincide con el golpe de la música)
        paste(frame, Image.new("RGBA", (W, H), (255, 255, 255, int(40 * (1 - lt / 0.25)))), 0, 0)
    if t > DURATION - 0.8:     # fundido final a negro
        paste(frame, Image.new("RGBA", (W, H), (0, 0, 0, int(255 * prog(t, DURATION - 0.8, 0.8)))), 0, 0)
    return frame.convert("RGB")


# ---------- banda sonora ----------
def read_wav(path):
    with wave.open(path) as w:
        data = np.frombuffer(w.readframes(w.getnframes()), np.int16).astype(np.float32) / 32768
        return data.reshape(-1, w.getnchannels()).mean(axis=1)


def soundtrack(path):
    for d in (os.path.join(os.path.dirname(REPO), "Sonidos", "EDM"), os.path.join(REPO, "dist", "PadBox-1.0.0", "Sonidos", "EDM")):
        if os.path.isdir(d):
            src = d
            break
    else:
        raise FileNotFoundError("Ejecuta build.bat primero: faltan los sonidos EDM")
    names = {"bat": "01 Bateria", "hats": "02 Hats", "perc": "03 Percusion", "bajo": "04 Bajo", "wub": "05 Bajo wub", "acordes": "06 Acordes",
             "pad": "07 Pad", "mel": "08 Melodia", "arp": "09 Arpegio", "voz": "10 Voz", "subida": "11 Subida", "impacto": "12 Impacto", "redoble": "13 Redoble"}
    s = {k: read_wav(os.path.join(src, v + ".wav")) for k, v in names.items()}
    sr = 44100
    seg = int(round(SCENE * sr))
    total = seg * N_SCENES + sr * 2
    out = np.zeros(total, np.float32)

    def put(key, sec, gain, offset=0.0):
        a = s[key]
        start = sec * seg + int(offset * sr)
        n = min(len(a), total - start)
        out[start:start + n] += a[:n] * gain

    plan = [
        {"pad": 0.75, "arp": 0.35, "subida": 0.8},
        {"impacto": 0.9, "bat": 0.85, "bajo": 0.7, "acordes": 0.5},
        {"bat": 0.85, "bajo": 0.7, "acordes": 0.45, "hats": 0.55, "mel": 0.6},
        {"bat": 0.85, "bajo": 0.7, "hats": 0.55, "mel": 0.55, "voz": 0.55, "perc": 0.5},
        {"bat": 0.85, "hats": 0.55, "wub": 0.45, "acordes": 0.45, "perc": 0.5},
        {"bat": 0.85, "hats": 0.55, "bajo": 0.7, "mel": 0.55, "arp": 0.35, "voz": 0.5},
        {"bat": 0.85, "hats": 0.55, "perc": 0.45, "bajo": 0.7, "acordes": 0.45, "mel": 0.5, "voz": 0.45, "redoble": 0.6},
        {"impacto": 0.9, "pad": 0.8, "acordes": 0.4, "arp": 0.3},
    ]
    for sec, parts in enumerate(plan):
        for k, g in parts.items():
            put(k, sec, g)
    out = out[: seg * N_SCENES]
    fade = int(1.2 * sr)
    out[-fade:] *= np.linspace(1, 0, fade) ** 1.5
    out = np.tanh(out * 1.1)
    out *= 0.9 / max(1e-6, np.abs(out).max())
    st = (np.repeat(out[:, None], 2, axis=1) * 32767).astype(np.int16)
    with wave.open(path, "wb") as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(st.tobytes())


def main():
    os.makedirs(OUT, exist_ok=True)
    if "--stills" in sys.argv:
        for ts in sys.argv[sys.argv.index("--stills") + 1].split(","):
            render_frame(float(ts)).save(os.path.join(OUT, f"still_{float(ts):.1f}.png"))
        return
    soundtrack(os.path.join(OUT, "musica.wav"))
    frames = int(round(DURATION * FPS))
    vw = cv2.VideoWriter(os.path.join(OUT, "video.mp4"), cv2.VideoWriter_fourcc(*"mp4v"), FPS, (W, H))
    vw.set(cv2.VIDEOWRITER_PROP_QUALITY, 100)
    for i in range(frames):
        img = render_frame(i / FPS)
        vw.write(cv2.cvtColor(np.asarray(img), cv2.COLOR_RGB2BGR))
        if i % 60 == 0:
            print(f"{i}/{frames}", flush=True)
    vw.release()
    print("listo", frames, "fotogramas")


if __name__ == "__main__":
    main()
