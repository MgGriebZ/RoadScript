# Generates wwwroot/showcase/portfolio.json from dated facts.
# Scale: 16 column units. "Earlier" (Jun 2024 to Mar 2025) is 1 unit, each quarter from
# Q2 2025 to Q2 2026 is 2 units, Q3 2026 is 3 units, and Next is 2 units.
import json, calendar
from showcase_json import OUT_DIR, write
from datetime import date

def pos(d):
    if d < date(2025, 4, 1):
        return (d - date(2024, 6, 1)).days / 304
    if d < date(2026, 7, 1):
        months = (d.year - 2025) * 12 + d.month - 4 + (d.day - 1) / calendar.monthrange(d.year, d.month)[1]
        return 1 + 2 * months / 3
    if d < date(2026, 10, 1):
        months = d.month - 7 + (d.day - 1) / calendar.monthrange(d.year, d.month)[1]
        return 11 + months
    raise ValueError(d)

NOW = 14.0      # end of Q3 2026
NEXT = 14.05    # planned items start just inside the Next column
END = 16.0

# One short lead sentence per item: the timeline shows only this line
SUMMARIES = {
    'Action Tarot': 'Gen-AI tarot journal with a social layer',
    'MgGriebZ.com': 'Personal hub and the API behind my other sites',
    'RoadScript': 'Roadmaps you edit visually or as JSON',
    'Public launch': 'Launch on LinkedIn and Reddit',
    'Custom sites': 'Three custom sites, then a public template',
    'CosmoScope': 'Frisbee golf or the telescope tonight?',
    'HPP Glass': 'Portfolio and admin for two glass artists',
    'shaco': 'AP Shaco support guide with clips',
    'Guides and clips': 'Every champion and role',
    'Sacred Sigils': 'Meditative sigil app',
    'FlowForge': 'A game about workplace conversations',
    'Rehoboam': 'Does AI news move markets?',
    'Core Clock': 'Third-person arena game in Godot',
    'Core Clock on itch.io': 'Single-player release',
    'AI chat': 'Drafted in chat, committed by hand',
    'Claude Code on the web': 'Agents open pull requests for review',
    'Local agents': 'Codex and local Claude Code',
}

def item(title, start, end, icon, color, desc, spanning=False, greyed=False, link=None, min_len=None):
    s = round(start, 2)
    length = round(round(end, 2) - s, 2)
    if min_len and length < min_len:
        length = min_len
    it = {"title": title, "start": s, "length": length}
    if spanning: it["spanning"] = True
    it["icon"] = icon
    it["color"] = color
    if greyed: it["greyed"] = True
    if link: it["linkedRoadmapId"] = link
    it["description"] = SUMMARIES[title] + "\n" + "\n".join("- " + b for b in desc)
    return it

def planned(title, icon, color, desc):
    return item(title, NEXT, NEXT + 1.9, icon, color, desc)

cols = [{"label": "Earlier", "sub": "Jun 2024 to Mar 2025"}]
for label, sub in [("Q2 2025", "Apr to Jun"), ("Q3 2025", "Jul to Sep"), ("Q4 2025", "Oct to Dec"),
                   ("Q1 2026", "Jan to Mar"), ("Q2 2026", "Apr to Jun")]:
    cols += [{"label": label, "sub": sub}, {"label": "", "sub": ""}]
cols += [{"label": "Q3 2026", "sub": "Jul to Sep"}, {"label": "", "sub": ""}, {"label": "", "sub": ""}]
cols += [{"label": "Next", "sub": "Planned"}, {"label": "", "sub": ""}]
assert len(cols) == 16

def ms(title, d, color):
    return {"start": round(pos(d) / 16 * 100, 2), "title": title, "icon": "rocket", "color": color}

data = {
    "title": "Matt Griebel: products and projects",
    "subtitle": "Rockets mark public launches. Dashed borders are in progress. Grey items are retired or paused. The Next column holds plans.",
    "columns": cols,
    "milestones": [
        ms("Action-Tarot.com", date(2024, 7, 15), "#45B69C"),
        ms("MgGriebZ.com", date(2025, 6, 18), "#D4652F"),
        ms("RoadScript.NET", date(2025, 11, 25), "#1E3A8A"),
        ms("shaco.MgGriebZ.com", date(2026, 7, 3), "#EF4444"),
    ],
    "lanes": [
        {"title": "Products", "color": "#1E3A8A", "height": 1.5, "items": [
            item("Action Tarot", 0.0, pos(date(2025, 5, 29)), "calendar", "#45B69C", [
                "AI readings with generated images, logged to Cosmos DB to build a working personality of the tarot, the user and the AI",
                "Grew into a social platform: public and private readings, votes on public cards, comments, Azure AD B2C sign-in",
                ".NET MVC version live in July 2024, rebuilt in Blazor WebAssembly over the 2024 holidays",
                "Went dormant because I had built more than I planned to market. A 2026 rework into a tarot auto-battler paused after three days",
            ], greyed=True),
            item("MgGriebZ.com", pos(date(2025, 6, 18)), NOW, "bookmark", "#D4652F", [
                "Life-events calendar with photos, League of Legends match history from the Riot API, GitHub commit tracking",
                "One API behind it: also serves shaco's stats and Mg Glass's catalog, and hosts one-off event pages",
                "React and .NET 8 on Azure (Cosmos DB, Blob Storage)",
                "1,500+ commits and 435 merged pull requests since June 2025",
            ], spanning=True, link="showcase:mggriebz"),
            item("RoadScript", pos(date(2025, 11, 25)), NOW, "code", "#1E3A8A", [
                "Built after a Fortune 500 bank client asked for development roadmaps during an onsite visit, and live within a week",
                "Refined through daily use at work: developers as columns, lanes for committed and blocked work, used in refinement and planning",
                "Visual editor for stakeholders, JSON editor for technical users; runs in the browser with no backend",
                "330+ commits and 81 merged pull requests",
            ], spanning=True, link="showcase:roadscript"),
            planned("Public launch", "flag", "#1E3A8A", [
                "New landing page and example roadmaps, including this one",
            ]),
        ]},
        {"title": "Websites", "color": "#F88379", "height": 1.0, "items": [
            item("Custom sites", pos(date(2025, 5, 31)), pos(date(2025, 8, 30)), "wrench", "#8F8FE8", [
                "AshMe Glass, then Captain Hook Glass Art, then Sassy Cakes, from May to August 2025",
                "Each added a piece: React on Azure Static Web Apps, a .NET API, an admin CMS, Cosmos DB and Blob Storage",
                "Packaged into a public full-stack site template that shaped MgGriebZ.com",
            ]),
            item("CosmoScope", pos(date(2025, 12, 1)), pos(date(2026, 4, 29)), "globe", "#6366F1", [
                "A weather-based helper for choosing between the two",
                "A .NET MAUI app in December 2025 and January 2026, then a web app, live on Azure since April 2026",
            ]),
            item("HPP Glass", pos(date(2026, 4, 30)), pos(date(2026, 7, 2)), "star", "#F88379", [
                "Still live",
                "Built for volume: bulk uploads of hundreds of photos, sorting, categories and rotating featured pieces",
                "The best parts of the earlier custom sites, with a server-side Functions API",
                "Led to Mg Glass in June 2026: a planner for my own lampworking studio at mgglass.mggriebz.com, parked until the studio budget is ready",
            ]),
            item("shaco", pos(date(2026, 7, 3)), pos(date(2026, 8, 1)), "square", "#EF4444", [
                "A League of Legends field guide built from MgGriebZ.com game data",
                "Practice for making video: a repeatable path from replay, to a 5 to 10 second clip, to a guide entry",
                "Clip controls: several paused side by side, full screen, slow and fast playback",
                "Astro static site with a written PRD and decision log; 159 commits in July 2026",
            ], link="showcase:shaco", min_len=1.3),
            planned("Guides and clips", "flag", "#EF4444", [
                "Broaden shaco into guides and clips for every champion and role, as a slice of the League data on MgGriebZ.com",
            ]),
        ]},
        {"title": "Apps and games", "color": "#7C4A1E", "height": 1.0, "items": [
            item("Sacred Sigils", pos(date(2025, 5, 1)), pos(date(2025, 6, 30)), "star", "#9999ff", [
                "Roll a sigil from six traditions and see how it echoes the others",
                "Built in Flutter in May and June 2025; the 120 sigil artworks and release prep followed in April and May 2026",
                "Paused just before an app store release",
            ], greyed=True),
            item("FlowForge", pos(date(2025, 9, 29)), pos(date(2026, 2, 18)), "lightbulb", "#8B9A8B", [
                "Turn-based game about workplace conversations: buzzwords, emotions and roles (IC, manager, exec) decide when to agree, challenge, ask or defer",
                "Scenario builder for replaying conversations",
                "Built from September 2025 to February 2026, then paused before reaching production",
            ], greyed=True),
            item("Rehoboam", pos(date(2026, 4, 10)), pos(date(2026, 7, 10)), "chart", "#94A3B8", [
                "Reddit and news sweeps classified by Claude",
                "Sector visuals showing how AI companies affect wider markets",
                "Idea stage",
            ], greyed=True),
            item("Core Clock", pos(date(2026, 7, 23)), NOW, "clock", "#7C4A1E", [
                "Pick rogue, mage, warrior or hunter and move pucks into sockets to fix the Core's clock",
                "Movement and abilities inspired by World of Warcraft arena",
                "Started in Unity, moved to Godot after a week",
                "690+ commits in two months",
            ], spanning=True),
            planned("Core Clock on itch.io", "flag", "#7C4A1E", [
                "Once the game feels right. The itch.io page is set up and private",
            ]),
        ]},
        {"title": "How I build", "color": "#0F766E", "height": 0.65, "items": [
            item("AI chat", 0.0, pos(date(2025, 11, 5)), "code", "#94A3B8", [
                "First Codex pull request in June 2025",
            ]),
            item("Claude Code on the web", pos(date(2025, 11, 5)), pos(date(2026, 7, 16)), "atom", "#D97706", [
                "From November 2025, agents work from a written plan",
                "More than 90% of commits in this period are authored by Claude",
            ]),
            item("Local agents", pos(date(2026, 7, 16)), END, "gear", "#0F766E", [
                "Codex from July 2026, local Claude Code from August",
                "Core Clock needs them: game assets, tuning and MCP tools live on my machine, and work runs in planned execution slices",
                "Pocket Playroom, a digital playroom my kids use on a tablet, was built with Codex over a weekend in July 2026",
            ], spanning=True),
        ]},
    ],
}

out = OUT_DIR / "portfolio.json"
write(data, out)

for lane in data["lanes"]:
    print(lane["title"])
    for it in lane["items"]:
        print(f"  {it['title']:<24} {it['start']:>6} {it['length']:>6} -> {round(it['start']+it['length'],2)}")
print([m["start"] for m in data["milestones"]])
