---
title: Bookmarks
parent: API reference
nav_order: 1
---

# Bookmarks

## `GET /api/bookmarks`

Public. Returns every bookmark.

```json
[
  {
    "id": "a1b2c3",
    "title": "Just the Docs",
    "url": "https://just-the-docs.com",
    "description": "",
    "thumbnail": "",
    "tags": ["docs", "jekyll"],
    "folderId": "bookmarks-bar",
    "favorite": false,
    "dateAdded": "2026-07-01T12:00:00Z"
  }
]
```

## `GET /api/bookmarks/{id}`

Public. Returns a single bookmark, or `404 Not Found` if `id` doesn't exist.

## `POST /api/bookmarks`

**Requires `X-Api-Key`.** Creates a bookmark.

Request body:

```json
{
  "title": "Just the Docs",
  "url": "https://just-the-docs.com",
  "description": "",
  "thumbnail": "",
  "tags": ["docs", "jekyll"],
  "folderId": null,
  "favorite": false
}
```

| Field | Required | Notes |
| :---- | :------- | :---- |
| `title` | Yes | Non-empty |
| `url` | Yes | Absolute `http`/`https` URL |
| `description` | No | Defaults to `""` |
| `thumbnail` | No | Defaults to `""` |
| `tags` | No | Created on demand; trimmed and de-duplicated |
| `folderId` | No | Must reference an existing folder if set |
| `favorite` | No | Defaults to `false` |

Returns `201 Created` with the created bookmark and a `Location` header. Returns `400 Bad Request` if validation fails or `folderId` doesn't exist.

## `PUT /api/bookmarks/{id}`

**Requires `X-Api-Key`.** Replaces a bookmark's fields (same body shape as create). Returns `200 OK` with the updated bookmark, `404 Not Found` if `id` doesn't exist, or `400 Bad Request` on validation failure.

## `DELETE /api/bookmarks/{id}`

**Requires `X-Api-Key`.** Deletes a bookmark. Returns `204 No Content`, or `404 Not Found` if `id` doesn't exist.
