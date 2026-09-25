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

`tests/RoadScript.Tests/ShowcaseDataTests.cs` checks every example: items stay inside
the timeline, links and icons exist, and the copy has no em dashes, TODOs or banned words.
To add an example, add its JSON file to `wwwroot/showcase/` and register it in `Services/ShowcaseService.cs`.
