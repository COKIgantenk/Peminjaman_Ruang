using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PeminjamanRuangAPI.OpenApi
{
    public sealed class EndpointResponseOperationTransformer
        : IOpenApiOperationTransformer
    {
        public Task TransformAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            operation.Responses ??=
                new OpenApiResponses();

            var path =
                context.Description.RelativePath?
                    .TrimStart('/')
                    ?? string.Empty;

            var method =
                context.Description.HttpMethod?
                    .ToUpperInvariant()
                    ?? string.Empty;

            // =========================
            // AUTH
            // =========================

            if (path == "api/Auth/login" &&
                method == "POST")
            {
                AddResponse(
                    operation,
                    "401",
                    "Email atau password salah, atau akun tidak aktif.");

                AddResponse(
                    operation,
                    "429",
                    "Terlalu banyak percobaan login.");
            }

            if (path == "api/Auth/register" &&
                method == "POST")
            {
                AddResponse(
                    operation,
                    "409",
                    "Email sudah digunakan oleh user aktif.");

                AddResponse(
                    operation,
                    "429",
                    "Terlalu banyak percobaan registrasi.");
            }

            // =========================
            // BOOKING
            // =========================

            if ((path == "api/Booking" ||
                 path == "api/Booking/admin") &&
                method == "POST")
            {
                AddResponse(
                    operation,
                    "404",
                    "User atau room tidak ditemukan.");

                AddResponse(
                    operation,
                    "409",
                    "Room tidak tersedia karena booking atau maintenance.");
            }

            if (path == "api/Booking/{id}" &&
                method == "GET")
            {
                AddResponse(
                    operation,
                    "404",
                    "Booking tidak ditemukan.");
            }

            if (path == "api/Booking/{id}/approve" &&
                method == "PUT")
            {
                AddResponse(
                    operation,
                    "404",
                    "Booking atau room tidak ditemukan.");

                AddResponse(
                    operation,
                    "409",
                    "Booking tidak dapat disetujui karena conflict booking atau maintenance.");
            }

            if (path == "api/Booking/{id}/reject" &&
                method == "PUT")
            {
                AddResponse(
                    operation,
                    "404",
                    "Booking tidak ditemukan.");
            }

            if (path == "api/Booking/{id}/cancel" &&
                method == "PUT")
            {
                AddResponse(
                    operation,
                    "403",
                    "User tidak memiliki izin membatalkan booking tersebut.");

                AddResponse(
                    operation,
                    "404",
                    "Booking tidak ditemukan.");
            }

            // =========================
            // ROOM
            // =========================

            if (path == "api/Room/{id}" &&
                method is "GET" or "PUT" or "DELETE")
            {
                AddResponse(
                    operation,
                    "404",
                    "Room tidak ditemukan.");
            }

            if (path == "api/Room/{id}" &&
                method == "DELETE")
            {
                AddResponse(
                    operation,
                    "409",
                    "Room tidak dapat dinonaktifkan karena sedang digunakan, maintenance, atau cleaning.");
            }

            if (path ==
                    "api/Room/{roomId}/facilities/{facilityId}" &&
                method is "POST" or "DELETE")
            {
                AddResponse(
                    operation,
                    "404",
                    "Room atau facility tidak ditemukan.");

                AddResponse(
                    operation,
                    "409",
                    "Relasi room dan facility mengalami conflict.");
            }

            // =========================
            // FACILITY
            // =========================

            if (path == "api/Facility/{id}" &&
                method is "GET" or "PUT" or "DELETE")
            {
                AddResponse(
                    operation,
                    "404",
                    "Facility tidak ditemukan.");
            }

            if ((path == "api/Facility" &&
                 method == "POST") ||
                (path == "api/Facility/{id}" &&
                 method is "PUT" or "DELETE"))
            {
                AddResponse(
                    operation,
                    "409",
                    "Facility mengalami conflict dengan data yang sudah ada atau relasi lain.");
            }

            // =========================
            // DEPARTMENT
            // =========================

            if (path == "api/Department" &&
                method == "POST")
            {
                AddResponse(
                    operation,
                    "409",
                    "Department dengan nama yang sama sudah tersedia.");
            }

            // =========================
            // MAINTENANCE
            // =========================

            if (path == "api/Maintenance" &&
                method == "POST")
            {
                AddResponse(
                    operation,
                    "404",
                    "Room tidak ditemukan.");

                AddResponse(
                    operation,
                    "409",
                    "Jadwal maintenance conflict dengan maintenance atau booking.");
            }

            if (path == "api/Maintenance/{id}" &&
                method == "GET")
            {
                AddResponse(
                    operation,
                    "404",
                    "Maintenance tidak ditemukan.");
            }

            if (path == "api/Maintenance/{id}/complete" &&
                method == "PUT")
            {
                AddResponse(
                    operation,
                    "404",
                    "Maintenance tidak ditemukan.");

                AddResponse(
                    operation,
                    "409",
                    "Maintenance tidak dapat diselesaikan pada state saat ini.");
            }

            // =========================
            // CLEANING
            // =========================

            if (path == "api/RoomCleaning/{id}" &&
                method == "GET")
            {
                AddResponse(
                    operation,
                    "404",
                    "Cleaning session tidak ditemukan.");
            }

            if (path == "api/RoomCleaning/{id}/duration" &&
                method == "PUT")
            {
                AddResponse(
                    operation,
                    "404",
                    "Cleaning session tidak ditemukan.");

                AddResponse(
                    operation,
                    "409",
                    "Cleaning session sudah selesai atau tidak dapat diperbarui.");
            }

            // =========================
            // ROOM STATUS
            // =========================

            if (path.StartsWith(
                    "api/RoomStatus/{roomId}",
                    StringComparison.Ordinal))
            {
                AddResponse(
                    operation,
                    "404",
                    "Room atau riwayat status tidak ditemukan.");
            }

            if (path == "api/RoomStatus/{roomId}" &&
                method == "PUT")
            {
                AddResponse(
                    operation,
                    "409",
                    "Status room tidak dapat diubah karena lifecycle room saat ini.");
            }

            // =========================
            // USER MANAGEMENT
            // =========================

            if (path == "api/User/{id}" &&
                method is "GET" or "PUT" or "DELETE")
            {
                AddResponse(
                    operation,
                    "404",
                    "User tidak ditemukan.");
            }

            if ((path == "api/User" &&
                 method == "POST") ||
                (path == "api/User/{id}/restore" &&
                 method == "POST"))
            {
                AddResponse(
                    operation,
                    "409",
                    "Email sudah digunakan oleh akun lain.");
            }

            if (path == "api/User/{id}/restore" &&
                method == "POST")
            {
                AddResponse(
                    operation,
                    "404",
                    "User yang sudah dihapus tidak ditemukan.");
            }

            return Task.CompletedTask;
        }

        private static void AddResponse(
            OpenApiOperation operation,
            string statusCode,
            string description)
        {
            operation.Responses ??=
                new OpenApiResponses();

            if (operation.Responses.ContainsKey(statusCode))
            {
                return;
            }

            operation.Responses[statusCode] =
                new OpenApiResponse
                {
                    Description = description
                };
        }
    }
}