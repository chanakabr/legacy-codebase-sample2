namespace ApiObjects.Response
{
    public class ActionResult
    {
        public long Id { get; set; }
        public Status Result { get; set; }

        public ActionResult(long id, Status result)
        {
            Id = id;
            Result = result;
        }
    }
}