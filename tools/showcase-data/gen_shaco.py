# Generates wwwroot/showcase/shaco.json: shaco's build history from its git log.
from datetime import date
from showcase_json import OUT_DIR, write

PERIODS = [
    (date(2026, 7, 3), date(2026, 7, 10), 0, 3),
    (date(2026, 7, 10), date(2026, 7, 17), 3, 2),
    (date(2026, 7, 17), date(2026, 7, 24), 5, 2),
    (date(2026, 7, 24), date(2026, 8, 3), 7, 1),
]
TOTAL = 10

def pos(d):
    for start, end, first, units in PERIODS:
        if start <= d < end:
            return first + units * (d - start).days / (end - start).days
    raise ValueError(d)

def end_of(d):
    """Position at the end of day d."""
    for start, end, first, units in PERIODS:
        if start <= d < end:
            return first + units * ((d - start).days + 1) / (end - start).days
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

cols = [
    {"label": "Week 1", "sub": "Jul 3 to 9, 81 commits"}, {"label": "", "sub": ""}, {"label": "", "sub": ""},
    {"label": "Week 2", "sub": "Jul 10 to 16, 47 commits"}, {"label": "", "sub": ""},
    {"label": "Week 3", "sub": "Jul 17 to 23, 29 commits"}, {"label": "", "sub": ""},
    {"label": "Week 4", "sub": "Jul 24 to Aug 2, 2 commits"},
    {"label": "Next", "sub": "Planned"}, {"label": "", "sub": ""},
]
assert len(cols) == TOTAL

RED, PURPLE, ORANGE, TEAL = "#EF4444", "#7C3AED", "#D4652F", "#0F766E"
D = lambda day, month=7: date(2026, month, day)

data = {
    "title": "shaco: how it was built",
    "subtitle": "A field guide to AP Shaco support in League of Legends at shaco.mggriebz.com. Built in four weeks from July 3, 2026, and live since. Column subtitles count commits.",
    "columns": cols,
    "milestones": [
        {"start": 1.0, "title": "Live on day one", "icon": "rocket", "color": RED},
    ],
    "lanes": [
        {"title": "Site", "color": RED, "height": 1.0, "items": [
            item("First release", pos(D(3)), end_of(D(4)), "rocket", RED, [
                "A written PRD first, then six build phases the same day: scaffold, design system, content model, data pipeline, components and pages",
                "Astro and Tailwind on Azure Static Web Apps",
                "The next day, the build page became a step-by-step guide",
            ]),
            item("Polish passes", pos(D(5)), end_of(D(14)), "star", RED, [
                "A UX overhaul PRD applied across every page (July 5)",
                "Home page redesign (July 13) and a performance PRD for first-load speed and caching (July 14)",
            ]),
            item("Field notes", pos(D(20)), end_of(D(23)), "bookmark", RED, [
                "Visitor comments on each page, with a moderation panel (July 20 to 21)",
            ]),
            item("Every champion", 8.05, 9.95, "flag", RED, [
                "Guides and clips for every champion and role, from MgGriebZ.com's League data",
            ]),
        ]},
        {"title": "Clips", "color": PURPLE, "height": 1.0, "items": [
            item("Clip boxes", pos(D(5)), end_of(D(2, 8)), "square", PURPLE, [
                "Five to ten second clips with notes, tagged by mechanic or matchup: vision control, level 1 trades, lane defense",
                "Each clip links to its real match record",
                "A repeatable path from replay, to clip, to a guide entry",
            ]),
        ]},
        {"title": "Player", "color": PURPLE, "height": 0.8, "items": [
            item("Clip player", pos(D(4)), end_of(D(5)), "target", PURPLE, [
                "Short clips that loop, with loop controls on the detail page (July 4 to 5)",
            ], min_len=0.9),
            item("Detail screen", pos(D(13)), end_of(D(18)), "clock", PURPLE, [
                "Player and notes side by side instead of stacked (July 13)",
                "Redesigned detail screen with a playback dock: speed control between frame-step buttons (July 16 to 18)",
            ]),
        ]},
        {"title": "Data", "color": ORANGE, "height": 1.0, "items": [
            item("Stats pipeline", pos(D(3)), end_of(D(7)), "chart", ORANGE, [
                "Game stats pulled from the MgGriebZ.com API at build time (July 3)",
                "MgGriebZ.com starts a rebuild when a new clip is logged (July 7)",
            ]),
            item("Wards", pos(D(8)), end_of(D(8)), "search", ORANGE, [
                "First-party visit tracking and game vision stats, planned in one PRD and shipped in eight phases on July 8",
                "A private dashboard behind a GitHub login",
                "Unit and end-to-end tests with Vitest and Playwright",
            ], min_len=0.9),
            item("Matchups", pos(D(11)), end_of(D(14)), "globe", ORANGE, [
                "Matchup pages built from past games, sorted by the opponents met most",
            ]),
        ]},
        {"title": "Process", "color": TEAL, "height": 0.6, "items": [
            item("PRDs and a decision log", pos(D(3)), end_of(D(23)), "lightbulb", TEAL, [
                "Every feature started as a PRD and shipped in numbered phases",
                "Decisions logged with IDs, past 200 by mid-July, and shipped PRDs archived",
                "A CLAUDE.md guide tells coding agents how to execute a PRD",
            ]),
        ]},
    ],
}

write(data, OUT_DIR / "shaco.json")
for lane in data["lanes"]:
    print(lane["title"])
    for it in lane["items"]:
        print(f"  {it['title']:<26} {it['start']:>5} {it['length']:>5} -> {round(it['start']+it['length'],2)}")
