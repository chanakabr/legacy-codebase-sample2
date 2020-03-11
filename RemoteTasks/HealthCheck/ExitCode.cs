namespace HealthCheck
{
	public enum ExitCode : int
	{
		Success = 0,
		RabbitError = 100,
		MSSQLError = 200,
		CBError = 300,
		ESError = 400,

	}
}
