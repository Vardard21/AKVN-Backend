namespace AKVN_Backend.Classes.DTO
{
    public class Response<o>
    {
        public bool Success { get; set; }
        public List<int> ErrorMessage { get; set; } = new List<int>();
        public o Data { get; set; }

        public void ServerError()
        {
            Success = false;
            ErrorMessage.Add(1);
        }
        public void RequestError()
        {
            Success = false;
            ErrorMessage.Add(2);
        }
        public void InvalidOperation()
        {
            Success = false;
            ErrorMessage.Add(3);
        }
        public void InvalidData()
        {
            Success = false;
            ErrorMessage.Add(4);
        }
        public void AuthorizationError()
        {
            Success = false;
            ErrorMessage.Add(5);
        }
    }
}
