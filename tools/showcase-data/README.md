# Example roadmap data

These scripts write the example roadmaps in `wwwroot/showcase/`. Each item's position is
computed from real dates (from git history or my notes), so the dates stay on record
next to the layout.

```
python3 tools/showcase-data/gen_portfolio.py
python3 tools/showcase-data/gen_roadscript.py
python3 tools/showcase-data/gen_mggriebz.py
python3 tools/showcase-data/gen_shaco.py
```

Each script prints the lanes with item positions so overlaps are easy to spot.
`showcase_json.py` keeps columns and milestones on one line each, so the JSON view
in the app stays readable.

The JSON files are what the app loads. If you edit one by hand instead, the matching
script will overwrite that edit the next time it runs.

## How item text is written

An item's `description` starts with one summary line, then optional bullets:

```
Roadmaps you edit visually or as JSON
- Edit on the canvas or in the JSON, and both stay in sync
- Saved in your browser, nothing to sign up for
```

The timeline shows only the summary line, so keep it short: 50 characters or fewer, no
leading `- `, and no trailing period. The details panel and the List view show every bullet.
In the editor, bullets that don't fit an item are counted as "+N more".

`tests/RoadScript.Tests/ShowcaseDataTests.cs` checks every example: items stay inside
the timeline, each item starts with a short summary line, links and icons exist, and the
copy has no em dashes, TODOs or banned words.
To add an example, add its JSON file to `wwwroot/showcase/` and register it in `Services/ShowcaseService.cs`.
