# Koshel

## Запуск приложения

Для запуска приложения вам потребуется выполнить следующие шаги:

1. Установите Docker и Docker Compose на ваш компьютер.
2. Измените значение переменной `HOST` в файле `.env` на текущий ваш IP адрес.
3. Выполните команду `docker compose up` в корневом каталоге проекта.

После выполнения этих шагов, вы сможете открыть веб-интерфейс клиента на странице http://localhost:5053.

## Веб-API

Web MVC клиент для тестирования: http://localhost:5053
Веб-API доступно по адресу http://localhost:5003.
Swagger для просмотра и тестирования API на странице http://localhost:5003/swagger/index.html.

## Технологии и инструменты

- **ASP.NET Core** - это платформа для создания веб-приложений на языке C#.
- **EF Core** - это объектно-реляционная система управления базами данных, разработанная Microsoft.
- **PostgreSQL** - это открытая система управления базами данных.
- **Docker** - это платформа для контейнерного развертывания приложений.
- **Docker Compose** - это инструмент для определения и запуска многоконтейнерных приложений.
- **Swagger** - это инструмент для создания и документирования RESTful API.
- **SignalR** - это библиотека для реализации двусторонних взаимодействий в режиме реального времени в веб-приложениях.
- **Serilog** - это библиотека для ведения журнала в приложениях .NET.

## Дополнительная информация

P.S.
Думал реализовать передачу сообщений через web socket через **RabbitMQ**, но в тестовом задании не было про него ни слова.
Также не было сказано, в момент подключения к web socket'у нужно ли предоставлять прошлые сообщения и в каком количестве.

Поэтому я реализовал прием только тех сообщений, которые отослались уже после подключения приемника (Consumer) к web socket.
И поэтому я и не увидел смысла громоздить здесь **RabbitMQ**. Если будет уточнение по заданию, я переделаю его через подключение к **RabbitMQ**.
