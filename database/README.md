# Установка и настройка Liquibase
Для установки и настройки Liquebase необходимо выполнить следующие действия:
1. Скачиваем Liquebase по адресу: http://www.liquibase.org/download/index.html
2. Распаковываем Liquebase в папку. Для Ubuntu лучше всего распаковать в /usr/local/lib/liquibase
3. Качаем jdbc driver (берем 4-ю версию) для MS SQL Server по адресу: https://www.microsoft.com/en-us/download/details.aspx?displaylang=en&id=11774
4. Перемещаем файл sqljdbc4.jar в папку lib, которая находится в папке, в которую распаковали Liquibase
5. Добавляем переменную среды $LIQUIBASE_HOME. Для Windows не описываю, потому что это делается просто. Для Ubuntu нужно выполнить следующие действия:
    - открыть терминал
    - ввести команду sudo nano /etc/environment
    - в открышемся файле добавить строчку LIQUIBASE_HOME="/usr/local/lib/liquibase" перед PATH
    - сохраняем и закрываем файл
6. Переменная среды будет в последствии использоваться для вызова Liquebase

# Запуск команд обновления базы данных

Перед запуском команды обновления базы данных установите и настройке Liquebase, а также убедитесь, что в СУБД создана база данных pawnshop.  
Команды для обновления Базы данных для разных стендов описаны в файлах *.sh для каждого стенда соответственно. Для вызова команды откройте терминал, перейдите в папку database проекта и выполните команду.

/*ВНИМАНИЕ!!! в Linux и OS X в начале команды обязательно добавление ключевого слова sudo*/  
/*Нужно постараться эти команды добавить в одну из команд сборочной машины*/

# Правила наименования файлов changelog'ов
1. changelog master должен именоваться 0-db.changelog-master.xml, для того чтобы файл всегда был вверху списка файлов
2. changelog файл должен именоваться по следующему шаблону yyyy.mm.dd-i-db.changelog.xml, где:
    - yyyy.mm.dd - соответственно год, месяц, день
    - i - номер changelog'а за этот день
3. после создание changelog файла ссылку на него необходимо добавить в 0-db.changelog-master.xml  
`<include file="./changelog/2016-11-11-1-db.changelog.xml"></include> <!-- Добавление таблиц для организации security -->`  
где необходимо обязательно добавить комментарий, в котором описать какой действия или действия производит данный changelog
4. id changeset'а должен формироваться по следующему шаблону changelog_filename-i, где:
    - changelog_filename - имя файла changelog'а, например: 2016-11-11-1-db.changelog
    - i - номер по порядку

Внимание!!! Для пользователей OS X

If the hostname of the connecting client is not set or ends with .local, the connection will fail

To fix it, follow these steps:
 open terminal (/Applications/Utilities/Terminal)
 run scutil --get HostName (case sensitive!)
 if hostname is not set or contains .local, run sudo scutil --set HostName "newname"
 try JDBC Azure SQL Connection again - hostname change is effective immediately