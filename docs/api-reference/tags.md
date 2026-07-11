---
title: Tags
parent: API reference
nav_order: 3
---

# Tags

## `GET /api/tags`

Public. Returns every tag name currently in use, sorted alphabetically.

```json
["docs", "jekyll", "reference"]
```

There is no dedicated write endpoint for tags — they are created implicitly when referenced by a bookmark's `tags` array (see [Bookmarks]({{ site.baseurl }}/api-reference/bookmarks/)). Tags are not garbage-collected when the last bookmark referencing them is deleted or updated; an unreferenced tag simply stops appearing in any bookmark's `tags` array but still shows up in this list.
