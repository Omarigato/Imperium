-- Включаем расширения
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enum типы
CREATE TYPE UserRole AS ENUM ('Client', 'Admin');
CREATE TYPE OrderStatus AS ENUM ('Pending', 'SentToAdmin', 'Confirmed', 'Cancelled');
CREATE TYPE VerificationType AS ENUM ('Email', 'Phone');

-- Таблица пользователей
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    FullName VARCHAR(255) NOT NULL,
    Phone VARCHAR(20) UNIQUE,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Role UserRole DEFAULT 'Client',
    IsEmailVerified BOOLEAN DEFAULT FALSE,
    IsPhoneVerified BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    DeletedAt TIMESTAMP NULL
);

-- Универсальная таблица справочников
CREATE TABLE Dictionaries (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Type VARCHAR(50) NOT NULL, -- 'Categories', 'Materials', 'Colors', 'Sizes', 'Weights'
    NameRu VARCHAR(255) NOT NULL,
    NameKz VARCHAR(255) NOT NULL,
    Code VARCHAR(50) UNIQUE NOT NULL,
    Value VARCHAR(100), -- для размеров, весов, hex для цветов
    DescriptionRu TEXT,
    DescriptionKz TEXT,
    ParentId UUID REFERENCES Dictionaries(Id), -- для иерархии категорий
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица файлов
CREATE TABLE Files (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Url TEXT NOT NULL,
    PublicId TEXT NOT NULL,
    FileName TEXT NOT NULL,
    Size INTEGER NOT NULL,
    MimeType TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица товаров
CREATE TABLE Products (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    CategoryId UUID REFERENCES Dictionaries(Id) NOT NULL,
    MaterialId UUID REFERENCES Dictionaries(Id),
    NameRu VARCHAR(255) NOT NULL,
    NameKz VARCHAR(255) NOT NULL,
    Code VARCHAR(50) UNIQUE NOT NULL,
    DescriptionRu TEXT,
    DescriptionKz TEXT,
    Price DECIMAL(10,2) NOT NULL,
    IsAvailable BOOLEAN DEFAULT true,
    IsFeatured BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Связь товаров с файлами
CREATE TABLE ProductFiles (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ProductId UUID NOT NULL REFERENCES Products(Id) ON DELETE CASCADE,
    FileId UUID NOT NULL REFERENCES Files(Id) ON DELETE CASCADE,
    IsAddition BOOLEAN DEFAULT false
);

-- Связь товаров с доступными цветами
CREATE TABLE ProductColors (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ProductId UUID REFERENCES Products(Id) ON DELETE CASCADE,
    ColorId UUID REFERENCES Dictionaries(Id),
    IsAvailable BOOLEAN DEFAULT true
);

-- Связь товаров с доступными размерами
CREATE TABLE ProductSizes (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ProductId UUID REFERENCES Products(Id) ON DELETE CASCADE,
    SizeId UUID REFERENCES Dictionaries(Id),
    IsAvailable BOOLEAN DEFAULT true
);

-- Таблица избранного
CREATE TABLE Favorites (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    ProductId UUID REFERENCES Products(Id) ON DELETE CASCADE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(UserId, ProductId)
);

-- Таблица корзины
CREATE TABLE Carts (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    ProductId UUID REFERENCES Products(Id) ON DELETE CASCADE,
    Quantity INTEGER DEFAULT 1 CHECK (Quantity > 0),
    SelectedColorId UUID REFERENCES Dictionaries(Id),
    SelectedSizeId UUID REFERENCES Dictionaries(Id),
    Notes TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (UserId, ProductId)
);

-- Таблица адресов
CREATE TABLE Addresses (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    Title VARCHAR(100),
    City VARCHAR(100) NOT NULL,
    Street VARCHAR(255) NOT NULL,
    HouseNumber VARCHAR(20) NOT NULL,
    Apartment VARCHAR(20),
    Notes TEXT,
    IsDefault BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица заказов
CREATE TABLE Orders (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    OrderNumber VARCHAR(50) UNIQUE NOT NULL,
    UserId UUID REFERENCES Users(Id),
    DeliveryAddressId UUID REFERENCES Addresses(Id),
    Status OrderStatus DEFAULT 'Pending',
    TotalAmount DECIMAL(10,2) NOT NULL,
    DeliveryFee DECIMAL(8,2) DEFAULT 0,
    Notes TEXT,
    AdminNotes TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица позиций заказа
CREATE TABLE OrderItems (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    OrderId UUID REFERENCES Orders(Id) ON DELETE CASCADE,
    ProductId UUID REFERENCES Products(Id),
    Quantity INTEGER NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    SelectedColorId UUID REFERENCES Dictionaries(Id),
    SelectedSizeId UUID REFERENCES Dictionaries(Id),
    ItemNotes TEXT
);

-- Таблица отзывов
CREATE TABLE Reviews (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID REFERENCES Users(Id),
    ProductId UUID REFERENCES Products(Id) ON DELETE CASCADE,
    Rating INTEGER CHECK (Rating >= 1 AND Rating <= 5),
    Comment TEXT,
    Photos JSON,
    IsVerified BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(UserId, ProductId)
);

-- Таблица верификации
CREATE TABLE Verifications (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    Type VerificationType NOT NULL,
    Contact VARCHAR(255) NOT NULL, -- email или phone
    Code VARCHAR(10) NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    IsVerified BOOLEAN DEFAULT FALSE,
    AttemptCount INTEGER DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица логов
CREATE TABLE Logs (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Level VARCHAR(20) NOT NULL, -- Info, Warning, Error, Debug
    Message TEXT NOT NULL,
    Exception TEXT,
    UserId UUID REFERENCES Users(Id),
    RequestPath VARCHAR(500),
    RequestMethod VARCHAR(10),
    IPAddress VARCHAR(45),
    UserAgent TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для производительности
CREATE INDEX IX_Users_Email ON Users(Email) WHERE DeletedAt IS NULL;
CREATE INDEX IX_Users_Phone ON Users(Phone) WHERE DeletedAt IS NULL;
CREATE INDEX IX_Products_Category ON Products(CategoryId);
CREATE INDEX IX_Products_Code ON Products(Code);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Verifications_Contact_Type ON Verifications(Contact, Type);
CREATE INDEX IX_Logs_Level_CreatedAt ON Logs(Level, CreatedAt);
CREATE INDEX IX_Dictionaries_Type ON Dictionaries(Type);

-- Заполняем базовые данные словарей

-- Материалы
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, DescriptionRu, DescriptionKz) VALUES 
('MATERIALS', 'Натуральная кожа', 'Табиғи тері', 'natural_leather', 'Высококачественная натуральная кожа', 'Жоғары сапалы табиғи тері'),
('MATERIALS', 'Экокожа', 'Эко тері', 'eco_leather', 'Искусственная кожа', 'Жасанды тері'),
('MATERIALS', 'Ткань', 'Мата', 'fabric', 'Мебельная ткань', 'Жиһаз матасы'),
('MATERIALS', 'Вельвет', 'Бархат', 'velvet', 'Мягкая бархатистая ткань', 'Жұмсақ бархат мата'),
('MATERIALS', 'Массив дуба', 'Емен ағашы', 'oak_solid', 'Натуральное дерево дуба', 'Табиғи емен ағашы'),
('MATERIALS', 'Массив бука', 'Бук ағашы', 'beech_solid', 'Натуральное дерево бука', 'Табиғи бук ағашы'),
('MATERIALS', 'МДФ', 'МДФ', 'mdf', 'Древесноволокнистая плита', 'Ағаш талшық плитасы'),
('MATERIALS', 'ЛДСП', 'ЛДСП', 'chipboard', 'Ламинированная древесностружечная плита', 'Ламинатталған ағаш үгінді плитасы'),
('MATERIALS', 'Металл', 'Металл', 'metal', 'Металлический каркас', 'Металл каркас'),
('MATERIALS', 'Пластик', 'Пластик', 'plastic', 'Высокопрочный пластик', 'Берік пластик');

-- Цвета
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, Value, DescriptionRu, DescriptionKz) VALUES 
('COLORS', 'Белый', 'Ақ', 'white', '#FFFFFF', 'Белый цвет', 'Ақ түс'),
('COLORS', 'Черный', 'Қара', 'black', '#000000', 'Черный цвет', 'Қара түс'),
('COLORS', 'Серый', 'Сұр', 'gray', '#808080', 'Серый цвет', 'Сұр түс'),
('COLORS', 'Коричневый', 'Қоңыр', 'brown', '#8B4513', 'Коричневый цвет', 'Қоңыр түс'),
('COLORS', 'Бежевый', 'Сүт түсті', 'beige', '#F5F5DC', 'Бежевый цвет', 'Сүт түсті'),
('COLORS', 'Синий', 'Көк', 'blue', '#0000FF', 'Синий цвет', 'Көк түс'),
('COLORS', 'Зеленый', 'Жасыл', 'green', '#008000', 'Зеленый цвет', 'Жасыл түс'),
('COLORS', 'Красный', 'Қызыл', 'red', '#FF0000', 'Красный цвет', 'Қызыл түс'),
('COLORS', 'Желтый', 'Сары', 'yellow', '#FFFF00', 'Желтый цвет', 'Сары түс'),
('COLORS', 'Оранжевый', 'Қызғылт сары', 'orange', '#FFA500', 'Оранжевый цвет', 'Қызғылт сары түс'),
('COLORS', 'Темно-синий', 'Қою көк', 'navy', '#000080', 'Темно-синий цвет', 'Қою көк түс'),
('COLORS', 'Розовый', 'Қызғылт', 'pink', '#FFC0CB', 'Розовый цвет', 'Қызғылт түс');

-- Размеры
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, Value) VALUES 
('SIZES', 'Малый', 'Кішкентай', 'size_s', 'S'),
('SIZES', 'Средний', 'Орташа', 'size_m', 'M'),
('SIZES', 'Большой', 'Үлкен', 'size_l', 'L'),
('SIZES', 'Очень большой', 'Өте үлкен', 'size_xl', 'XL'),
('SIZES', '120x80 см', '120x80 см', 'size_120x80', '120x80'),
('SIZES', '140x80 см', '140x80 см', 'size_140x80', '140x80'),
('SIZES', '160x90 см', '160x90 см', 'size_160x90', '160x90'),
('SIZES', '180x90 см', '180x90 см', 'size_180x90', '180x90'),
('SIZES', '200x100 см', '200x100 см', 'size_200x100', '200x100');

-- Веса
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, Value) VALUES 
('WEIGHTS', 'Легкий (до 10 кг)', 'Жеңіл (10 кг дейін)', 'weight_light', '10'),
('WEIGHTS', 'Средний (10-25 кг)', 'Орташа (10-25 кг)', 'weight_medium', '25'),
('WEIGHTS', 'Тяжелый (25-50 кг)', 'Ауыр (25-50 кг)', 'weight_heavy', '50'),
('WEIGHTS', 'Очень тяжелый (50-100 кг)', 'Өте ауыр (50-100 кг)', 'weight_very_heavy', '100'),
('WEIGHTS', 'Сверхтяжелый (свыше 100 кг)', 'Аса ауыр (100 кг астам)', 'weight_super_heavy', '100+');

-- Главные категории
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, DescriptionRu, DescriptionKz) VALUES 
('CATEGORIES', 'Мягкая мебель', 'Жұмсақ жиһаз', 'soft_furniture', 'Диваны, кресла, пуфы для комфортного отдыха', 'Ыңғайлы демалыс үшін диван, креслолар, пуфтар'),
('CATEGORIES', 'Корпусная мебель', 'Корпусты жиһаз', 'case_furniture', 'Шкафы, комоды, стеллажи для хранения', 'Сақтауға арналған шкафтар, комодтар, сөрелер'),
('CATEGORIES', 'Столы', 'Үстелдер', 'tables', 'Обеденные, журнальные, рабочие столы', 'Ас үстелдері, журнал үстелдері, жұмыс үстелдері'),
('CATEGORIES', 'Стулья', 'Орындықтар', 'chairs', 'Обеденные стулья, барные стулья', 'Ас үстелі орындықтары, бар орындықтары'),
('CATEGORIES', 'Кровати', 'Төсектер', 'beds', 'Односпальные, двуспальные кровати', 'Бір орынды, екі орынды төсектер'),
('CATEGORIES', 'Детская мебель', 'Балалар жиһазы', 'kids_furniture', 'Мебель для детских комнат', 'Балалар бөлмелеріне арналған жиһаз');

-- Подкатегории для мягкой мебели
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, DescriptionRu, DescriptionKz, ParentId) VALUES 
('CATEGORIES', 'Диваны', 'Диван', 'sofas', 'Прямые и угловые диваны', 'Түзу және бұрышты диван', (SELECT id FROM dictionaries WHERE code = 'soft_furniture')),
('CATEGORIES', 'Кресла', 'Креслолар', 'armchairs', 'Кресла для отдыха и работы', 'Демалыс пен жұмысқа арналған креслолар', (SELECT id FROM dictionaries WHERE code = 'soft_furniture')),
('CATEGORIES', 'Пуфы', 'Пуфтар', 'ottomans', 'Пуфы и банкетки', 'Пуфтар және банкеткалар', (SELECT id FROM dictionaries WHERE code = 'soft_furniture')),
('CATEGORIES', 'Кресла-кровати', 'Төсек-креслолар', 'sofa_beds', 'Раскладные кресла', 'Жиналатын креслолар', (SELECT id FROM dictionaries WHERE code = 'soft_furniture'));

-- Подкатегории для столов
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, DescriptionRu, DescriptionKz, ParentId) VALUES
('CATEGORIES', 'Обеденные столы', 'Ас үстелдері', 'dining_tables', 'Столы для кухни и столовой', 'Ас үй мен ас бөлмесіне арналған үстелдер', (SELECT id FROM dictionaries WHERE code = 'tables')),
('CATEGORIES', 'Журнальные столы', 'Журнал үстелдері', 'coffee_tables', 'Столы для гостиной', 'Қонақ бөлмесіне арналған үстелдер', (SELECT id FROM dictionaries WHERE code = 'tables')),
('CATEGORIES', 'Рабочие столы', 'Жұмыс үстелдері', 'work_desks', 'Столы для работы и учебы', 'Жұмыс пен оқуға арналған үстелдер', (SELECT id FROM dictionaries WHERE code = 'tables')),
('CATEGORIES', 'Компьютерные столы', 'Компьютер үстелдері', 'computer_desks', 'Столы для компьютеров', 'Компьютерге арналған үстелдер', (SELECT id FROM dictionaries WHERE code = 'tables'));

-- Подкатегории для корпусной мебели
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, DescriptionRu, DescriptionKz, ParentId) VALUES
('CATEGORIES', 'Шкафы', 'Шкафтар', 'wardrobes', 'Платяные и встроенные шкафы', 'Киім шкафтары және кіріктірілген шкафтар', (SELECT id FROM dictionaries WHERE code = 'case_furniture')),
('CATEGORIES', 'Комоды', 'Комодтар', 'dressers', 'Комоды с ящиками', 'Жәшіктері бар комодтар', (SELECT id FROM dictionaries WHERE code = 'case_furniture')),
('CATEGORIES', 'Стеллажи', 'Сөрелер', 'shelves', 'Открытые стеллажи и книжные полки', 'Ашық сөрелер мен кітап сөрелері', (SELECT id FROM dictionaries WHERE code = 'case_furniture')),
('CATEGORIES', 'Тумбы', 'Тумбалар', 'cabinets', 'ТВ-тумбы, прикроватные тумбы', 'ТВ-тумбалар, төсек жанындағы тумбалар', (SELECT id FROM dictionaries WHERE code = 'case_furniture'));

-- Подкатегории для стульев
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, DescriptionRu, DescriptionKz, ParentId) VALUES
('CATEGORIES', 'Обеденные стулья', 'Ас үстелі орындықтары', 'dining_chairs', 'Стулья для кухни и столовой', 'Ас үй мен ас бөлмесіне арналған орындықтар', (SELECT id FROM dictionaries WHERE code = 'chairs')),
('CATEGORIES', 'Барные стулья', 'Бар орындықтары', 'bar_stools', 'Высокие стулья для барных стоек', 'Бар үстелдеріне арналған биік орындықтар', (SELECT id FROM dictionaries WHERE code = 'chairs')),
('CATEGORIES', 'Офисные кресла', 'Кеңсе креслолары', 'office_chairs', 'Кресла для работы за компьютером', 'Компьютерде жұмыс істеуге арналған креслолар', (SELECT id FROM dictionaries WHERE code = 'chairs'));

-- Подкатегории для кроватей
INSERT INTO Dictionaries (Type, NameRu, NameKz, Code, DescriptionRu, DescriptionKz, ParentId) VALUES
('CATEGORIES', 'Односпальные кровати', 'Бір орынды төсектер', 'single_beds', 'Кровати для одного человека', 'Бір адамға арналған төсектер', (SELECT id FROM dictionaries WHERE code = 'beds')),
('CATEGORIES', 'Двуспальные кровати', 'Екі орынды төсектер', 'double_beds', 'Кровати для двух человек', 'Екі адамға арналған төсектер', (SELECT id FROM dictionaries WHERE code = 'beds')),
('CATEGORIES', 'Детские кроватки', 'Балалар төсектері', 'cribs', 'Кроватки для малышей', 'Сәбилерге арналған төсектер', (SELECT id FROM dictionaries WHERE code = 'beds'));


-- Создаем админа
INSERT INTO Users (FullName, Email, Password, Role, IsEmailVerified)
VALUES ('Admin User', 'admin@imperium.kz', '$2a$11$8F8VjyCzOr7h/jVhV6KsKu3i0C8DnGGZYMGZ5N5C4zDGJ6K7qE.pq', 'Admin', TRUE);
-- Пароль: admin123