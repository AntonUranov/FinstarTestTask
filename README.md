# Finstar.TaskManagement

Система управления задачами, состоящая из двух сервисов на ASP.NET Core, которые общаются через RabbitMQ. Каждый сервис использует собственный экземпляр PostgreSQL. Трассировка и метрики собираются через Elastic APM.

## Состав решения

| Сервис | Назначение | БД |
|---|---|---|
| `Finstar.TaskManagement.Api` | Основной API управления задачами | `finstar_taskmanagement` |
| `Finstar.TaskManagement.ChangeTracker` | Отслеживание изменений задач | `finstar_change_tracker` |

Инфраструктура:

- **PostgreSQL 17**: по одному инстансу на сервис
- **RabbitMQ 4**: обмен сообщениями между сервисами
- **Elasticsearch + APM Server + Kibana (8.15)**: мониторинг и трассировка

## Структура репозитория

```
.
├── .dockerignore
├── deploy/
│   ├── docker-compose.yml    # вся инфраструктура и оба сервиса
│   └── Dockerfile            # общий multi-stage Dockerfile для обоих проектов
└── src/
    ├── Finstar.TaskManagement.Api/
    └── Finstar.TaskManagement.ChangeTracker/
```

## Требования

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (на Windows с WSL2)
- Git Bash или любой другой терминал
- Минимум 4 ГБ памяти для Docker (Elasticsearch и Kibana требовательны к ресурсам)
- .NET SDK 10.0: только для локальной разработки без Docker

## Быстрый старт

```bash
git clone <repository-url>
cd <repository-folder>/deploy
docker compose up -d --build
```

Первая сборка занимает несколько минут: скачиваются образы и собираются проекты.

Проверка состояния:

```bash
docker compose ps
docker compose logs -f taskmanagement-api
docker compose logs -f taskmanagement-changetracker
```

## Адреса и порты

| Компонент | Адрес | Примечание |
|---|---|---|
| Api | http://localhost:5000 | |
| ChangeTracker | http://localhost:5001 | |
| RabbitMQ UI | http://localhost:15672 | `guest` / `guest` |
| Kibana (APM) | http://localhost:5601 | |
| APM Server | http://localhost:8200 | |
| Elasticsearch | http://localhost:9200 | |
| PostgreSQL (Api) | `localhost:5432` | БД `finstar_taskmanagement` |
| PostgreSQL (ChangeTracker) | `localhost:5433` | БД `finstar_change_tracker` |
| RabbitMQ (AMQP) | `localhost:5672` | |

Учётные данные PostgreSQL для обеих БД: `postgres` / `helloagain`.

Внутри docker-сети оба PostgreSQL слушают порт `5432` и различаются именами хостов (`postgres-taskmanagement`, `postgres-changetracker`). Порт `5433` проброшен на хост только для удобства подключения с локальной машины.

## Конфигурация

В `appsettings.json` обоих проектов указаны значения для локального запуска (`localhost`). В контейнерах они переопределяются переменными окружения в `docker-compose.yml`. Вложенные ключи задаются через двойное подчёркивание:

| Переменная | Описание |
|---|---|
| `ConnectionStrings__TaskManagementContext` | Строка подключения Api |
| `ConnectionStrings__TaskChangesContext` | Строка подключения ChangeTracker |
| `RabbitMqOptions__Host` / `Port` / `VirtualHost` / `Username` / `Password` | Параметры RabbitMQ |
| `ElasticApm__ServerUrl` | Адрес APM Server |
| `ElasticApm__ServiceName` | Имя сервиса в APM |

Для запуска вне Docker достаточно поднять инфраструктуру (PostgreSQL, RabbitMQ, APM Server) на `localhost` с портами из `appsettings.json` и запустить проекты через `dotnet run`.

## Управление окружением

```bash
cd deploy

docker compose up -d --build                         # собрать и запустить всё
docker compose up -d --build taskmanagement-api      # пересобрать один сервис
docker compose logs -f <service>                     # логи сервиса
docker compose restart <service>                     # перезапуск сервиса
docker compose down                                  # остановить, данные сохраняются
docker compose down -v                               # остановить и удалить все данные
```

Имена сервисов: `taskmanagement-api`, `taskmanagement-changetracker`, `postgres-taskmanagement`, `postgres-changetracker`, `rabbitmq`, `elasticsearch`, `kibana`, `apm-server`.

## Проверка баз данных

```bash
docker compose exec postgres-taskmanagement psql -U postgres -c "\l"
docker compose exec postgres-changetracker psql -U postgres -c "\l"
```

## Миграции

Docker Compose создаёт только пустые базы данных. Если сервисы не применяют миграции EF Core при старте, примените их вручную с хоста (инфраструктура должна быть запущена):

```bash
# Api
dotnet ef database update --project src/Finstar.TaskManagement.Api \
  --connection "Host=localhost;Port=5432;Database=finstar_taskmanagement;Username=postgres;Password=helloagain"

# ChangeTracker
dotnet ef database update --project src/Finstar.TaskManagement.ChangeTracker \
  --connection "Host=localhost;Port=5433;Database=finstar_change_tracker;Username=postgres;Password=helloagain"
```

Если миграции лежат в отдельном проекте, добавьте `--startup-project` с путём к соответствующему сервису.

## Мониторинг

1. Откройте Kibana: http://localhost:5601
2. Перейдите в раздел **Observability, APM, Services**
3. Сервисы `Finstar.TaskManagement.Api` и `Finstar.TaskManagement.ChangeTracker` появятся после первых запросов

## Решение проблем

| Симптом | Возможная причина и решение |
|---|---|
| Elasticsearch перезапускается или падает | Не хватает памяти: увеличьте лимит в Docker Desktop (Settings, Resources) минимум до 4 ГБ |
| Сборка падает на `restore` | Проверьте, что пути `src/<Project>/<Project>.csproj` совпадают с реальной структурой |
| Образ `dotnet/sdk:10.0` не найден | Измените `DOTNET_VERSION` в `deploy/Dockerfile` на актуальную версию |
| `ACCESS_REFUSED` при подключении к RabbitMQ | Задайте пользователя через `RABBITMQ_DEFAULT_USER` / `RABBITMQ_DEFAULT_PASS` и укажите его в `RabbitMqOptions__*` |
| Порт уже занят | Измените левую часть проброса порта в `docker-compose.yml` (например, `5002:8080`) |
| Таблицы в БД отсутствуют | Примените миграции, см. раздел «Миграции» |

## Безопасность

Учётные данные в `docker-compose.yml` указаны открытым текстом и предназначены только для локальной разработки. Для любых других окружений вынесите секреты в `.env` или в менеджер секретов, а в Elasticsearch и APM Server включите аутентификацию.
