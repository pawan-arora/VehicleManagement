# API contract (ASP.NET Core API ↔ React frontend)

This is the REST interface between the ASP.NET Core backend and the React frontend in `frontend/`. It's the only
thing the two share. Change both sides together.

- Base URL: `http://localhost:5080` in development. The frontend calls relative paths (`/api/...`), and its
  development server forwards them to this address, so the backend needs no frontend settings (no CORS).
- Content type: `application/json`, with camelCase property names (the ASP.NET Core default).

## Business rules the API enforces

The frontend runs the same checks so the user gets immediate feedback. The server is the authority.

- **Category ranges are lower-inclusive and upper-exclusive:** a vehicle belongs to the category where
  `minWeightKg <= weightKg < maxWeightKg`. A weight of exactly 500.00 kg is therefore **Medium**, not Light.
- The last category has `maxWeightKg: null`, meaning it has no upper bound.
- When sorted by `minWeightKg`, the categories must:
  - start at `0`;
  - be contiguous, so each `maxWeightKg` equals the next category's `minWeightKg`, which rules out gaps and overlaps;
  - have `minWeightKg < maxWeightKg`;
  - end with exactly one open-ended category.
- A vehicle's category is **calculated from its weight when read**, never stored, so it always reflects the current configuration.
- Weights are positive, with at most 2 decimal places. Years run from 1886 to the current year + 1.

## Errors

Failures return RFC 7807 problem details, the ASP.NET Core `ProblemDetails` / `ValidationProblemDetails`:

```json
{ "title": "One or more validation errors occurred.", "status": 400,
  "detail": "optional human-readable message",
  "errors": { "WeightKg": ["Weight must be greater than 0."] } }
```

| Status | When |
|---|---|
| 400 | Validation failure. `errors` keys are property names (for categories: `Name`, `MaxWeightKg`, and so on, or `Categories` for rules about all categories together, such as gaps) |
| 404 | The record doesn't exist |
| 500 | Unexpected error. `detail` must not contain stack traces |

## Endpoints

### `GET /api/manufacturers`
Returns the manufacturers in name order. Read-only: they're seeded by migration.
```json
[ { "id": 4, "name": "Ferrari" }, { "id": 3, "name": "Honda" } ]
```

### `GET /api/vehicles?sortBy={field}&sortDirection={asc|desc}`
`sortBy` is one of `ownerName`, `manufacturer`, `yearOfManufacture`, `weightKg`. The defaults are `ownerName` and `asc`.
Any other value gives `400` with `errors.SortBy` or `errors.SortDirection`. Vehicles with the same value are ordered
by id. Each vehicle's `category` is worked out from its weight when the list is read; its image is in
`GET /api/icons`, looked up by `icon`.
```json
[ {
    "id": 7, "ownerName": "John Smith",
    "manufacturer": { "id": 1, "name": "Mazda" },
    "yearOfManufacture": 2019, "weightKg": 1850.75,
    "category": { "id": 1002, "name": "Medium", "icon": "car" }
} ]
```

### `POST /api/vehicles`
```json
{ "ownerName": "John Smith", "manufacturerId": 1, "yearOfManufacture": 2019, "weightKg": 1850.75 }
```
Returns `201 Created` with the vehicle shape above. On failure, `400` with `errors` keyed by `OwnerName` (required,
at most 100 characters), `ManufacturerId` (required, must exist), `YearOfManufacture` (required, 1886 to next year)
or `WeightKg` (required, above 0, at most 2 decimal places).

### `GET /api/categories/all`
Returns the categories ordered by `minWeightKg`.
```json
[ { "id": 1001, "name": "Light",  "minWeightKg": 0,    "maxWeightKg": 500,  "icon": "motorcycle" },
  { "id": 1002, "name": "Medium", "minWeightKg": 500,  "maxWeightKg": 2500, "icon": "car" },
  { "id": 1003, "name": "Heavy",  "minWeightKg": 2500, "maxWeightKg": null, "icon": "truck" } ]
```

Each category also carries `iconSvg`, the icon image as SVG markup (left out above for brevity). It's
sent here, once per category, and not with every vehicle; a client looks the image up by `icon`.

### `GET /api/categories/findByWeight?weightKg={weight}`
Returns the one category a vehicle of that weight belongs to, in the same shape as an item of `GET /api/categories/all`
(including `iconSvg`). It uses the boundary rule above, so `weightKg=500` returns Medium.
```json
{ "id": 1002, "name": "Medium", "minWeightKg": 500, "maxWeightKg": 2500, "icon": "car", "iconSvg": "<svg ...>" }
```
`weightKg` follows the vehicle weight rules; if it's missing, not above 0, or has more than 2 decimal places, the
response is `400` with `errors.WeightKg`.

### Changing categories

Categories are changed one at a time. The server checks the resulting configuration against every rule above
before saving, and saves in one transaction. Because adding or deleting a category can move a neighbour's
boundary, **every change returns the full, saved list** (same shape as `GET /api/categories/all`). On failure
the response is `400`, with `errors` keyed by field (`Name`, `Icon`, `MinWeightKg`, `MaxWeightKg`) or
`Categories` for a rule about all categories together; `404` if the id doesn't exist.

#### `POST /api/categories`
Adds one category. Returns `201` with the full list.
```json
{ "name": "Super heavy", "minWeightKg": 3000, "maxWeightKg": null, "icon": "bus" }
```
The new range must fit inside **one** existing category and start or end where that category does; that
category is shortened to make room:
- Heavy is 2500+; adding 3000+ makes Heavy 2500–3000.
- Adding 2500–3000 makes Heavy 3000+.
- Adding 3000–3500 is rejected: it would split Heavy in two and leave 3500+ with no category.
- Adding 2000–3000 is rejected: it overlaps both Medium and Heavy.
- Adding exactly a category's range (500–2500) is rejected: that category would have no weights left.
- A name already used by another category (ignoring case) is rejected.

#### `PUT /api/categories/{id}`
Changes one category's **name, icon and range** (a null `maxWeightKg` means no upper limit). If the range
changes, the neighbouring categories move with it, so the ranges keep touching. Returns `200` with the full list.
```json
{ "name": "Medium", "icon": "car", "minWeightKg": 500, "maxWeightKg": 2500 }
```

#### `DELETE /api/categories/{id}`
Deletes one category. The **heavier neighbour** takes over its range (deleting Medium 500–2500 makes Heavy
start at 500). If the heaviest category is deleted, the lighter neighbour loses its upper limit instead. The
last remaining category can't be deleted. Returns `200` with the full list.

### Icons
`icon` is a key from a fixed set: `bicycle`, `motorcycle`, `car`, `van`, `truck`, `bus`, `tractor`. The API
rejects unknown keys.

#### `GET /api/icons`
Returns every icon a category can be given, ordered by key, with its image as SVG markup. A client uses it to
offer an icon picker. Icons are read-only: they're seeded by migration, and users can't add their own.
```json
[ { "key": "bicycle", "svg": "<svg ...>" }, { "key": "bus", "svg": "<svg ...>" } ]
```
