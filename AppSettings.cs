namespace ms_validacion_riesgo;

public class AppSettings
{
    public string KafkaBootstrapServers { get; set; }
    public string TopicRiskEvaluationResponse { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal AccumulatedAmount { get; set; }

}