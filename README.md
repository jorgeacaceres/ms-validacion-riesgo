# ms-validacion-riesgo

Microservicio .NET 8 orientado a eventos para la evaluación de riesgo transaccional, utilizando una arquitectura de consumidor/productor con Kafka.

## 1. Tipo de proyecto

- .NET 8 (Target framework `net8.0`)
- Worker Service / Consumidor de mensajes basado en CAP.
- Arquitectura orientada a eventos:
  - `Listeners`: suscripción a tópicos y recepción de eventos.

## 2. Tecnologías principales y dependencias

- C# 12 / .NET 8
- DotNetCore.CAP (integración con Kafka para patrón Outbox/Inbox)
- Serilog (Console + Seq)
- Microsoft.Extensions.Options (para configuración de límites)

## 3. Conexiones y configuración

### 3.1 Reglas de Negocio

`appsettings.json`:

```json
"RiskSettings": {
  "BaseAmount": 5000.0,
  "AccumulatedAmount": 20000.0
}
```

### 3.2 Kafka + CAP

appsettings.json:

```json
"Configuration": {
  "KafkaBootstrapServers": "localhost:9092",
  "TopicRiskEvaluationResponse": "risk-evaluation-response"
}
```

- Escucha el tópico risk-evaluation-request para procesar solicitudes de evaluación.
- Publica el resultado en el tópico definido en TopicRiskEvaluationResponse mediante ICapPublisher.

### 3.3 Logging

- Serilog configurado con consola y Seq

### 4. Arquitectura / flujo

- Recepción: El servicio detecta un evento RiskControlRequest en el tópico risk-evaluation-request.
- Evaluación de Riesgo:
  - Valida el monto individual contra el parámetro BaseAmount.
  - Valida el acumulado diario del cliente contra AccumulatedAmount usando un diccionario interno.
  - Persistencia: Se actualiza el diccionario concurrente de acumulados si la transacción es aceptada.
  - Respuesta: Publica un evento RiskControlResponse con el estado final (accepted o denied) en Kafka.

### 5. Requisitos de instalación

- .NET 8 SDK
- Kafka y Zookeeper según versión
- Seq

## 8. Ejecución local

1. Clonar repo.
2. Ajustar `appsettings.json` conexiones (Postgres/Kafka/Seq).
3. Ejecutar:
   - `dotnet run`

## 9. Validaciones y control de errores

- Lógica de Riesgo: Marcado automático como denied si se superan los límites configurados en RiskSettings.
- Resiliencia: CAP asegura que la respuesta sea entregada al broker mediante reintentos automáticos en caso de desconexión.
- Trazabilidad: Cada evaluación genera logs detallados en Seq para facilitar el monitoreo de operaciones rechazadas.
