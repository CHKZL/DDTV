namespace BiliAccount
{
    internal class Stream4Methods
    {
        #region Public Methods

        public static void CopyTo(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        #endregion Public Methods
    }
}