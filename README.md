# ZAMMEK

## Оглавление
1. [Артефакты API](#артефакты-api-лр2)
2. [Метрики ЛР 3](#метрики-лр-3)

API для микрокредитной организации, занимающейся продажей долгов на собственной бирже.

Состоит из файлов
```
.
├── README.md
├── Zammek
│   └── Protos
│      ├── credit.proto
│      ├── debt.proto
│      ├── user.proto
│      └── google/type/money.proto

```
Описание:
* [credit.proto](./Zammek/Protos/credit.proto) - API для сервиса кредитов
* [debt.proto](./Zammek/Protos/debt.proto) - API для сервиса долгов
* [user.proto](./Zammek/Protos/user.proto) - API для сервиса управления пользователям
* [money.proto](./Zammek/Protos/google/type/money.proto) - описание стандартного google protobuf типа для передачи денежных значений

Как проверить работоспосность:
1. Иметь установленным dotnet версии 10 и выше
2. Запустить в корне dotnet build 

## Артефакты API (ЛР2)
ServerReflectionInfo:
![image](docs/server_reflection_1.png)
![image](docs/server_reflection_2.png)

## Метрики (ЛР 3)
Используемый стек: Grafana + Prometheus \
Отображались:
* Суммарный RPS
* Изменения баланса счетов пользователей
* Квантили средней транзакции на списание/пополнеие кредитных счетов
* Текущая ставка по кредитам (менялась раз в 10 секунд рандомно от 20 до 32)

![image](docs/grafana.png)