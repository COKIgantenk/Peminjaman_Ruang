# Peminjaman Ruang API — Frontend Integration Contract

## 1. Overview

Backend menyediakan REST API untuk sistem Peminjaman Ruang.

API documentation tersedia melalui:

- Swagger UI (Development): `/swagger`
- OpenAPI document: `/openapi/v1.json`
- Exported OpenAPI contract: `Docs/openapi-v1.json`

Frontend harus menggunakan OpenAPI/Swagger sebagai sumber utama untuk melihat:
- request schema
- response schema
- parameter
- HTTP status code
- authentication requirement

---

## 2. Base URL

### Local Development

`https://localhost:5074`

### Production

Akan diisi setelah backend berhasil dideploy.

Frontend tidak boleh melakukan hardcode base URL di setiap API call.

Gunakan environment variable frontend, misalnya:

```text
VITE_API_BASE_URL=https://localhost:5074