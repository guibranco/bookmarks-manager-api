---
title: API reference
nav_order: 4
has_children: true
---

# API reference

Base URL: **`https://api.bookmarks.straccini.com`** in production, `http://localhost:5000` (or whichever port is printed at startup) locally.

All request/response bodies are JSON. All error responses are [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807) `application/problem+json`.

| Resource | Reference |
| :------- | :-------- |
| [Bookmarks]({{ site.baseurl }}/api-reference/bookmarks/) | `/api/bookmarks` |
| [Folders]({{ site.baseurl }}/api-reference/folders/) | `/api/folders` |
| [Tags]({{ site.baseurl }}/api-reference/tags/) | `/api/tags` |
| [Health]({{ site.baseurl }}/api-reference/health/) | `/health` |

An interactive, always-up-to-date version of this reference is served by the API itself in development at `/swagger`.

## Authentication

`GET` requests never require authentication. `POST`, `PUT` and `DELETE` requests require an `X-Api-Key` header — see [Security]({{ site.baseurl }}/architecture/security/) for details.

```http
X-Api-Key: your-secret-key
```

Missing or invalid keys return `401 Unauthorized`.
