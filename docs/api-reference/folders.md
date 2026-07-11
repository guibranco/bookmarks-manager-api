---
title: Folders
parent: API reference
nav_order: 2
---

# Folders

## `GET /api/folders`

Public. Returns every folder (flat list; build the tree client-side using `parentId`).

```json
[
  { "id": "bookmarks-bar", "name": "Bookmarks bar", "parentId": null, "icon": null }
]
```

## `GET /api/folders/{id}`

Public. Returns a single folder, or `404 Not Found` if `id` doesn't exist.

## `POST /api/folders`

**Requires `X-Api-Key`.** Creates a folder.

```json
{ "name": "Reading list", "parentId": null, "icon": null }
```

| Field | Required | Notes |
| :---- | :------- | :---- |
| `name` | Yes | Non-empty |
| `parentId` | No | Must reference an existing folder if set |
| `icon` | No | Free-form string, blank values stored as `null` |

Returns `201 Created`, or `400 Bad Request` if validation fails or `parentId` doesn't exist.

## `PUT /api/folders/{id}`

**Requires `X-Api-Key`.** Updates a folder's fields.

- Returns `400 Bad Request` if the new `parentId` would make the folder its own ancestor (cycle detection).
- Returns `404 Not Found` if `id` doesn't exist.

## `DELETE /api/folders/{id}?cascade={bool}`

**Requires `X-Api-Key`.** Deletes a folder.

- If the folder has bookmarks or subfolders and `cascade` is not `true`, returns `409 Conflict`.
- If `cascade=true`: bookmarks in the folder have their `folderId` set to `null`, and subfolders are re-parented to this folder's own parent, before the folder is deleted.
- Returns `204 No Content` on success, `404 Not Found` if `id` doesn't exist.
