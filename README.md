# Imperium - Система управления мебельным магазином

Полнофункциональная система для управления интернет-магазином мебели с административной панелью, API для клиентов и интеграциями с внешними сервисами.

## 🏗️ Архитектура проекта

Проект построен на архитектуре Clean Architecture с разделением на слои:

- **Imperium.Core** - доменные модели и интерфейсы
- **Imperium.Data** - репозитории и доступ к данным  
- **Imperium.Service** - бизнес-логика и сервисы
- **Imperium.Web** - API контроллеры и веб-слой

## 🚀 Возможности

### Для клиентов:
- 📱 Регистрация/авторизация по номеру телефона с OTP
- 🛍️ Каталог товаров с фильтрацией и поиском
- 🛒 Корзина покупок
- ❤️ Избранные товары
- 📦 История заказов
- 📍 Управление адресами доставки
- ⭐ Отзывы и рейтинги

### Для администраторов:
- 👥 Управление пользователями и ролями
- 📦 Управление товарами и категориями
- 📊 Аналитика и отчеты
- 🖼️ Загрузка и управление изображениями
- 📋 Обработка заказов
- 💬 Система уведомлений (Telegram, WhatsApp)

## 🛠️ Технологический стек

- **Backend**: ASP.NET Core 8.0
- **База данных**: MySQL 8.0
- **Миграции**: Liquibase
- **Контейнеризация**: Docker & Docker Compose
- **Файловое хранилище**: Cloudinary
- **Уведомления**: WhatsApp Business API, Telegram Bot
- **Авторизация**: JWT Bearer Tokens
- **Документация API**: Swagger/OpenAPI

## 📋 Требования

### Обязательные:
- [Docker](https://www.docker.com/get-started) (v20.10 или выше)
- [Docker Compose](https://docs.docker.com/compose/install/) (v2.0 или выше)

### Для ручного запуска:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL 8.0](https://dev.mysql.com/downloads/mysql/)
- [Java 8+](https://www.oracle.com/java/technologies/downloads/) (для Liquibase)

## 🚀 Быстрый старт с Docker

### 1. Клонируйте репозиторий
```bash
git clone <repository-url>
cd imperium
```

### 2. Настройте конфигурацию
Скопируйте файл настроек и отредактируйте его:
```bash
cp ImperiumBack/sourses/Imperium.Web/appsettings.json ImperiumBack/sourses/Imperium.Web/appsettings.Development.json
```

### 3. Запустите проект
```bash
docker-compose up -d
```

Это запустит:
- MySQL сервер на порту 3306
- Backend API на порту 5000
- Автоматические миграции базы данных

### 4. Проверьте статус
```bash
docker-compose ps
```

### 5. Откройте приложение
- **API Swagger документация**: http://localhost:5000/swagger
- **Базовый API endpoint**: http://localhost:5000/api

## ⚙️ Конфигурация

### Основные настройки (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=imperiumdb;user=root;password=yourpassword;"
  },
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key",
    "Issuer": "Imperium",
    "Audience": "ImperiumUsers",
    "ExpiryInDays": 7
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key", 
    "ApiSecret": "your-api-secret"
  },
  "WhatsApp": {
    "ApiUrl": "https://graph.facebook.com/v22.0/YOUR_PHONE_ID/messages",
    "AccessToken": "your-whatsapp-token",
    "AdminPhone": "+77781277546"
  },
  "Telegram": {
    "BotToken": "your-telegram-bot-token",
    "ChatId": "your-chat-id"
  }
}
```

### Обязательные настройки для продакшена:

1. **JWT Secret** - смените на надежный ключ (минимум 256 бит)
2. **Cloudinary** - получите credentials на [Cloudinary](https://cloudinary.com/)
3. **WhatsApp Business API** - настройте через [Facebook Developers](https://developers.facebook.com/)
4. **Telegram Bot** - создайте бота через [@BotFather](https://t.me/BotFather)

## 🗄️ База данных

### Автоматическая настройка с Docker
При запуске через docker-compose база данных настраивается автоматически.

### Ручная настройка базы данных

1. **Создайте базу данных MySQL:**
```sql
CREATE DATABASE imperiumdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'imperium'@'localhost' IDENTIFIED BY 'imperium';
GRANT ALL PRIVILEGES ON imperiumdb.* TO 'imperium'@'localhost';
FLUSH PRIVILEGES;
```

2. **Запустите миграции Liquibase:**
```bash
cd ImperiumBack/database
java -jar liquibase/liquibase.jar --defaultsFile=env-local.properties update
```

### Пользователь по умолчанию
После настройки БД создается администратор:
- **Email**: admin@imperium.kz  
- **Пароль**: admin123 (хэшированный в БД)

## 🔧 Ручной запуск для разработки

### 1. Установите зависимости
```bash
cd ImperiumBack/sourses
dotnet restore
```

### 2. Настройте базу данных
- Запустите MySQL сервер
- Выполните миграции Liquibase (см. раздел выше)

### 3. Запустите backend
```bash
cd ImperiumBack/sourses/Imperium.Web
dotnet run
```

API будет доступно на: http://localhost:5000

## 📡 API Документация

### Основные endpoints:

#### Авторизация клиентов
- `POST /api/client/register-or-login` - Регистрация/вход по телефону
- `POST /api/client/verify-otp` - Подтверждение OTP кода
- `POST /api/client/resend-otp` - Повторная отправка OTP

#### Товары
- `GET /api/products` - Список всех товаров
- `GET /api/products/{id}` - Товар по ID
- `GET /api/products/category/{categoryId}` - Товары по категории
- `GET /api/products/search/{searchTerm}` - Поиск товаров

#### Корзина
- `GET /api/cart/{clientId}` - Корзина клиента
- `POST /api/cart/{clientId}` - Добавить в корзину
- `PUT /api/cart/{clientId}/item/{cartId}` - Обновить товар в корзине
- `DELETE /api/cart/{clientId}/item/{cartId}` - Удалить из корзины

#### Избранное
- `GET /api/favorite/{clientId}` - Избранные товары
- `POST /api/favorite/{clientId}` - Добавить в избранное
- `DELETE /api/favorite/{clientId}/product/{productId}` - Удалить из избранного

#### Заказы (для администраторов)
- `GET /api/admin/orders` - Список заказов
- `GET /api/admin/orders/{id}` - Детали заказа
- `PUT /api/admin/orders/{id}/status` - Изменить статус заказа

### Swagger документация
Полная документация API доступна по адресу: http://localhost:5000/swagger

## 🗂️ Структура проекта

```
ImperiumBack/
├── sourses/
│   ├── Imperium.Core/          # Доменные модели
│   │   ├── Models/             # Модели данных
│   │   ├── Enums/              # Перечисления
│   │   └── Constants.cs        # Константы приложения
│   ├── Imperium.Data/          # Слой данных
│   │   ├── Repositories/       # Репозитории
│   │   └── Connections/        # Подключения к БД
│   ├── Imperium.Service/       # Бизнес-логика
│   │   ├── Services/           # Сервисы
│   │   ├── DTOs/               # Объекты передачи данных
│   │   └── Mapping/            # AutoMapper профили
│   └── Imperium.Web/           # Web API
│       ├── Controllers/        # API контроллеры
│       ├── Controllers/Admin/  # Админские контроллеры
│       └── Program.cs          # Точка входа
├── database/
│   ├── changelog/              # Liquibase миграции
│   ├── liquibase/              # Liquibase JAR файлы
│   ├── driver/                 # JDBC драйверы
│   └── *.properties            # Конфиги подключения
└── README.md                   # Документация БД
```

## 🔍 Устранение неполадок

### Проблемы с Docker

**Контейнер базы данных не запускается:**
```bash
# Проверьте логи
docker-compose logs db

# Пересоздайте контейнеры
docker-compose down -v
docker-compose up -d
```

**Backend не может подключиться к БД:**
```bash
# Проверьте, что БД готова к подключениям
docker-compose logs db | grep "ready for connections"

# Перезапустите backend
docker-compose restart backend
```

### Проблемы с миграциями

**Ошибки Liquibase:**
```bash
# Проверьте логи миграции
docker-compose logs liquibase

# Запустите миграции вручную
docker-compose run liquibase --url=jdbc:mysql://db:3306/imperiumdb --username=imperium --password=imperium --changelog-file=changelog/0-db.changelog-master.xml update
```

### Проблемы с файлами

**Cloudinary не работает:**
1. Проверьте правильность credentials в appsettings.json
2. Убедитесь, что у аккаунта Cloudinary есть квота для загрузок

### Проблемы с уведомлениями

**WhatsApp уведомления не отправляются:**
1. Проверьте токен WhatsApp Business API
2. Убедитесь, что номер телефона верифицирован в Facebook Business

**Telegram уведомления не работают:**
1. Проверьте токен бота
2. Убедитесь, что бот добавлен в чат/канал

## 🔐 Безопасность

### Продакшен настройки:
1. Смените JWT secret на криптографически стойкий ключ
2. Используйте HTTPS для всех запросов
3. Настройте firewall для ограничения доступа к БД
4. Регулярно обновляйте зависимости
5. Включите логирование безопасности

### Управление секретами:
- Используйте переменные окружения для чувствительных данных
- Никогда не коммитьте production credentials в Git
- Рассмотрите использование Azure Key Vault или AWS Secrets Manager

## 📚 Дополнительные ресурсы

- [ASP.NET Core документация](https://docs.microsoft.com/aspnet/core/)
- [Liquibase документация](https://docs.liquibase.com/)
- [Cloudinary API документация](https://cloudinary.com/documentation)
- [WhatsApp Business API](https://developers.facebook.com/docs/whatsapp)
- [Telegram Bot API](https://core.telegram.org/bots/api)

## 🤝 Поддержка

Для получения поддержки:
1. Проверьте раздел "Устранение неполадок" выше
2. Изучите логи приложения: `docker-compose logs backend`
3. Создайте issue в репозитории с подробным описанием проблемы

## 📄 Лицензия

Этот проект является частной разработкой. Все права защищены.