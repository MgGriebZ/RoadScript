# Generates wwwroot/showcase/roadscript.json: RoadScript's build history from its git log.
# Columns are periods of different lengths; the launch weeks get the most room because
# most of the building happened then. Column subtitles are non-merge commit counts.
from datetime import date
from showcase_json import OUT_DIR, write

# (start date, end date exclusive, first unit, units)
PERIODS = [
    (date(2025, 11, 25), date(2025, 12, 1), 0, 2),
    (date(2025, 12, 1), date(2026, 1, 1), 2, 2),
    (date(2026, 1, 1), date(2026, 4, 1), 4, 2),
    (date(2026, 4, 1), date(2026, 7, 1), 6, 2),
    (date(2026, 7, 1), date(2026, 10, 1), 8, 1),
]
TOTAL = 10

def pos(d):
    for start, end, first, units in PERIODS:
        if start <= d < end:
            return first + units * (d - start).days / (end - start).days
    raise ValueError(d)

def item(title, start, end, icon, color, desc, spanning=False, greyed=False, min_len=None):
    s = round(start, 2)
    length = round(round(end, 2) - s, 2)
    if min_len and length < min_len:
        length = min_len
    it = {"title": title, "start": s, "length": length}
    if spanning: it["spanning"] = True
    it["icon"] = icon
    it["color"] = color
    if greyed: it["greyed"] = True
    it["description"] = "\n".join("- " + b for b in desc)
    return it

def ms(title, d, icon, color):
    return {"start": round(pos(d) / TOTAL * 100, 2), "title": title, "icon": icon, "color": color}

cols = [
    {"label": "Nov 2025", "sub": "Launch week, 65 commits"}, {"label": "", "sub": ""},
    {"label": "Dec 2025", "sub": "170 commits"}, {"label": "", "sub": ""},
    {"label": "Q1 2026", "sub": "76 commits"}, {"label": "", "sub": ""},
    {"label": "Q2 2026", "sub": "20 commits"}, {"label": "", "sub": ""},
    {"label": "Q3 2026", "sub": "This release"},
    {"label": "Next", "sub": "Planned"},
]
assert len(cols) == TOTAL

BLUE, TEAL, CORAL, PURPLE, GREEN = "#1E3A8A", "#0F766E", "#F88379", "#7C3AED", "#059669"
Q3 = pos(date(2026, 7, 1))

data = {
    "title": "RoadScript: how it was built",
    "subtitle": "From the first commit on November 25, 2025. Column subtitles count commits. Dashed items are ongoing.",
    "columns": cols,
    "milestones": [
        {"start": 1.0, "title": "First deploy", "icon": "rocket", "color": BLUE},
        ms("Share links", date(2025, 12, 20), "flag", CORAL),
        ms("Installable app", date(2026, 6, 2), "flag", TEAL),
    ],
    "lanes": [
        {"title": "Editing", "color": BLUE, "height": 1.0, "items": [
            item("First version", pos(date(2025, 11, 25)), pos(date(2025, 11, 28)), "rocket", BLUE, [
                "Live on day one: the Azure Static Web Apps deploy was added the same day as the first commit",
                "Visual preview next to a JSON editor, with clickable lanes, items and milestones",
                "Milestones and keyboard shortcuts by day two",
            ]),
            item("Direct editing", pos(date(2025, 12, 2)), pos(date(2025, 12, 19)), "wrench", BLUE, [
                "Drag items to move them and drag their edges to resize; drag milestones along the timeline",
                "Click anything in the preview to jump to it in the JSON",
                "Overlapping items stack into rows, Gantt style",
            ]),
            item("Richer items", pos(date(2026, 2, 18)), pos(date(2026, 3, 1)), "star", BLUE, [
                "Milestones can sit inside a lane, not only in the header",
                "Item details became free-form markdown in place of fixed bullet fields",
            ], min_len=0.88),
            item("Wide columns", pos(date(2026, 4, 30)), pos(date(2026, 5, 3)), "arrow-right", BLUE, [
                "A column with an empty label widens the column before it, for periods of different lengths like the ones on this roadmap",
            ], min_len=1.0),
        ]},
        {"title": "Organizing", "color": TEAL, "height": 0.9, "items": [
            item("Tabs and folders", pos(date(2025, 11, 30)), pos(date(2025, 12, 4)), "folder", TEAL, [
                "Several roadmaps per folder, in tabs, saved in the browser",
                "Folders with their own names, icons and colors",
            ], min_len=1.0),
            item("Linked roadmaps", pos(date(2026, 1, 14)), pos(date(2026, 1, 29)), "bookmark", TEAL, [
                "Link the title, a lane or an item to another roadmap, across folders",
                "Drag roadmaps between folders, with a check for links a move would break",
            ], min_len=1.0),
        ]},
        {"title": "Sharing", "color": CORAL, "height": 0.9, "items": [
            item("Share links", pos(date(2025, 12, 20)), pos(date(2025, 12, 21)), "globe", CORAL, [
                "The whole roadmap is compressed into the link itself, so sharing needs no server",
                "Opening a link offers to import the roadmap into your own folders",
            ], min_len=1.0),
            item("Export and install", pos(date(2026, 4, 17)), pos(date(2026, 6, 3)), "plus", CORAL, [
                "Paste a share link into the import dialog (April 2026)",
                "Export as Markdown or SVG (June 2026)",
                "Installs as an app; on phones, other apps can share a roadmap link to it",
            ], min_len=1.3),
            item("Landing page", Q3, Q3 + 0.95, "flag", CORAL, [
                "A landing page that shows before the app loads",
                "Read-only examples like this one, with an editable copy one click away",
                "Backups of saved data the app can't read, so nothing is overwritten",
            ]),
            item("Public launch", 9.05, 9.95, "rocket", CORAL, [
                "Launch on LinkedIn and Reddit",
            ]),
        ]},
        {"title": "Look and feel", "color": PURPLE, "height": 0.9, "items": [
            item("Templates", pos(date(2025, 11, 26)), pos(date(2025, 12, 8)), "dice", PURPLE, [
                "Five starter templates: daily planning, projects, milestones, retro and flows",
                "A dice button fills a template with random items to try things out",
            ]),
            item("Themes", pos(date(2025, 12, 19)), pos(date(2025, 12, 21)), "star", PURPLE, [
                "Backgrounds that follow the season (December 2025)",
                "Replaced the original vibe mode in January 2026",
            ], min_len=0.77),
            item("Phone layouts", pos(date(2025, 12, 31)), pos(date(2026, 3, 13)), "target", PURPLE, [
                "A first mobile layout in December 2025 and a full redesign in March 2026",
                "Several rounds of fixes, including a blank screen below 1200px wide",
                "Editing on a phone is still limited",
            ]),
        ]},
        {"title": "In use", "color": GREEN, "height": 0.55, "items": [
            item("Daily use at work", 0.0, TOTAL, "calendar", GREEN, [
                "Developers as columns, lanes for committed and blocked work",
                "Used in backlog refinement and sprint planning, and refined through that daily use",
            ], spanning=True),
        ]},
    ],
}

write(data, OUT_DIR / "roadscript.json")
for lane in data["lanes"]:
    print(lane["title"])
    for it in lane["items"]:
        print(f"  {it['title']:<28} {it['start']:>5} {it['length']:>5} -> {round(it['start']+it['length'],2)}")
print([m["start"] for m in data["milestones"]])
