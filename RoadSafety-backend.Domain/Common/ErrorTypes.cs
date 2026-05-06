namespace RoadSafety_backend.Domain.Common;

public enum ErrorType
{
    Failure = 0,         // Общая ошибка
    Validation = 1,      // Ошибка валидации (400)
    Unauthorized = 2,    // Не авторизован (401)
    NotFound = 3,        // Не найдено (404)
    Conflict = 4,        // Конфликт данных (409)
    Forbidden = 5        // Нет прав доступа (403)
}