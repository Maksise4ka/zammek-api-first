# ZAMMEK

## Оглавление
1. [Артефакты API](#артефакты-api-лр2)
2. [Метрики ЛР 3](#метрики-лр-3)
3. [Логи ЛР 4](#логи-лр-4)
4. [Трейсы ЛР 5](#трейсы-лр-5)
5. [CI/CD ЛР 6](#cicd-лр-6)

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
* Квантили размеров транзакций на списание/пополнени кредитных счетов
* Текущая ставка по кредитам (менялась раз в 10 секунд рандомно от 20 до 32)

![image](docs/grafana.png)

## Логи (ЛР 4)
Используемые стек: Grafana + Loki \
Созданы следующие запросы:
1. Изменения процентной ставки по кредитам
![image](docs/logs/loan_changes.png)

2. Количество подозрительных списаний на отрицательный баланс в окне 30s
![image](docs/logs/incorrect_decreases.png)

3. Визуализация для отображения 5 самых частых пользователей, которые пытаются сделать отрицательные списания со счета
```
topk(5,
  sum by (userId) (
    count_over_time(
      {service="zammek"} 
      | json 
      | SourceContext = "Zammek.Services.CreditGrpcService" 
      | Message =~ "Suspicious decrease transaction.*"
      | userId != "" 
      [1h]
    )
  )
)
```
![image](docs/logs/user_ids.png)

## Трейсы (ЛР 5)

Используемый стек: Grafana + Tempo. Также была настроена интеграция с отображением логов по определенным трейсам из
Loki  \
Пример трейса c логами по нему:
![image](docs/traces/trace_sample.png)
Пример поиска трейсов:
1. Поиск трейсов, имеющие grpc статус InvalidArgument для ручки снятия средств с баланса
![image](docs/traces/bad_request.png)
2. Поиск трейсов, в которых commit транзакций занимает больше 1.5 секунды
![image](docs/traces/long_transactions.png)

## CI/CD (ЛР 6)
Добавлено 4 джобы:
1. Build - сборка проекта
2. Tests - запуск тестов (пока их 6 штук). Пример [запуска](https://github.com/Maksise4ka/zammek-api-first/actions/runs/24638175870/job/72037289471#step:6:23)
3. Analyze - проверка code quality ошибок. Пример [ошибки](https://github.com/Maksise4ka/zammek-api-first/actions/runs/24636236666#:~:text=exit%20code%201.-,Analyze%3A%20Zammek/Services/UserGrpcService.cs%23L6,-Parameter%20%27logger%27%20is)
4. Coverage - процент покрытия тестами. Джоба падает если процент < 40%. Пример [отчета](https://github.com/Maksise4ka/zammek-api-first/actions/runs/24638175870/job/72037313605#step:7:24)