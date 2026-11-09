import html
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
PERSON = "#08427B"
SYSTEM = "#1168BD"
CONTAINER = "#438DD5"
EXTERNAL = "#8A8A8A"
LINE = "#555555"


class Svg:
    def __init__(self, width, height, title, subtitle):
        self.width, self.height = width, height
        self.parts = [
            f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}" '
            'font-family="Segoe UI, Arial, sans-serif">',
            '<defs><marker id="arrow" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="9" markerHeight="9" orient="auto-start-reverse">'
            f'<path d="M0,0 L10,5 L0,10 z" fill="{LINE}"/></marker></defs>',
            f'<rect width="{width}" height="{height}" fill="#ffffff"/>',
            f'<text x="30" y="40" font-size="24" font-weight="700" fill="#222">{html.escape(title)}</text>',
            f'<text x="30" y="66" font-size="15" fill="#555">{html.escape(subtitle)}</text>',
        ]

    def text(self, x, y, lines, size=14, color="#fff", weight="400", anchor="middle"):
        for i, line in enumerate(lines):
            w = "700" if i == 0 and weight == "title" else ("400" if weight == "title" else weight)
            s = size + 3 if i == 0 and weight == "title" else size
            self.parts.append(f'<text x="{x}" y="{y + i * (size + 5)}" font-size="{s}" font-weight="{w}" fill="{color}" '
                              f'text-anchor="{anchor}">{html.escape(line)}</text>')

    def box(self, x, y, w, h, fill, lines, dashed=False):
        dash = ' stroke-dasharray="6 4"' if dashed else ""
        self.parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="10" fill="{fill}" stroke="#1f1f1f" stroke-opacity="0.35"{dash}/>')
        top = y + h / 2 - (len(lines) - 1) * 19 / 2 + 5
        self.text(x + w / 2, top, lines, weight="title")
        return (x, y, w, h)

    def person(self, x, y, w, h, lines):
        r = 26
        self.parts.append(f'<circle cx="{x + w / 2}" cy="{y + r}" r="{r}" fill="{PERSON}" stroke="#1f1f1f" stroke-opacity="0.35"/>')
        return self.box(x, y + 2 * r - 6, w, h, PERSON, lines)

    def cylinder(self, x, y, w, h, fill, lines):
        e = 16
        self.parts.append(f'<path d="M{x},{y + e} L{x},{y + h - e} A{w / 2},{e} 0 0 0 {x + w},{y + h - e} L{x + w},{y + e} '
                          f'A{w / 2},{e} 0 0 0 {x},{y + e} Z" fill="{fill}" stroke="#1f1f1f" stroke-opacity="0.35"/>')
        self.parts.append(f'<ellipse cx="{x + w / 2}" cy="{y + e}" rx="{w / 2}" ry="{e}" fill="{fill}" stroke="#1f1f1f" stroke-opacity="0.35"/>')
        top = y + h / 2 - (len(lines) - 1) * 19 / 2 + 12
        self.text(x + w / 2, top, lines, weight="title")
        return (x, y, w, h)

    def boundary(self, x, y, w, h, label):
        self.parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="6" fill="none" stroke="#444" stroke-width="2" stroke-dasharray="10 6"/>')
        self.text(x + 14, y + h - 14, [label], size=15, color="#333", weight="700", anchor="start")

    def arrow(self, x1, y1, x2, y2, lines, lx=None, ly=None, dotted=False):
        dash = ' stroke-dasharray="5 5"' if dotted else ""
        self.parts.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{LINE}" stroke-width="1.8" marker-end="url(#arrow)"{dash}/>')
        lx = (x1 + x2) / 2 if lx is None else lx
        ly = (y1 + y2) / 2 if ly is None else ly
        for i, line in enumerate(lines):
            italic = ' font-style="italic"' if line.startswith("[") else ""
            self.parts.append(f'<text x="{lx}" y="{ly + i * 17}" font-size="13" fill="#222" text-anchor="middle" paint-order="stroke" '
                              f'stroke="#ffffff" stroke-width="5" stroke-linejoin="round"{italic}>{html.escape(line)}</text>')

    def legend(self, y, items):
        x = 30
        self.parts.append(f'<text x="{x}" y="{y}" font-size="14" font-weight="700" fill="#333">Key</text>')
        x += 50
        for fill, label, shape in items:
            if shape == "dash":
                self.parts.append(f'<rect x="{x}" y="{y - 14}" width="28" height="18" fill="none" stroke="#444" stroke-width="2" stroke-dasharray="6 4"/>')
            elif shape == "dotted":
                self.parts.append(f'<line x1="{x}" y1="{y - 5}" x2="{x + 28}" y2="{y - 5}" stroke="{LINE}" stroke-width="1.8" stroke-dasharray="5 5"/>')
            else:
                self.parts.append(f'<rect x="{x}" y="{y - 14}" width="28" height="18" rx="4" fill="{fill}"/>')
            self.parts.append(f'<text x="{x + 36}" y="{y}" font-size="13" fill="#333">{html.escape(label)}</text>')
            x += 46 + 7.2 * len(label)

    def save(self, name):
        path = os.path.join(HERE, name)
        with open(path, "w", encoding="utf-8") as f:
            f.write("\n".join(self.parts + ["</svg>"]) + "\n")
        return path


def context():
    s = Svg(1400, 900, "The Meridian Newsfolio — C4 Level 1: System Context",
            "Who uses the system and which outside systems it depends on. Scope: the whole software system.")
    s.person(120, 95, 260, 104, ["Reader", "[Person]", "Reads, searches, comments,", "bookmarks, subscribes"])
    s.person(570, 95, 260, 104, ["Author", "[Person]", "Writes stories and submits", "them for review"])
    s.person(1020, 95, 260, 104, ["Admin", "[Person]", "Reviews, schedules, moderates,", "sends the digest"])
    s.box(450, 360, 500, 170, SYSTEM, ["The Meridian Newsfolio", "[Software System]", "Bilingual (English/Bangla) news archive with",
                                      "editorial review, word search, recommendations,", "comment moderation and a newsletter"])
    s.box(30, 380, 250, 120, EXTERNAL, ["Font and icon CDN", "[External System]", "Google Fonts, jsDelivr", "(Bootstrap Icons)"])
    s.box(1120, 380, 250, 120, EXTERNAL, ["Search engines and", "feed readers", "[External Systems]", "crawl pages, sitemap, RSS"])
    s.box(110, 680, 300, 120, EXTERNAL, ["API client", "[External System]", "App or script using the", "token-protected REST API"])
    s.text(1000, 700, ["No email provider is connected.", "Real email is not implemented;", "Development writes mails to a", "private local outbox file."], size=15, color="#333", anchor="start")
    s.arrow(300, 255, 560, 360, ["Reads and searches news,", "comments, subscribes", "[HTTPS, web browser]"], lx=540, ly=268)
    s.arrow(700, 255, 700, 360, ["Writes and submits stories", "[HTTPS, web browser]"], lx=805, ly=300)
    s.arrow(1100, 255, 840, 360, ["Reviews, schedules,", "moderates", "[HTTPS, web browser]"], lx=1135, ly=300)
    s.arrow(1120, 440, 950, 440, ["Fetch pages,", "sitemap, RSS", "[HTTPS, XML]"], lx=1035, ly=470)
    s.arrow(330, 680, 520, 530, ["Calls JSON endpoints", "[HTTPS/JSON, bearer token]"], lx=280, ly=600)
    s.arrow(200, 255, 155, 380, ["Browser loads", "fonts and icons", "[HTTPS]"], lx=110, ly=300, dotted=True)
    s.legend(870, [(PERSON, "Person", "box"), (SYSTEM, "System in scope", "box"), (EXTERNAL, "External system", "box"),
                   (None, "Call made by the browser, not the server", "dotted")])
    return s.save("c4_level1_context.svg")


def containers():
    s = Svg(1500, 1000, "The Meridian Newsfolio — C4 Level 2: Containers",
            "The separately running parts inside the system boundary and the technology each one uses.")
    s.box(30, 100, 230, 110, EXTERNAL, ["Font and icon CDN", "[External System]", "Google Fonts, jsDelivr"])
    s.person(330, 80, 280, 86, ["Reader / Author / Admin", "[Person]", "Uses the site in a web browser"])
    s.box(690, 100, 260, 110, EXTERNAL, ["API client", "[External System]", "Bearer-token REST calls"])
    s.box(1030, 100, 260, 110, EXTERNAL, ["Search engines and", "feed readers", "[External Systems]"])
    s.boundary(60, 280, 1140, 640, "The Meridian Newsfolio [Software System]")
    s.box(90, 360, 270, 150, CONTAINER, ["Browser scripts", "[Container: JavaScript +", "service worker, runs in", "the browser]",
                                         "Theme, suggestions, preview"])
    s.box(540, 340, 400, 190, CONTAINER, ["Web application", "[Container: ASP.NET Core 10", "MVC + Web API on Kestrel]",
                                          "UI, BLL and DAL in one process;", "Razor pages, JSON API, Swagger,", "background publisher and digest"])
    s.cylinder(560, 700, 360, 170, CONTAINER, ["Database", "[Container: SQL Server,", "EF Core migrations]",
                                               "Articles, users, comments,", "word index, audit log"])
    s.cylinder(110, 710, 260, 150, CONTAINER, ["Uploaded pictures", "[Container: file system,", "wwwroot/uploads]"])
    s.cylinder(990, 710, 190, 150, CONTAINER, ["Mail outbox", "[Container: private", "file folder, App_Data,", "Development only]"])
    s.text(1225, 470, ["No email provider:", "real email is not", "implemented. Mail is", "only written to the", "Development outbox."], size=14, color="#333", anchor="start")
    s.arrow(470, 212, 640, 340, ["Visits pages,", "posts forms", "[HTTPS]"], lx=470, ly=296)
    s.arrow(820, 210, 820, 340, ["Calls REST API", "[HTTPS/JSON,", "bearer token]"], lx=895, ly=232)
    s.arrow(1130, 210, 920, 340, ["Reads sitemap,", "RSS, pages", "[HTTPS/XML]"], lx=1190, ly=232)
    s.arrow(540, 400, 360, 400, ["Delivers HTML,", "CSS, JS [HTTPS]"], lx=450, ly=378)
    s.arrow(360, 470, 540, 470, ["JSON fetch calls", "[HTTPS, antiforgery]"], lx=450, ly=500)
    s.arrow(160, 360, 150, 210, ["Loads fonts,", "icons [HTTPS]"], lx=210, ly=300, dotted=True)
    s.arrow(740, 530, 740, 700, ["Reads and writes", "[EF Core 10,", "TDS over TCP 1433]"], lx=820, ly=600)
    s.arrow(580, 530, 300, 710, ["Saves and deletes", "pictures [file I/O]"], lx=390, ly=610)
    s.arrow(900, 530, 1060, 710, ["Writes email copies", "[file I/O]"], lx=1050, ly=610)
    s.legend(970, [(PERSON, "Person", "box"), (CONTAINER, "Container", "box"), (EXTERNAL, "External system", "box"),
                   (None, "System boundary", "dash"), (None, "Call made by the browser", "dotted")])
    return s.save("c4_level2_containers.svg")


def render(svg, png, width, height):
    browser = next((p for p in [r"C:\Program Files\Google\Chrome\Application\chrome.exe",
                                r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"] if os.path.exists(p)), None)
    if browser is None:
        print("No Chrome or Edge found; SVG written, PNG skipped.")
        return
    subprocess.run([browser, "--headless=new", "--disable-gpu", "--hide-scrollbars", f"--window-size={width},{height}",
                    "--force-device-scale-factor=2", f"--screenshot={png}", "file:///" + svg.replace(os.sep, "/")],
                   check=True, capture_output=True)


if __name__ == "__main__":
    for svg, (w, h) in ((context(), (1400, 900)), (containers(), (1500, 1000))):
        png = svg[:-4] + ".png"
        render(svg, png, w, h)
        print("wrote", os.path.relpath(svg, HERE), "and", os.path.relpath(png, HERE) if os.path.exists(png) else "(no PNG)")
    sys.exit(0)
