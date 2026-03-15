namespace ms_validacion_riesgo;

public record RiskControlRequest(Guid ExternalOperationId, string CustomerId, decimal Amount);