# Generates wwwroot/showcase/mggriebz.json: MgGriebZ.com's build history from its git log.
# Private areas of the site (family details, event pages for private occasions, money
# tracking) are left out on purpose.
from datetime import date
from showcase_json import OUT_DIR, write

PERIODS = [
    (date(2025, 6, 18), date(2025, 7, 1), 0, 1),
    (date(2025, 7, 1), date(2025, 10, 1), 1, 1),
    (date(2025, 10, 1), date(2026, 1, 1), 2, 1),
    (date(2026, 1, 1), date(2026, 4, 1), 3, 2),
    (date(2026, 4, 1), date(2026, 7, 1), 5, 2),
    (date(2026, 7, 1), date(2026, 10, 1), 7, 2),
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
    {"label": "Jun 2025", "sub": "101 commits"},
    {"label": "Q3 2025", "sub": "142 commits"},
    {"label": "Q4 2025", "sub": "48 commits"},
    {"label": "Q1 2026", "sub": "602 commits"}, {"label": "", "sub": ""},
    {"label": "Q2 2026", "sub": "392 commits"}, {"label": "", "sub": ""},
    {"label": "Q3 2026", "sub": "225 commits"}, {"label": "", "sub": ""},
    {"label": "Next", "sub": "Planned"},
]
assert len(cols) == TOTAL

ORANGE, TEAL, RED, BLUE, AMBER, GREY = "#D4652F", "#0F766E", "#EF4444", "#1E3A8A", "#D97706", "#94A3B8"
NOW = 9.0

data = {
    "title": "MgGriebZ.com: how it was built",
    "subtitle": "My personal site and the API behind my other projects, from the first commit on June 18, 2025. Column subtitles count commits.",
    "columns": cols,
    "milestones": [
        ms("Live", date(2025, 6, 20), "rocket", ORANGE),
        ms("Claude Code rebuild", date(2025, 12, 21), "flag", AMBER),
        ms("shaco.MgGriebZ.com", date(2026, 7, 3), "rocket", RED),
    ],
    "lanes": [
        {"title": "Site", "color": ORANGE, "height": 1.0, "items": [
            item("First version", pos(date(2025, 6, 18)), pos(date(2025, 9, 7)), "bookmark", ORANGE, [
                "React and Tailwind on Azure Static Web Apps, live two days after the first commit",
                "About, career and gallery pages, then share pages, hero slides and comparison views (July to August 2025)",
            ]),
            item("RoadScript page", pos(date(2025, 12, 21)), pos(date(2026, 1, 20)), "code", ORANGE, [
                "An editable RoadScript roadmap inside the site (December 2025), reworked in January 2026",
            ], min_len=1.0),
            item("Home and about pages", pos(date(2026, 3, 12)), pos(date(2026, 5, 17)), "star", ORANGE, [
                "New landing page with a hero carousel (March 2026)",
                "About pages rewritten, and a header with a five-day calendar strip (May 2026)",
            ]),
            item("Sister sites", pos(date(2026, 6, 29)), pos(date(2026, 7, 8)), "globe", ORANGE, [
                "The API also serves Mg Glass's catalog on mgglass.mggriebz.com (June 2026)",
                "It also serves shaco's stats and starts a shaco rebuild when new clips are logged (July 2026)",
            ], min_len=0.8),
            item("Rolling week", pos(date(2026, 8, 22)), pos(date(2026, 9, 21)), "calendar", ORANGE, [
                "Home page rebuilt around a rolling week of events, games and commits, from a design handoff (August to September 2026)",
                "Faster first load after fixing over-fetching on the home page",
            ], spanning=True, min_len=0.87),
        ]},
        {"title": "Calendar", "color": TEAL, "height": 1.0, "items": [
            item("Calendar", pos(date(2025, 12, 22)), pos(date(2026, 2, 1)), "calendar", TEAL, [
                "Calendar of life events with photos (December 2025)",
                "Commits from my GitHub repos on the same calendar (January 2026)",
                "Bulk photo uploads and a standalone calendar view (January 2026)",
            ], min_len=0.94),
            item("Shared events", pos(date(2026, 2, 8)), pos(date(2026, 3, 27)), "bell", TEAL, [
                "Guest views, notification subscriptions and photo uploads (February 2026)",
                "Shareable event pages (February 2026)",
                "Multi-month selection and quick-log templates (March 2026)",
            ]),
            item("Media search", pos(date(2026, 4, 20)), pos(date(2026, 5, 16)), "search", TEAL, [
                "Movies and shows looked up through the TMDB API, with poster images stored (April to May 2026)",
                "Weekly navigation and preview cards on the calendar (May 2026)",
            ], min_len=0.8),
        ]},
        {"title": "Game data", "color": RED, "height": 1.0, "items": [
            item("League matches", pos(date(2026, 2, 1)), pos(date(2026, 3, 21)), "chart", RED, [
                "League of Legends matches imported from the Riot API (February 2026)",
                "Richer stats and a win and loss view on the calendar (March 2026)",
            ]),
            item("Game logs", pos(date(2026, 5, 4)), pos(date(2026, 5, 30)), "square", RED, [
                "Game logs rebuilt, with batch import (May 2026)",
                "A written PRD for public game-log pages",
            ], min_len=0.8),
            item("Match data v2", pos(date(2026, 7, 8)), pos(date(2026, 7, 16)), "target", RED, [
                "A second version of the Riot match data and a game dossier view (July 2026)",
                "Feeds shaco, which rebuilds when new clips are logged here",
            ], min_len=0.8),
            item("Champion data", 9.05, 9.95, "flag", RED, [
                "League data for guides and clips across every champion, served to shaco",
            ]),
        ]},
        {"title": "API and admin", "color": BLUE, "height": 0.9, "items": [
            item("API and database", pos(date(2025, 6, 27)), pos(date(2025, 7, 29)), "gear", BLUE, [
                "A separate .NET API with Cosmos DB and Blob Storage (June 2025)",
                "Admin editing and image uploads (July 2025)",
            ], min_len=0.9),
            item("Admin overhaul", pos(date(2026, 1, 18)), pos(date(2026, 2, 18)), "wrench", BLUE, [
                "Image management for calendar events, and image storage reworked (January 2026)",
                "Admin page overhaul and write authorization on the API (February 2026)",
            ], min_len=0.9),
            item("Security pass", pos(date(2026, 7, 14)), pos(date(2026, 7, 16)), "lock", BLUE, [
                "Audited secrets exposed to the browser and moved every API secret server-side",
                "Secrets in Azure Key Vault; storage access through a managed identity instead of shared keys (July 2026)",
            ], min_len=0.95),
        ]},
        {"title": "How it's built", "color": AMBER, "height": 0.6, "items": [
            item("By hand", pos(date(2025, 6, 18)), pos(date(2025, 9, 7)), "code", GREY, [
                "About 240 commits written with help from AI chat and committed by hand",
            ]),
            item("Claude Code pull requests", pos(date(2025, 12, 21)), 10.0, "atom", AMBER, [
                "From December 2025, agents work from a written plan and open pull requests; I review and merge",
                "435 merged pull requests; about 1,200 commits authored by Claude",
            ], spanning=True),
        ]},
    ],
}

write(data, OUT_DIR / "mggriebz.json")
for lane in data["lanes"]:
    print(lane["title"])
    for it in lane["items"]:
        print(f"  {it['title']:<28} {it['start']:>5} {it['length']:>5} -> {round(it['start']+it['length'],2)}")
print([m["start"] for m in data["milestones"]])
