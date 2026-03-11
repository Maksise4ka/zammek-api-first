# ZAMMEK
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