using Phoenix.Generated.Api.Events.Logical.HouseholdRecordingMigrationStatus;

namespace Phoenix.AsyncHandler.Kafka
{
    public interface IHouseholdRecordingMigrationPublisher
    {
        void Publish(HouseholdRecordingMigrationStatus status);
    }
}
