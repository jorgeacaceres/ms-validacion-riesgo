using DotNetCore.CAP;
using Microsoft.Extensions.Options;
using ms_validacion_riesgo;
using System.Collections.Concurrent;


public class RiskEvaluationListener : ICapSubscribe
{
    private readonly ILogger<RiskEvaluationListener> _logger;
    private readonly ICapPublisher _capPublisher;
    private readonly AppSettings _config;
    private readonly ConcurrentDictionary<string, (DateTime Date, decimal Total)> _dailyAccumulated = new();

    public RiskEvaluationListener(ILogger<RiskEvaluationListener> logger,
                                  ICapPublisher capPublisher,
                                  IOptions<AppSettings> config)
    {
        _logger = logger;
        _capPublisher = capPublisher;
        _config = config.Value;
    }

    [CapSubscribe("risk-evaluation-request")]
    public async Task HandleRiskRequest(RiskControlRequest @event)
    {
        try
        {
            _logger.LogInformation("Inicia validación de riesgo {@request}", @event);
            string status = "accepted";
            if (@event.Amount > _config.BaseAmount)
            {
                status = "denied";
                _logger.LogWarning("Monto {Amount} excede base {BaseAmount}.", @event.Amount, _config.BaseAmount);
            }
            else
            {
                var today = DateTime.Today;
                var entry = _dailyAccumulated.GetOrAdd(@event.CustomerId, _ => (today, new Random().Next(0, 5001)));
                if (entry.Date != today)
                {
                    _logger.LogInformation("Reseteando acumulado diario para customer={CustomerId} del día {OldDate} a {Today}.", @event.CustomerId, entry.Date, today);
                    entry = (today, new Random().Next(0, 5001));
                    _dailyAccumulated[@event.CustomerId] = entry;
                }
                _logger.LogInformation("Estado previo acumulado para customer={CustomerId}: {Total} + nuevo={Amount} (limite={AccumulatedAmount})",
                                 @event.CustomerId, entry.Total, @event.Amount, _config.AccumulatedAmount);
                if (entry.Total + @event.Amount > _config.AccumulatedAmount)
                {
                    status = "denied";
                    _logger.LogWarning("Acumulado diario {Total} + {Amount} supera el limite {AccumulatedAmount}.",
                                       entry.Total, @event.Amount, _config.AccumulatedAmount);
                }
                else
                {
                    entry.Total += @event.Amount;
                    _dailyAccumulated[@event.CustomerId] = entry;
                    _logger.LogInformation("Validación exitosa, nuevo acumulado diario para customer={CustomerId} es {Total}.",
                                            @event.CustomerId, entry.Total);
                }
            }
            var result = new RiskControlResponse(@event.ExternalOperationId, status);
            _logger.LogInformation("Enviando respuesta validación {topico} {@result}", _config.TopicRiskEvaluationResponse, result);
            await _capPublisher.PublishAsync(_config.TopicRiskEvaluationResponse, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrio una excepción al validar el riesgo");
        }
    }
}